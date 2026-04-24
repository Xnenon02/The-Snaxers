// ===================================================
// main.bicep — Container Registry, Container Apps Environment, Container App
// ===================================================
// Usage: az deployment group create --resource-group The-Snaxers --template-file infra/main.bicep --parameters environmentName=dev

@description('Environment name: dev, staging or prod')
param environmentName string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

@description('Container image to deploy, e.g. acrsnaxersdev.azurecr.io/thesnaxers:latest')
param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('Managed Identity resource ID — from security.bicep output')
param managedIdentityId string

@description('Managed Identity client ID — from security.bicep output')
param managedIdentityClientId string

@description('Key Vault URI — from security.bicep output')
param keyVaultUri string

@description('Log Analytics workspace ID — from monitoring.bicep output')
param logAnalyticsWorkspaceId string

@description('Application Insights connection string — from monitoring.bicep output')
param appInsightsConnectionString string = ''

// ===================================================
// AZURE CONTAINER REGISTRY (ACR)
// ===================================================
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'acrsnaxers${environmentName}'
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    adminUserEnabled: false
    policies: {
      retentionPolicy: {
        // Rensa otaggade images efter 7 dagar (dev) eller 30 dagar (prod)
        days: environmentName == 'prod' ? 30 : 7
        status: 'enabled'
      }
    }
  }
}

// ===================================================
// RBAC — Ge Managed Identity rätt att läsa från ACR
// ===================================================
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, managedIdentityId, acrPullRoleId)
  scope: acr
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: reference(managedIdentityId, '2023-01-31').principalId
    principalType: 'ServicePrincipal'
  }
}

// ===================================================
// CONTAINER APPS ENVIRONMENT
// ===================================================
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'cae-snaxers-${environmentName}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(logAnalyticsWorkspaceId, '2023-09-01').customerId
        sharedKey: listKeys(logAnalyticsWorkspaceId, '2023-09-01').primarySharedKey
      }
    }
  }
}

// ===================================================
// CONTAINER APP
// ===================================================
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'ca-snaxers-${environmentName}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'thesnaxers'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'prod' ? 'Production' : environmentName == 'staging' ? 'Staging' : 'Development'
            }
            {
              name: 'KeyVault__Url'
              value: keyVaultUri
            }
            {
              // TODO: Ersätt med parameter från database.bicep när US5 är klar (Martina)
              name: 'CosmosDb__AccountEndpoint'
              value: 'https://snaxers.documents.azure.com:443/'
            }
            {
              name: 'CosmosDb__DatabaseName'
              value: environmentName == 'prod' ? 'TheSnaxersDb' : 'TheSnaxersDb-${environmentName}'
            }
            {
              name: 'CosmosDb__ContainerName'
              value: 'Products'
            }
            {
              name: 'CosmosDb__FavoritesContainerName'
              value: 'Favorites'
            }
            {
              name: 'ApplicationInsights__ConnectionString'
              value: appInsightsConnectionString
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentityClientId
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

// ===================================================
// OUTPUTS
// ===================================================
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output acrLoginServer string = acr.properties.loginServer
output containerAppEnvironmentId string = containerAppsEnvironment.id