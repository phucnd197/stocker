using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Stocker.Core.Settings;
using Stocker.Infrastructure.Database;
using Stocker.Features.Stock.StockRanking;
using Testcontainers.Minio;
using Testcontainers.MsSql;
using Microsoft.IdentityModel.JsonWebTokens;
using Minio;
using Minio.DataModel.Args;

namespace Stocker.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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

      // Override MinioOptions with container connection
      services.Configure<MinioSettings>(options =>
      {
        options.Endpoint = _minioContainer.GetConnectionString();
        options.AccessKey = "minioadmin";
        options.SecretKey = "minioadmin";
        options.PublicBucket = "stocker-public-bucket";
        options.PrivateBucket = "stocker-private-bucket";
      });

      // Configure fake JWT bearer authentication with RSA256 support
      services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidIssuer = "https://test-auth0-domain.com/",
          ValidAudience = "https://your-api-audience.com",
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
        };

        // For testing, we'll bypass signature validation but still validate structure
        options.TokenValidationParameters.SignatureValidator = (token, parameters) =>
        {
          var jwt = new JsonWebToken(token);
          // Basic validation - check token structure and claims
          return jwt;
        };

        // Require signed tokens (even though we're bypassing the actual signature check)
        options.TokenValidationParameters.RequireSignedTokens = true;
      });

      // Remove rate limiting for tests
      var rateLimiterDescriptors = services.Where(d =>
        d.ServiceType?.Name?.Contains("RateLimiter") == true ||
        d.ServiceType?.Name?.Contains("RateLimit") == true).ToList();
      foreach (var descriptor in rateLimiterDescriptors)
      {
        services.Remove(descriptor);
      }
    });
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    // Remove hosted services that might interfere with tests (like MinIO bucket creation)
    builder.ConfigureServices(services =>
    {
      var hostedServiceDescriptors = services.Where(d =>
        d.ServiceType == typeof(IHostedService) ||
        d.ServiceType == typeof(BackgroundService)).ToList();
      foreach (var descriptor in hostedServiceDescriptors)
      {
        services.Remove(descriptor);
      }
    });

    return base.CreateHost(builder);
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

    // Create HTTP client
    Client = CreateClient();

    // Ensure database schema is created and migrations applied
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StockerDataContext>();
    await db.Database.MigrateAsync();
  }

  public new async Task DisposeAsync()
  {
    await Task.WhenAll(_dbContainer.DisposeAsync().AsTask(), _minioContainer.DisposeAsync().AsTask());
    GC.SuppressFinalize(this);
  }
}
