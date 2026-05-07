using Azure.Storage.Blobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TheSnaxers.Services;

public class BlobHealthCheck : IHealthCheck
{
    private readonly BlobServiceClient _client;
    private readonly string _containerName;

    public BlobHealthCheck(BlobServiceClient client, IConfiguration config)
    {
        _client = client;
        _containerName = config["AzureStorage:ProductContainerName"] ?? "products";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Use a 5-second timeout so a hung Blob call does not freeze the readiness endpoint
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            var containerClient = _client.GetBlobContainerClient(_containerName);
            var response = await containerClient.ExistsAsync(cancellationToken: cts.Token);
            return response.Value
                ? HealthCheckResult.Healthy("Blob Storage reachable")
                : HealthCheckResult.Unhealthy("Blob container missing");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Blob Storage unreachable", ex);
        }
    }
}