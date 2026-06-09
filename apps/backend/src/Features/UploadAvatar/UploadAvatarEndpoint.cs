using FastEndpoints;
using Minio;
using Minio.DataModel.Args;

namespace Stocker.Features.StockRanking;


public class UploadAvatarEndpoint : EndpointWithoutRequest
{
  private static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif"];
  private readonly IMinioClient _minioClient;

  public UploadAvatarEndpoint(IMinioClient minioClient)
  {
    _minioClient = minioClient;
  }

  public override void Configure()
  {
    Post("/api/profile/upload-avatar");
    AllowAnonymous();
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
    // Make a bucket on the server, if not already present.
    var beArgs = new BucketExistsArgs()
        .WithBucket("stocker-public");
    bool found = await _minioClient.BucketExistsAsync(beArgs, ct).ConfigureAwait(false);
    if (!found)
    {
      var mbArgs = new MakeBucketArgs()
          .WithBucket("stocker-public");
      await _minioClient.MakeBucketAsync(mbArgs, ct).ConfigureAwait(false);
    }

    using var stream = file.OpenReadStream();
    var objectArgs = new PutObjectArgs().WithBucket("stocker-public").WithObject(objectKey).WithStreamData(stream);
    await _minioClient.PutObjectAsync(objectArgs, ct);

    // Return the key back to React immediately
    await Send.OkAsync(new { imageKey = objectKey }, ct);
  }
}
