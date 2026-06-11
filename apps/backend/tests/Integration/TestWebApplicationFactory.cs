using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Stocker.Core.Clients;
using Stocker.Core.Settings;
using Stocker.Infrastructure.Database;
using Testcontainers.Minio;
using Testcontainers.MsSql;
using Minio;
using Minio.DataModel.Args;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;

namespace Stocker.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private IHost? _host;

  public IServiceProvider ServerServices
  {
    get
    {
      if (_host == null)
        throw new InvalidOperationException("The host has not been initialized yet.");

      return _host.Services;
    }
  }

  public HttpClient KestrelClient { get; private set; } = null!;

  private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithPassword("YourStrong@Passw0rd")
    .Build();

  private readonly MinioContainer _minioContainer = new MinioBuilder()
    .WithUsername("minioadmin")
    .WithPassword("minioadmin")
    .Build();

  public ITradingViewClient MockTradingViewClient { get; } = Substitute.For<ITradingViewClient>();
  public HttpClient Client { get; private set; } = default!;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      // Replace TradingViewClient with mock
      var tradingViewDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITradingViewClient));
      if (tradingViewDescriptor != null)
      {
        services.Remove(tradingViewDescriptor);
      }
      services.AddSingleton(MockTradingViewClient);

      // Replace DbContext with Testcontainers SQL Server
      var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StockerDataContext>));
      if (dbContextDescriptor != null)
      {
        services.Remove(dbContextDescriptor);
      }

      services.AddDbContext<StockerDataContext>(options =>
      {
        options.UseSqlServer(_dbContainer.GetConnectionString());
        options.EnableSensitiveDataLogging();
      });

      // Replace Redis distributed cache with in-memory cache for tests
      var redisCacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
      if (redisCacheDescriptor != null)
      {
        services.Remove(redisCacheDescriptor);
      }
      services.AddDistributedMemoryCache();

      // Override MinioOptions with container connection
      services.Configure<MinioSettings>(options =>
      {
        options.Endpoint = _minioContainer.GetConnectionString();
        options.AccessKey = "minioadmin";
        options.SecretKey = "minioadmin";
        options.PublicBucket = "stocker-public-bucket";
        options.PrivateBucket = "stocker-private-bucket";
      });

      //1. Completely remove your production Options configuration block
      var descriptors = services.Where(d =>
          d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>) ||
          d.ServiceType == typeof(IPostConfigureOptions<JwtBearerOptions>)).ToList();

      foreach (var descriptor in descriptors)
      {
        services.Remove(descriptor);
      }

      // 2. Add your pure, clean testing options configuration directly
      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      });
      // Configure JWT bearer to validate against the test signing key
      services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
      {
        // CRITICAL: Clear out Authority metadata endpoints so the test runner
        // doesn't attempt to connect to a real internet identity server
        options.Authority = null;
        options.MetadataAddress = null;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidIssuer = TokenTestingExtensions.Issuer,
          ValidAudience = TokenTestingExtensions.Audience,
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = false,
          IssuerSigningKey = TokenTestingExtensions.TestSecurityKey,
        };
      });

      // Remove rate limiting for tests
      var rateLimiterDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>));
      if (rateLimiterDescriptor is not null)
      {
        services.Remove(rateLimiterDescriptor);
      }
      // 3. Register a wide-open dummy/noop configuration so the middleware doesn't crash
      services.Configure<RateLimiterOptions>(options =>
      {
        // Leave options blank so no restrictions or policies apply
      });
    });
  }



  // Custom helper method to safely build your client without factory.CreateClient()
  public HttpClient CreateKestrelClient()
  {
    if (KestrelClient is not null)
    {
      return KestrelClient;
    }
    if (_host == null)
    {
      throw new InvalidOperationException("The host has not been initialized yet.");
    }

    // 1. Extract the real address feature from the running Kestrel server instance
    var server = _host.Services.GetRequiredService<IServer>();
    var addressesFeature = server.Features.Get<IServerAddressesFeature>();
    var baseAddress = addressesFeature?.Addresses.FirstOrDefault();

    if (string.IsNullOrEmpty(baseAddress))
    {
      throw new InvalidOperationException("Could not retrieve the dynamic Kestrel binding address.");
    }

    // 2. Instantiate a pure HttpClient pointing directly to the TCP socket address
    return KestrelClient = new HttpClient
    {
      BaseAddress = new Uri(baseAddress)
    }.AuthenticateAs("test-user-id", ["read:stocks"]);
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    // Only remove specific hosted services that interfere with tests (e.g. MinIO bucket setup, TickerQ).
    // Do NOT remove all IHostedService entries — that would also remove Kestrel (the web server).
    builder.ConfigureServices(services =>
    {
      var toRemove = services.Where(d =>
        d.ServiceType == typeof(BackgroundService) ||
        (d.ServiceType == typeof(IHostedService) &&
         d.ImplementationType?.Name?.Contains("TickerQ") == true)).ToList();

      foreach (var descriptor in toRemove)
      {
        services.Remove(descriptor);
      }
    });

    builder.ConfigureWebHost(webBuilder =>
        {
          webBuilder.UseKestrel(options =>
          {
            // Fix: Bind directly to the IPv4 loopback address on an anonymous dynamic port
            options.Listen(IPAddress.Loopback, 0);
          });
        });

    _host = base.CreateHost(builder);

    // Ensure database schema is created and migrations applied
    using var scope = _host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StockerDataContext>();
    db.Database.Migrate();

    _host.Start();
    return _host;
  }

  public async ValueTask InitializeAsync()
  {
    // Start both containers concurrently
    await Task.WhenAll(_dbContainer.StartAsync(), _minioContainer.StartAsync());

    // Create test MinIO buckets
    try
    {
      var endpoint = _minioContainer.GetConnectionString();
      var minio = new MinioClientFactory(options =>
      {
        options.WithEndpoint(endpoint)
        .WithCredentials("minioadmin", "minioadmin")
        .WithSSL(false)
        .Build();
      }).CreateClient();

      var publicBucket = "stocker-public-bucket";
      var mbArgs = new MakeBucketArgs()
          .WithBucket(publicBucket);
      await minio.MakeBucketAsync(mbArgs);
      string publicReadPolicy = $$"""
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": { "AWS": ["*"] },
                "Action": [ "s3:GetObject" ],
                "Resource": [ "arn:aws:s3:::{{publicBucket}}/*" ]
            }
        ]
    }
    """;

      // 3. Apply the policy to the MinIO bucket
      await minio.SetPolicyAsync(new SetPolicyArgs()
          .WithBucket(publicBucket)
          .WithPolicy(publicReadPolicy));
    }
    catch (Exception ex)
    {
      // Bucket creation might fail if buckets already exist, that's ok for tests
      Console.WriteLine($"Note: MinIO bucket setup: {ex.Message}");
    }

    try
    {
      _ = Server;
    }
    catch (InvalidCastException)
    {
      // We intentionally swallow the cast exception here because by the time 
      // the cast fails, CreateHost() has already executed completely,
      // your migrations have run, and Kestrel is fully running!
    }
  }

  public new async Task DisposeAsync()
  {
    _host?.Dispose();
    KestrelClient?.Dispose();
    await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _minioContainer.DisposeAsync().AsTask());
    GC.SuppressFinalize(this);
  }
}
