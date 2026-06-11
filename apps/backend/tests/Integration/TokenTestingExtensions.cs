using System.Net.Http.Headers;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Stocker.Tests.Integration;

public static class TokenTestingExtensions
{
  public static HttpClient AuthenticateAs(this HttpClient client, string userId, string[] permissions)
  {
    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, userId),
      new Claim("sub", userId),
      new Claim("iss", "https://test-auth0-domain.com/"),
      new Claim("aud", "https://your-api-audience.com"),
      new Claim(ClaimTypes.Name, "Test User"),
      new Claim(ClaimTypes.Email, "test@example.com")
    };

    // If your Auth0 setup puts permissions into a custom array claim
    foreach (var permission in permissions)
    {
      claims.Add(new Claim("permissions", permission));
    }

    // Create RSA key pair for asymmetric encryption (RS256 like Auth0 uses)
    var token = tokenHandler.CreateJwtSecurityToken(
      issuer: "https://test-auth0-domain.com/",
      audience: "https://your-api-audience.com",
      subject: new ClaimsIdentity(claims),
      expires: DateTime.UtcNow.AddHours(1),
      signingCredentials: null
    );

    var jwtString = tokenHandler.WriteToken(token);

    // Inject the bearer token into the HTTP Client headers
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", jwtString);

    return client;
  }
}

