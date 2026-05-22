# ===================================================
# setup-oidc.ps1 — Skapar GitHub OIDC Federated Credentials
# SNAX-5: Ersätter manuellt skapande i Azure Portal
# ===================================================
# Kör en gång per miljö/branch vid initial setup.
# Kräver att du är inloggad med: az login
# ===================================================
# Användning:
#   .\infra\setup-oidc.ps1
# ===================================================

$ErrorActionPreference = 'Stop'

# App Registration Object ID (inte client ID — se Azure Portal > App registrations)
$appObjectId = "183cedb9-3ad7-4a13-9c1e-bffcdcc2dbb6"
$org         = "School-Be-Fun-They-said"
$repo        = "The-Snaxers"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " The Snaxers — OIDC Federated Credentials" -ForegroundColor Cyan
Write-Host " App Registration : $appObjectId" -ForegroundColor Cyan
Write-Host " Repo             : $org/$repo" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Hämta befintliga credential-namn för idempotens-kontroll (P2-fix)
Write-Host "Hämtar befintliga federated credentials..." -ForegroundColor Yellow
$existing = az ad app federated-credential list --id $appObjectId --output json | ConvertFrom-Json
$existingNames = $existing | ForEach-Object { $_.name }
Write-Host "  Befintliga: $($existingNames -join ', ')" -ForegroundColor Gray
Write-Host ""

# Hjälpfunktion — skapar credential om det inte redan finns
function Set-FederatedCredential {
    param(
        [string]$Name,
        [string]$Subject,
        [string]$Description
    )
    if ($existingNames -contains $Name) {
        Write-Host "  SKIP  $Name (finns redan)" -ForegroundColor Gray
        return
    }
    az ad app federated-credential create `
        --id $appObjectId `
        --parameters "{
            `"name`": `"$Name`",
            `"issuer`": `"https://token.actions.githubusercontent.com`",
            `"subject`": `"$Subject`",
            `"description`": `"$Description`",
            `"audiences`": [`"api://AzureADTokenExchange`"]
        }" | Out-Null
    Write-Host "  OK    $Name" -ForegroundColor Green
}

# ===================================================
# develop-branchen
# ===================================================
Write-Host "Kontrollerar credential for 'develop'..." -ForegroundColor Yellow
Set-FederatedCredential `
    -Name "github-actions-develop" `
    -Subject "repo:$org/$($repo):ref:refs/heads/develop" `
    -Description "GitHub Actions OIDC for develop branch"

# ===================================================
# main-branchen
# ===================================================
Write-Host "Kontrollerar credential for 'main'..." -ForegroundColor Yellow
Set-FederatedCredential `
    -Name "github-actions-main" `
    -Subject "repo:$org/$($repo):ref:refs/heads/main" `
    -Description "GitHub Actions OIDC for main branch"

# ===================================================
# Pull Requests (valfritt — behövs om CI kör vid PR mot main/develop)
# ===================================================
Write-Host "Kontrollerar credential for 'pull_request'..." -ForegroundColor Yellow
Set-FederatedCredential `
    -Name "github-actions-pr" `
    -Subject "repo:$org/$($repo):pull_request" `
    -Description "GitHub Actions OIDC for pull requests"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " OIDC-setup klar!" -ForegroundColor Green
Write-Host " Verifiera i Azure Portal:" -ForegroundColor Green
Write-Host " App registrations > $appObjectId > Certificates & secrets > Federated credentials" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
