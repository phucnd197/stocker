using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FastEndpoints;
using FluentValidation;
using Stocker.Database;

namespace Stocker.Features.UserProfile.UpsertUserProfile;


public record UpsertUserProfileRequest(string Image, string Nickname, string Phone, string Address)
{
  public Entities.UserProfile ToEntity()
  {
    return new Entities.UserProfile
    {
      Image = Image,
      Nickname = Nickname,
      Phone = Phone,
      Address = Address,
    };
  }
}

public static partial class AppRegexPatterns
{
  // 1. Nickname Pattern
  [GeneratedRegex("""^[\p{L}0-9_-]$""", RegexOptions.IgnoreCase)]
  public static partial Regex Nickname();

  // 2. Address Pattern
  [GeneratedRegex("""^[\p{L}0-9\s,.\#\-\/]$""")]
  public static partial Regex Address();

  // 3. Flexible Phone Pattern
  [GeneratedRegex("""^\+?[0-9\s.\-\(\)]$""")]
  public static partial Regex Phone();

  [GeneratedRegex(
        """^(?!.*\.{2,})[a-zA-Z0-9_\-\/]+(?:\.(?i)(jpg|jpeg|png|gif|svg))$""",
        RegexOptions.Compiled
    )]
  public static partial Regex ImageKey();
}

public class UpsertUserProfileRequestValidator : Validator<UpsertUserProfileRequest>
{
  public UpsertUserProfileRequestValidator()
  {
    RuleFor(x => x.Address).MaximumLength(500).Matches(AppRegexPatterns.Address());
    RuleFor(x => x.Image).MaximumLength(200).Matches(AppRegexPatterns.ImageKey());
    RuleFor(x => x.Nickname).MaximumLength(100).Matches(AppRegexPatterns.Nickname());
    RuleFor(x => x.Phone).MaximumLength(20).Matches(AppRegexPatterns.Phone());
  }
}

public class UpsertUserProfileEndpoint : Endpoint<UpsertUserProfileRequest>
{
  private readonly StockerDataContext _context;

  public UpsertUserProfileEndpoint(StockerDataContext context)
  {
    _context = context;
  }

  public override void Configure()
  {
    Post("/api/profile");
  }

  public override async Task HandleAsync(UpsertUserProfileRequest request, CancellationToken ct)
  {
    if (!Guid.TryParse(User.Identity?.Name, out var userId))
    {
      AddError("User infor not found");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }
    var profile = await _context.UserProfiles.FindAsync([new[] { userId }], cancellationToken: ct);
    if (profile is null)
    {
      profile = request.ToEntity();
      await _context.UserProfiles.AddAsync(profile, ct);
    }
    else
    {
      profile.Image = request.Image;
      profile.Nickname = request.Nickname;
      profile.Address = request.Address;
      profile.Phone = request.Phone;
    }

    await _context.SaveChangesAsync(cancellationToken: ct);
  }
}
