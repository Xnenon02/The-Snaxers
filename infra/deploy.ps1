# ===================================================
# deploy.ps1 — Orchestrator för Bicep-deployment
# SNAX-1: Kör alla tre Bicep-steg i rätt ordning och
#         skickar outputs automatiskt som parametrar.
# ===================================================
# Användning:
#   .\infra\deploy.ps1 -ResourceGroup rg-snaxers-dev -EnvironmentName dev
#   .\infra\deploy.ps1 -ResourceGroup rg-snaxers-prod -EnvironmentName prod -ContainerImage acrsnaxersprod.azurecr.io/thesnaxers:latest
# ===================================================

param(
    [Parameter(Mandatory)]
    [string]$ResourceGroup,

    [Parameter(Mandatory)]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$EnvironmentName,

    [string]$Location = 'swedencentral',

    [string]$ContainerImage = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest',

    [string]$CosmosAccountEndpoint = 'https://snaxers.documents.azure.com:443/',

    [string]$BlobStorageEndpoint = 'https://sasnaxersdev.blob.core.windows.net/'
)

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " The Snaxers — Bicep Deployment" -ForegroundColor Cyan
Write-Host " Miljö : $EnvironmentName" -ForegroundColor Cyan
Write-Host " RG    : $ResourceGroup" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# ===================================================
# STEG 1 — security.bicep
# Skapar: Managed Identity + Key Vault + RBAC
# ===================================================
Write-Host "[1/3] Deploying security.bicep..." -ForegroundColor Yellow

$securityResult = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file "$PSScriptRoot\security.bicep" `
    --parameters environmentName=$EnvironmentName location=$Location `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if (-not $?) { Write-Host "ERROR: security.bicep misslyckades." -ForegroundColor Red; exit 1 }

$managedIdentityId       = $securityResult.managedIdentityId.value
$managedIdentityClientId = $securityResult.managedIdentityClientId.value
$keyVaultUri             = $securityResult.keyVaultUri.value
$keyVaultName            = $securityResult.keyVaultName.value

Write-Host "  OK  Managed Identity : $managedIdentityId" -ForegroundColor Green
Write-Host "  OK  Key Vault URI    : $keyVaultUri" -ForegroundColor Green

# ===================================================
# STEG 2 — monitoring.bicep
# Skapar: Log Analytics + Application Insights
# ===================================================
Write-Host ""
Write-Host "[2/3] Deploying monitoring.bicep..." -ForegroundColor Yellow

$monitoringResult = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file "$PSScriptRoot\monitoring.bicep" `
    --parameters environmentName=$EnvironmentName location=$Location `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if (-not $?) { Write-Host "ERROR: monitoring.bicep misslyckades." -ForegroundColor Red; exit 1 }

$logAnalyticsId             = $monitoringResult.logAnalyticsId.value
$appInsightsConnectionString = $monitoringResult.appInsightsConnectionString.value

Write-Host "  OK  Log Analytics ID         : $logAnalyticsId" -ForegroundColor Green
Write-Host "  OK  App Insights conn string : $($appInsightsConnectionString.Substring(0, 40))..." -ForegroundColor Green

# ===================================================
# STEG 3 — main.bicep
# Skapar: ACR + Container Apps Environment + Container App
# Tar emot outputs från steg 1 och 2 som parametrar
# ===================================================
Write-Host ""
Write-Host "[3/3] Deploying main.bicep..." -ForegroundColor Yellow

$mainResult = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file "$PSScriptRoot\main.bicep" `
    --parameters `
        environmentName=$EnvironmentName `
        location=$Location `
        containerImage=$ContainerImage `
        managedIdentityId=$managedIdentityId `
        managedIdentityClientId=$managedIdentityClientId `
        keyVaultUri=$keyVaultUri `
        logAnalyticsWorkspaceId=$logAnalyticsId `
        appInsightsConnectionString=$appInsightsConnectionString `
        cosmosAccountEndpoint=$CosmosAccountEndpoint `
        blobStorageEndpoint=$BlobStorageEndpoint `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if (-not $?) { Write-Host "ERROR: main.bicep misslyckades." -ForegroundColor Red; exit 1 }

$containerAppUrl   = $mainResult.containerAppUrl.value
$acrLoginServer    = $mainResult.acrLoginServer.value

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " Deployment klar!" -ForegroundColor Green
Write-Host " App URL    : $containerAppUrl" -ForegroundColor Green
Write-Host " ACR server : $acrLoginServer" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
