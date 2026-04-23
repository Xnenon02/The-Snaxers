// ===================================================
// security.bicep — Key Vault + Managed Identity + RBAC
// ===================================================
// Usage: az deployment group create --resource-group <rg> --template-file infra/security.bicep --parameters environmentName=dev

@description('Environment name: dev, staging or prod')
param environmentName string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

// ===================================================
// USER-ASSIGNED MANAGED IDENTITY
// ===================================================
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-snaxers-${environmentName}'
  location: location
}

// ===================================================
// KEY VAULT
// ===================================================
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-snaxers-${environmentName}'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true  // Managed Identity via RBAC, inga access policies
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// ===================================================
// NOTE: RBAC-roller (Key Vault Secrets User för Managed Identity) tilldelas separat av Hanita
// ===================================================
// OUTPUTS — Används av Container App och monitoring.bicep
// ===================================================
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
