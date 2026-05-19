# Infrastructure as Code — The Snaxers

Bicep-mallar för Azure-infrastruktur. Stöder tre miljöer: `dev`, `staging`, `prod`.

## Filer

| Fil | Beskrivning |
|-----|-------------|
| `main.bicep` | ACR (med retention policy), Container Apps Environment, Container App |
| `security.bicep` | Key Vault, User-Assigned Managed Identity, RBAC |
| `monitoring.bicep` | Log Analytics Workspace, Application Insights |
| `parameters/dev.json` | Parametrar för dev-miljö |
| `parameters/staging.json` | Parametrar för staging-miljö |
| `parameters/prod.json` | Parametrar för prod-miljö |

---

## Skillnader per miljö

| Resurs | dev | staging | prod |
|--------|-----|---------|------|
| ACR SKU | Basic | Basic | Standard |
| ACR retention | 7 dagar | 7 dagar | 30 dagar |
| Container App replicas | 1–3 | 1–3 | 1–3 |
| CosmosDB databas | TheSnaxersDb-dev | TheSnaxersDb-staging | TheSnaxersDb |
| ASPNETCORE_ENVIRONMENT | Development | Staging | Production |

---

## Förutsättningar

- Azure CLI installerat och inloggat (`az login`)
- Befintlig resursgrupp per miljö
- Contributor-åtkomst till resursgruppen

---

## Deployment — körs i denna ordning

### 1. Security (Managed Identity + Key Vault)

```powershell
az deployment group create --resource-group rg-snaxers-dev --template-file infra/security.bicep --parameters @infra/parameters/dev.json
```

### 2. Monitoring (Log Analytics + Application Insights)

```powershell
az deployment group create --resource-group rg-snaxers-dev --template-file infra/monitoring.bicep --parameters @infra/parameters/dev.json
```

### 3. Main (ACR + Container Apps)

```powershell
az deployment group create --resource-group rg-snaxers-dev --template-file infra/main.bicep --parameters @infra/parameters/dev.json managedIdentityId="<id-från-steg-1>" managedIdentityClientId="<clientId-från-steg-1>" keyVaultUri="<uri-från-steg-1>" logAnalyticsWorkspaceId="<id-från-steg-2>"
```

Byt ut `dev` mot `staging` eller `prod` för andra miljöer.

---

## Efter deployment

| Uppgift | Ansvarig |
|---------|----------|
| Lägga in App Insights connection string i Key Vault | Tomek |
| Lägga in Google OAuth ClientId/ClientSecret i Key Vault | Tomek |
| Lägga in Blob Storage connection string i Key Vault | Martina |
