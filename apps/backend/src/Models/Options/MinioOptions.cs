namespace Stocker.Models.Options;

public record MinioOptions(string Endpoint, string AccessKey, string SecretKey, string PublicBucket, string PrivateBucket);