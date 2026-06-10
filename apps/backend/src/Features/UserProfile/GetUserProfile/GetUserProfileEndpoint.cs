using System.Security.Claims;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stocker.Database;
using Stocker.Models.Options;

namespace Stocker.Features.UserProfile.GetUserProfile;

public class UserProfileResponse
{
  public string Image { get; set; }
  public string? AvatarUrl { get; set; }
  public string Nickname { get; set; }
  public string Phone { get; set; }
  public string Address { get; set; }

  public static UserProfileResponse FromEntity(Entities.UserProfile entity, MinioOptions minioOptions)
  {
    var avatarUrl = string.IsNullOrEmpty(entity.Image)
      ? ""
      : $"http://{minioOptions.Endpoint}/{minioOptions.PublicBucket}/{entity.Image}";

    return new UserProfileResponse
    {
      Image = entity.Image,
      AvatarUrl = avatarUrl,
      Nickname = entity.Nickname,
      Phone = entity.Phone,
      Address = entity.Address,
    };
  }
}

public class GetUserProfile : EndpointWithoutRequest<UserProfileResponse>
{
  private readonly StockerDataContext _context;
  private readonly MinioOptions _minioOptions;

  public GetUserProfile(StockerDataContext context, IOptions<MinioOptions> minioOptions)
  {
    _context = context;
    _minioOptions = minioOptions.Value;
  }

  public override void Configure()
  {
    Get("/api/profile");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(sub))
    {
      AddError("Can't find user Id");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    var entity = await _context.UserProfiles.FirstOrDefaultAsync(x => x.Auth0Sub == sub, cancellationToken: ct);
    var response = entity is null ? new UserProfileResponse() : UserProfileResponse.FromEntity(entity, _minioOptions);

    await Send.OkAsync(response, ct);
  }
}