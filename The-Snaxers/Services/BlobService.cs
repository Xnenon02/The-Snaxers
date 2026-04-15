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
        // 1. Hämta en referens till containern
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        // 2. Skapa containern om den inte finns (bra säkerhetsåtgärd)
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // 3. Skapa en unik referens för filen (vi kan lägga till ett Guid för att undvika namnkrockar)
        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        // 4. Ladda upp filen
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/jpeg" });

        // 5. Returnera URL:en till den nya bilden
        return blobClient.Uri.ToString();
    }
}