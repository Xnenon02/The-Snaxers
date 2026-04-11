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

## Köra lokalt
```bash
docker run -p 8080:8080 thesnaxers
```
Öppna `http://localhost:8080`

## Nästa steg
- Pusha till Azure Container Registry (ACR)
- Deploya via Azure Container Apps