using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using Stocker.Database;

namespace Stocker.Features.UserProfile.GetUserProfile;

public class UserProfileResponse
{
  public string Email { get; set; }
  public string Image { get; set; }
  public string Nickname { get; set; }
  public string Phone { get; set; }
  public string Address { get; set; }

  public static UserProfileResponse FromEntity(Entities.UserProfile entity)
  {
    return new UserProfileResponse
    {
      Image = entity.Image,
      Nickname = entity.Nickname,
      Phone = entity.Phone,
      Address = entity.Address,
    };
  }
}

public class GetUserProfile : EndpointWithoutRequest<UserProfileResponse>
{
  private readonly StockerDataContext _context;

  public GetUserProfile(StockerDataContext context)
  {
    _context = context;
  }

  public override void Configure()
  {
    Get("/api/profile");
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    if (!Guid.TryParse(User.Identity?.Name, out var userId))
    {
      AddError("Can't find user Id");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    var entity = await _context.UserProfiles.FindAsync([new[] { userId }], cancellationToken: ct);
    var response = entity is null ? new UserProfileResponse() : UserProfileResponse.FromEntity(entity);

    await Send.OkAsync(response, ct);
  }
}