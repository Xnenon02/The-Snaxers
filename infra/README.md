# Infrastructure as Code — The Snaxers

Bicep-mallar för Azure-infrastruktur.

## Filer

| Fil | Beskrivning |
|-----|-------------|
| `main.bicep` | Azure Container Registry (ACR), Container Apps Environment, Container App |
| `security.bicep` | Key Vault, User-Assigned Managed Identity |
| `monitoring.bicep` | Log Analytics Workspace, Application Insights |

---

## Förutsättningar

- Azure CLI installerat och inloggat (`az login`)
- En befintlig resursgrupp (t.ex. `rg-snaxers-dev`)
- Contributor-åtkomst till resursgruppen

---

## Deployment — körs i denna ordning

### 1. Security (Managed Identity + Key Vault)

```bash
az deployment group create \
  --resource-group rg-snaxers-dev \
  --template-file infra/security.bicep \
  --parameters environmentName=dev
```

### 2. Monitoring (Log Analytics + Application Insights)

```bash
az deployment group create \
  --resource-group rg-snaxers-dev \
  --template-file infra/monitoring.bicep \
  --parameters environmentName=dev
```

### 3. Main (ACR + Container Apps)

Kräver outputs från steg 1 och 2:

```bash
az deployment group create \
  --resource-group rg-snaxers-dev \
  --template-file infra/main.bicep \
  --parameters environmentName=dev \
               managedIdentityId=<id-från-steg-1> \
               managedIdentityClientId=<clientId-från-steg-1> \
               keyVaultUri=<uri-från-steg-1> \
               logAnalyticsWorkspaceId=<id-från-steg-2>
```

---

## Efter deployment — ansvar per person

| Uppgift | Ansvarig |
|---------|----------|
| Tilldela RBAC-roller till Managed Identity (AcrPull, Key Vault Secrets User) | Hanita |
| Lägga in Application Insights connection string i Key Vault | Hanita |
| Lägga in Blob Storage connection string i Key Vault | Martina |
| Koppla App Insights mot appen | Hanita |

---

## Miljöer

Byt `environmentName` mot `dev`, `staging` eller `prod` beroende på målmiljö.
