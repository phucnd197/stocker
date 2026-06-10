namespace Stocker.Models.Options;

public class MinioOptions
{
  public string Endpoint { get; init; }
  public string AccessKey { get; init; }
  public string SecretKey { get; init; }
  public string PublicBucket { get; init; }
  public string PrivateBucket { get; init; }
}