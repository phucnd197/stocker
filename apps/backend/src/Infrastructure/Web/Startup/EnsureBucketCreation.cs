using Minio;
using Minio.DataModel.Args;
using Stocker.Core.Settings;

namespace Stocker.Infrastructure.Web.Startup;

public static class EnsureBucketCreation
{
  public static async Task RunAsync(MinioSettings settings, IServiceProvider serviceProvider)
  {
    // Make a bucket on the server, if not already present.
    var _minioClient = serviceProvider.GetRequiredService<IMinioClient>();
    var beArgs = new BucketExistsArgs()
        .WithBucket(settings.PublicBucket);

    var found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
    if (!found)
    {
      var mbArgs = new MakeBucketArgs()
          .WithBucket(settings.PublicBucket);
      await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
      // 2. Define a Read-Only JSON Policy Statement for public anonymous access
      string publicReadPolicy = $$"""
    {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": { "AWS": ["*"] },
                "Action": [ "s3:GetObject" ],
                "Resource": [ "arn:aws:s3:::{{settings.PublicBucket}}/*" ]
            }
        ]
    }
    """;

      // 3. Apply the policy to the MinIO bucket
      await _minioClient.SetPolicyAsync(new SetPolicyArgs()
          .WithBucket(settings.PublicBucket)
          .WithPolicy(publicReadPolicy));
    }
  }
}