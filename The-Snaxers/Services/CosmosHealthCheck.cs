using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TheSnaxers.Services;

public class CosmosHealthCheck : IHealthCheck
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;
    private readonly string _containerName;

    public CosmosHealthCheck(CosmosClient client, IConfiguration config)
    {
        _client = client;
        _databaseName = config["CosmosDb:DatabaseName"]
            ?? throw new InvalidOperationException("CosmosDb:DatabaseName saknas.");
        _containerName = config["CosmosDb:ContainerName"]
            ?? throw new InvalidOperationException("CosmosDb:ContainerName saknas.");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Use a 5-second timeout so a hung Cosmos call does not freeze the readiness endpoint
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            var container = _client.GetContainer(_databaseName, _containerName);
            await container.ReadContainerAsync(cancellationToken: cts.Token);
            return HealthCheckResult.Healthy("Cosmos DB reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB unreachable", ex);
        }
    }
}