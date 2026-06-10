using Minio;
using Minio.DataModel.Args;
using Stocker.Models.Options;

namespace Stocker.Helpers;

public static class EnsureBucketCreation
{
  public static async Task RunAsync(MinioOptions options, IServiceProvider serviceProvider)
  {
    // Make a bucket on the server, if not already present.
    var _minioClient = serviceProvider.GetRequiredService<IMinioClient>();
    var beArgs = new BucketExistsArgs()
        .WithBucket(options.PublicBucket);

    var found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
    if (!found)
    {
      var mbArgs = new MakeBucketArgs()
          .WithBucket(options.PublicBucket);
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
                "Resource": [ "arn:aws:s3:::{{options.PublicBucket}}/*" ]
            }
        ]
    }
    """;

      // 3. Apply the policy to the MinIO bucket
      await _minioClient.SetPolicyAsync(new SetPolicyArgs()
          .WithBucket(options.PublicBucket)
          .WithPolicy(publicReadPolicy));
    }
  }
}