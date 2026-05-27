using Microsoft.AspNetCore.Http;

namespace TheSnaxers.Services;

public interface IImageValidationService
{
    string? ValidateImageFile(IFormFile file);
}

