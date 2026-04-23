# 🐳 Docker — The Snaxers

## Bygga imagen
```bash
docker build -t thesnaxers .
```

## Taggningsstrategi
Vi använder två taggar per release:
- `v1.0.0` — specifik version (ökas vid varje release)
- `latest` — pekar alltid på senaste stabila versionen

```bash
docker tag thesnaxers thesnaxers:v1.0.0
docker tag thesnaxers thesnaxers:latest
```

## Köra lokalt med Docker Compose

### 1. Sätt upp miljövariabler
Kopiera `.env.example` till `.env` och fyll i värdena (hämta från teamet via Discord):
```bash
cp docker/.env.example docker/.env
```

### 2. Sätt upp User Secrets
Kör dessa kommandon i `The-Snaxers/`-mappen (hämta värden från teamet via Discord):
```bash
dotnet user-secrets set "CosmosDb:AccountEndpoint" ""
dotnet user-secrets set "CosmosDb:TenantId" ""
dotnet user-secrets set "CosmosDb:DatabaseName" ""
dotnet user-secrets set "CosmosDb:ContainerName" ""
dotnet user-secrets set "ConnectionStrings:AzureBlobStorage" ""
dotnet user-secrets set "ApiKey" ""
dotnet user-secrets set "AdminSettings:Email" ""
dotnet user-secrets set "AdminSettings:Password" ""
dotnet user-secrets set "ApplicationInsights:ConnectionString" ""
dotnet user-secrets set "KeyVault:Url" ""
```

### 3. Starta appen
Kör från solution-roten (`Snaxers-Solution/`):
```bash
docker compose -f The-Snaxers/docker/docker-compose.yml --env-file The-Snaxers/docker/.env up --build -d
```
Öppna `http://localhost:8080`

### 4. Stoppa appen
```bash
docker compose -f The-Snaxers/docker/docker-compose.yml down
```

## Köra lokalt utan Docker
```bash
docker run -p 8080:8080 thesnaxers
```
Öppna `http://localhost:8080`

## Nästa steg
- Pusha till Azure Container Registry (ACR)
- Deploya via Azure Container Apps

## ⚠️ Att göra inför produktion
- Byt ASPNETCORE_ENVIRONMENT till Production
- Lägg till Azure Key Vault URL som miljövariabel
- Lägg till Application Insights connection string
- Ta bort SQLite volume — ersätt med CosmosDB
- Pusha imagen till Azure Container Registry