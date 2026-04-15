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