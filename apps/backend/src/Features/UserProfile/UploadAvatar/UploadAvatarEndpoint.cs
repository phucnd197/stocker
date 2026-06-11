using FastEndpoints;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Stocker.Core.Settings;

namespace Stocker.Features.UserProfile.UploadAvatar;

public record UploadAvatarResponse(string ImageKey);

public class UploadAvatarEndpoint : EndpointWithoutRequest<UploadAvatarResponse>
{
  private static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp"];
  private readonly IMinioClient _minioClient;
  private readonly MinioSettings _settings;

  public UploadAvatarEndpoint(IMinioClient minioClient, IOptions<MinioSettings> options)
  {
    _minioClient = minioClient;
    _settings = options.Value;
  }

  public override void Configure()
  {
    Post("/api/profile/upload-avatar");
    MaxRequestBodySize(5 * 1024 * 1024);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var file = Files[0];
    if (file is null || !file.ContentType.StartsWith("image/"))
    {
      AddError("Invalid image file.");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }
    var extension = Path.GetExtension(file.FileName).ToLower();

    if (!allowedExtensions.Contains(extension))
    {
      AddError("File extensions not allowed.");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }
    var objectKey = $"avatars/{Guid.NewGuid()}_{file.FileName}";

    using var stream = file.OpenReadStream();
    var objectArgs = new PutObjectArgs().WithBucket(_settings.PublicBucket).WithObject(objectKey).WithStreamData(stream).WithContentType(file.ContentType).WithObjectSize(file.Length);
    await _minioClient.PutObjectAsync(objectArgs, ct);

    // Return the key back to React immediately
    await Send.OkAsync(new UploadAvatarResponse(objectKey), ct);
  }
}
