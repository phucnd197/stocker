using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Stocker.Tests.Integration;

public class TestAuthenticationOptions : AuthenticationSchemeOptions
{
  public string? TestUserId { get; set; } = "test-user-id";
}

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationOptions>
{
  public TestAuthenticationHandler(
    IOptionsMonitor<TestAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
  {
  }

  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    // Create test claims principal
    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, Options.TestUserId ?? "test-user-id"),
      new Claim("sub", Options.TestUserId ?? "test-user-id"),
      new Claim(ClaimTypes.Name, "Test User"),
      new Claim(ClaimTypes.Email, "test@example.com")
    };

    var identity = new ClaimsIdentity(claims, Scheme.Name);
    var principal = new ClaimsPrincipal(identity);

    var ticket = new AuthenticationTicket(principal, Scheme.Name);

    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}
