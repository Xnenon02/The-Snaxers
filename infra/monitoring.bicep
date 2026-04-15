// ===================================================
// monitoring.bicep — Log Analytics + Application Insights
// ===================================================
// Usage: az deployment group create --resource-group <rg> --template-file infra/monitoring.bicep --parameters environmentName=dev

@description('Environment name: dev, staging or prod')
param environmentName string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

// ===================================================
// LOG ANALYTICS WORKSPACE
// ===================================================
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-snaxers-${environmentName}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ===================================================
// APPLICATION INSIGHTS
// ===================================================
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-snaxers-${environmentName}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id  // Kopplad till Log Analytics
    RetentionInDays: 30
  }
}

// ===================================================
// OUTPUTS — Connection string läggs in i Key Vault av deploy-processen
// ===================================================
output appInsightsName string = appInsights.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output logAnalyticsName string = logAnalytics.name
output logAnalyticsId string = logAnalytics.id
