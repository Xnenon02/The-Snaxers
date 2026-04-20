using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace TheSnaxers.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration["AzureStorage:ProductContainerName"] ?? "products";
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        if (fileStream.CanSeek)
            fileStream.Position = 0;

        var contentType = fileName.ToLower().EndsWith(".png") ? "image/png" :
                         fileName.ToLower().EndsWith(".gif") ? "image/gif" : "image/jpeg";

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(fileStream, options);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return;

        // Extrahera blob-namnet från URL:en
        // URL-format: https://<account>.blob.core.windows.net/<container>/<blobname>
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return;

        // Kontrollera att URL:en tillhör vår Blob Storage (inte en lokal /images/-sökväg)
        if (!uri.Host.EndsWith(".blob.core.windows.net"))
            return;

        var blobName = Path.GetFileName(uri.LocalPath);
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}