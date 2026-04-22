namespace TheSnaxers.Services;

public interface IBlobService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName);
    Task DeleteImageAsync(string imageUrl);
}