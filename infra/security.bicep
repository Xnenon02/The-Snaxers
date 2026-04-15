// ===================================================
// security.bicep — Key Vault + Managed Identity + RBAC
// ===================================================
// Usage: az deployment group create --resource-group <rg> --template-file infra/security.bicep --parameters environmentName=dev

@description('Environment name: dev, staging or prod')
param environmentName string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

@description('Name of the existing Container App (for Managed Identity binding)')
param containerAppName string

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
// RBAC — Ge Managed Identity läsrättighet till Key Vault
// ===================================================
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e0'

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, managedIdentity.id, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ===================================================
// OUTPUTS — Används av Container App och monitoring.bicep
// ===================================================
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
