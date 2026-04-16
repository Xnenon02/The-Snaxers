using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace TheSnaxers.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName; // Inte längre const, eftersom den sätts i konstruktorn

    public BlobService(IConfiguration configuration)
    {
        // 1. Hämta anslutningssträngen
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);

        // 2. Hämta containernamnet dynamiskt (matchar nu Toms Bicep-mall via config)
        // Vi hämtar den från sektionen "AzureStorage:ProductContainerName"
        _containerName = configuration["AzureStorage:ProductContainerName"] ?? "products";
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
    {
        // Använd det dynamiska namnet här
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        // Bra att ha kvar: skapar containern om den inte finns (t.ex. vid lokal utveckling)
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // Skapa ett unikt namn
        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        // Säkerställ att vi börjar läsa filen från början
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        // Detektera filtyp
        var contentType = fileName.ToLower().EndsWith(".png") ? "image/png" : 
                         fileName.ToLower().EndsWith(".gif") ? "image/gif" : "image/jpeg";

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        // Ladda upp
        await blobClient.UploadAsync(fileStream, options);

        // Returnera den färdiga URL-strängen till Repositoryt
        return blobClient.Uri.ToString();
    }
}