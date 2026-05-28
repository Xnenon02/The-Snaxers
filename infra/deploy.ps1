# ===================================================
# deploy.ps1 — Orchestrator för Bicep-deployment
# SNAX-1: Kör alla tre Bicep-steg i rätt ordning och
#         skickar outputs automatiskt som parametrar.
# ===================================================
# Användning:
#   .\infra\deploy.ps1 -ResourceGroup rg-snaxers-dev -EnvironmentName dev `
#       -CosmosAccountEndpoint https://snaxers.documents.azure.com:443/ `
#       -BlobStorageEndpoint https://sasnaxersdev.blob.core.windows.net/
#
#   # Prod med eget storage-konto:
#   .\infra\deploy.ps1 -ResourceGroup rg-snaxers-prod -EnvironmentName prod `
#       -ContainerImage acrsnaxersprod.azurecr.io/thesnaxers:latest `
#       -CosmosAccountEndpoint https://snaxers.documents.azure.com:443/ `
#       -BlobStorageEndpoint https://sasnaxersprod.blob.core.windows.net/
#
#   # Prod som delar dev storage-konto (BlobStorageResourceGroup skiljer sig):
#   .\infra\deploy.ps1 -ResourceGroup rg-snaxers-prod -EnvironmentName prod `
#       -CosmosAccountEndpoint https://snaxers.documents.azure.com:443/ `
#       -BlobStorageEndpoint https://sasnaxersdev.blob.core.windows.net/ `
#       -BlobStorageAccountName sasnaxersdev `
#       -BlobStorageResourceGroup rg-snaxers-dev
# ===================================================

param(
    [Parameter(Mandatory)]
    [string]$ResourceGroup,

    [Parameter(Mandatory)]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$EnvironmentName,

    [string]$Location = 'swedencentral',

    [string]$ContainerImage = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest',

    # P1-fix: Inga default-värden för endpoints — måste anges explicit
    # för att undvika att prod råkar pekas mot dev-resurser
    [Parameter(Mandatory)]
    [string]$CosmosAccountEndpoint,

    [Parameter(Mandatory)]
    [string]$BlobStorageEndpoint,

    # Blob Storage-kontonamn — används för att aktivera anonym åtkomst
    # Default: sasnaxers<miljö> (t.ex. sasnaxersdev / sasnaxersprod)
    [string]$BlobStorageAccountName = "sasnaxers$EnvironmentName",

    # Resursgrupp för Blob Storage-kontot.
    # Om du delar ett storage-konto från en annan miljö (t.ex. prod som använder sasnaxersdev),
    # ange den RG som kontot tillhör — steg 3 körs då mot rätt RG och Bicep hoppar
    # över container-skapandet (kontot är redan konfigurerat).
    # Default: samma RG som deployments-targeten.
    [string]$BlobStorageResourceGroup = $ResourceGroup
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
Write-Host "[1/4] Deploying security.bicep..." -ForegroundColor Yellow

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
Write-Host "[2/4] Deploying monitoring.bicep..." -ForegroundColor Yellow

$monitoringResult = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file "$PSScriptRoot\monitoring.bicep" `
    --parameters environmentName=$EnvironmentName location=$Location `
    --query properties.outputs `
    --output json | ConvertFrom-Json

if (-not $?) { Write-Host "ERROR: monitoring.bicep misslyckades." -ForegroundColor Red; exit 1 }

$logAnalyticsId             = $monitoringResult.logAnalyticsId.value
$appInsightsConnectionString = $monitoringResult.appInsightsConnectionString.value

Write-Host "  OK  Log Analytics ID  : $logAnalyticsId" -ForegroundColor Green
Write-Host "  OK  App Insights      : connection string captured" -ForegroundColor Green

# ===================================================
# STEG 3 — Blob Storage: aktivera anonym åtkomst på kontonivå
# Måste köras INNAN main.bicep eftersom Bicep sätter publicAccess: Blob
# på containern — det kräver att kontot tillåter public access först.
# Hoppas över om storage-kontot tillhör en annan RG (redan konfigurerat).
# ===================================================
Write-Host ""
Write-Host "[3/4] Configuring Blob Storage public access..." -ForegroundColor Yellow

$deployBlobContainer = 'true'

if ($BlobStorageResourceGroup -ne $ResourceGroup) {
    Write-Host "  SKIP  '$BlobStorageAccountName' tillhör '$BlobStorageResourceGroup' — already configured, skipping" -ForegroundColor Gray
    $deployBlobContainer = 'false'
} else {
    az storage account update `
        --name $BlobStorageAccountName `
        --resource-group $BlobStorageResourceGroup `
        --allow-blob-public-access true `
        --output none

    if (-not $?) { Write-Host "ERROR: Blob Storage public access misslyckades." -ForegroundColor Red; exit 1 }

    Write-Host "  OK  Anonymous blob access enabled on '$BlobStorageAccountName'" -ForegroundColor Green
}

# ===================================================
# STEG 4 — main.bicep
# Skapar: ACR + Container Apps Environment + Container App
# Tar emot outputs från steg 1 och 2 som parametrar
# ===================================================
Write-Host ""
Write-Host "[4/4] Deploying main.bicep..." -ForegroundColor Yellow

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
        blobStorageAccountName=$BlobStorageAccountName `
        deployBlobContainer=$deployBlobContainer `
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
Write-Host ""
Write-Host " Manuellt kvar:" -ForegroundColor Yellow
Write-Host "   Kopiera bilder från snaxerschocolateblob till $BlobStorageAccountName/products" -ForegroundColor Yellow
