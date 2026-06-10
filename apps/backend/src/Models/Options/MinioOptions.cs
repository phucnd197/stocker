namespace Stocker.Models.Options;

public class MinioOptions
{
  public string Endpoint { get; set; }
  public string AccessKey { get; set; }
  public string SecretKey { get; set; }
  public string PublicBucket { get; set; }
  public string PrivateBucket { get; set; }
}