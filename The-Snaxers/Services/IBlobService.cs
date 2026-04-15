namespace TheSnaxers.Services;

public interface IBlobService
{
    // Vi tar emot en Stream (filen) och filnamnet, och skickar tillbaka URL:en till bilden
    Task<string> UploadImageAsync(Stream fileStream, string fileName);
}