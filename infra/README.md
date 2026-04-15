# Infrastructure as Code — The Snaxers

This folder contains Bicep templates for Azure infrastructure deployment.

## Files

| File | Description |
|------|-------------|
| `security.bicep` | Key Vault, User-Assigned Managed Identity, RBAC role assignments |
| `monitoring.bicep` | Log Analytics Workspace, Application Insights |

## Prerequisites

- Azure CLI installed and logged in (`az login`)
- An existing Resource Group (created by Tom's main infrastructure)
- Contributor access to the Resource Group

## Deployment

### Security (Key Vault + Managed Identity)

```bash
az deployment group create \
  --resource-group rg-snaxers-dev \
  --template-file infra/security.bicep \
  --parameters environmentName=dev containerAppName=ca-snaxers-dev
```

### Monitoring (Log Analytics + Application Insights)

```bash
az deployment group create \
  --resource-group rg-snaxers-dev \
  --template-file infra/monitoring.bicep \
  --parameters environmentName=dev
```

## Environments

Replace `environmentName` with `dev`, `staging` or `prod` depending on target environment.

## After Deployment

Once deployed, add the Application Insights connection string to Key Vault:

```bash
az keyvault secret set \
  --vault-name kv-snaxers-dev \
  --name "ApplicationInsights--ConnectionString" \
  --value "<connection-string-from-output>"
```

The Container App must also be configured with:
- `KeyVault__Url` environment variable pointing to the Key Vault URI
- The Managed Identity assigned to the Container App