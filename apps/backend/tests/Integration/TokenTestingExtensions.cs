using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Stocker.Tests.Integration;

public static class TokenTestingExtensions
{
  private static readonly SymmetricSecurityKey _testKey =
    new(Encoding.UTF8.GetBytes("test-secret-key-that-is-at-least-32-bytes-long-for-hmac"));

  public static readonly SigningCredentials TestSigningCredentials =
    new(_testKey, SecurityAlgorithms.HmacSha256);

  public static readonly string Issuer = "https://test-auth0-domain.com/";

  public static readonly string Audience = "https://your-api-audience.com";

  public static readonly SecurityKey TestSecurityKey = _testKey;

  public static HttpClient AuthenticateAs(this HttpClient client, string userId, string[] permissions)
  {
    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
      new Claim(ClaimTypes.NameIdentifier, userId),
      new Claim("sub", userId),
      new Claim("iss", Issuer),
      new Claim("aud", Audience),
      new Claim(ClaimTypes.Name, "Test User"),
      new Claim(ClaimTypes.Email, "test@example.com")
    };

    foreach (var permission in permissions)
    {
      claims.Add(new Claim("permissions", permission));
    }

    var token = tokenHandler.CreateJwtSecurityToken(
      issuer: Issuer,
      audience: Audience,
      subject: new ClaimsIdentity(claims),
      expires: DateTime.UtcNow.AddHours(1),
      signingCredentials: TestSigningCredentials
    );

    var jwtString = tokenHandler.WriteToken(token);

    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", jwtString);

    return client;
  }
}
