using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TheSnaxers.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "products"; // Samma namn som du skapade i Azure

    public BlobService(IConfiguration configuration)
    {
        // Vi hämtar ConnectionString från appsettings.json
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
{
    var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

    // Skapa ett unikt namn
    var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
    var blobClient = containerClient.GetBlobClient(uniqueFileName);

    // Säkerställ att vi börjar läsa filen från början
    if (fileStream.CanSeek)
    {
        fileStream.Position = 0;
    }

    // Detektera filtyp baserat på ändelse (enklare än att gissa!)
    var contentType = fileName.ToLower().EndsWith(".png") ? "image/png" : 
                     fileName.ToLower().EndsWith(".gif") ? "image/gif" : "image/jpeg";

    var options = new BlobUploadOptions
    {
        HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
    };

    // Ladda upp
    await blobClient.UploadAsync(fileStream, options);

    return blobClient.Uri.ToString();
}
}