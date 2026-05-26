using Microsoft.AspNetCore.Http;

namespace TheSnaxers.Services;

public interface IImageValidationService
{
    string? ValidateImageFile(IFormFile file);
}

public class ImageValidationService : IImageValidationService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly byte[][] ImageMagicBytes =
    [
        [0xFF, 0xD8, 0xFF],       // JPEG
        [0x89, 0x50, 0x4E, 0x47], // PNG
        [0x52, 0x49, 0x46, 0x46], // WEBP (RIFF header)
    ];
    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    public string? ValidateImageFile(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            return "Filen är för stor. Max 2 MB tillåts.";

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return "Otillåtet filformat. Endast .jpg, .png och .webp tillåts.";

        using var stream = file.OpenReadStream();
        var header = new byte[4];
        var bytesRead = stream.Read(header, 0, header.Length);

        if (bytesRead < 3)
            return "Filen verkar inte vara en giltig bild.";

        var isValidImage = ImageMagicBytes.Any(magic =>
            header.Take(magic.Length).SequenceEqual(magic));

        if (!isValidImage)
            return "Filen verkar inte vara en giltig bild.";

        return null;
    }
}