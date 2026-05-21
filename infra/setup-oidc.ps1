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

# Lista befintliga credentials för att undvika dubletter
Write-Host "Befintliga federated credentials:" -ForegroundColor Yellow
az ad app federated-credential list --id $appObjectId --output table
Write-Host ""

# ===================================================
# develop-branchen
# ===================================================
Write-Host "Skapar credential for 'develop'..." -ForegroundColor Yellow

az ad app federated-credential create `
    --id $appObjectId `
    --parameters "{
        `"name`": `"github-actions-develop`",
        `"issuer`": `"https://token.actions.githubusercontent.com`",
        `"subject`": `"repo:$org/$($repo):ref:refs/heads/develop`",
        `"description`": `"GitHub Actions OIDC for develop branch`",
        `"audiences`": [`"api://AzureADTokenExchange`"]
    }"

Write-Host "  OK  develop" -ForegroundColor Green

# ===================================================
# main-branchen
# ===================================================
Write-Host "Skapar credential for 'main'..." -ForegroundColor Yellow

az ad app federated-credential create `
    --id $appObjectId `
    --parameters "{
        `"name`": `"github-actions-main`",
        `"issuer`": `"https://token.actions.githubusercontent.com`",
        `"subject`": `"repo:$org/$($repo):ref:refs/heads/main`",
        `"description`": `"GitHub Actions OIDC for main branch`",
        `"audiences`": [`"api://AzureADTokenExchange`"]
    }"

Write-Host "  OK  main" -ForegroundColor Green

# ===================================================
# Pull Requests (valfritt — behövs om CI kör vid PR mot main/develop)
# ===================================================
Write-Host "Skapar credential for pull_request..." -ForegroundColor Yellow

az ad app federated-credential create `
    --id $appObjectId `
    --parameters "{
        `"name`": `"github-actions-pr`",
        `"issuer`": `"https://token.actions.githubusercontent.com`",
        `"subject`": `"repo:$org/$($repo):pull_request`",
        `"description`": `"GitHub Actions OIDC for pull requests`",
        `"audiences`": [`"api://AzureADTokenExchange`"]
    }"

Write-Host "  OK  pull_request" -ForegroundColor Green

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host " OIDC-setup klar!" -ForegroundColor Green
Write-Host " Verifiera i Azure Portal:" -ForegroundColor Green
Write-Host " App registrations > $appObjectId > Certificates & secrets > Federated credentials" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
