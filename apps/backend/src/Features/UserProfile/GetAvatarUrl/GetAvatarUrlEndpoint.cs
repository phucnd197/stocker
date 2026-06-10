using FastEndpoints;
using Stocker.Models.Options;

namespace Stocker.Features.UserProfile.GetAvatarUrl;


public class GetAvatarUrlEndpoint : Endpoint<string>
{
  private readonly MinioOptions _minioOptions;

  public GetAvatarUrlEndpoint(MinioOptions minioOptions)
  {
    _minioOptions = minioOptions;
  }

  public override void Configure()
  {
    Get("/api/profile/avatar-url");
  }

  public override async Task HandleAsync(string req, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(req))
    {
      AddError("No key");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    await Send.OkAsync(new { Url = $"{_minioOptions.Endpoint}/{_minioOptions.PublicBucket}/{req}" }, ct);
  }
}