# ===================================================
# Multi-stage Dockerfile för The Snaxers
# Build (från solution-rot): docker build -t the-snaxers -f The-Snaxers/Dockerfile .
# Build (från The-Snaxers/): stöds ej — kör alltid från solution-roten
# Multi-platform: docker buildx build --platform linux/amd64,linux/arm64 -t hanita970/the-snaxers:1.0 --push -f The-Snaxers/Dockerfile .
# ===================================================

# Stage 1 — Build
# SDK-imagen (~900MB) används endast för att kompilera
# Den ingår INTE i den slutliga imagen
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore-first pattern:
# Kopierar .csproj FÖRST för att utnyttja Docker layer cache
# dotnet restore körs bara om .csproj ändrats — inte vid varje kodändring
COPY ["The-Snaxers/The-Snaxers.csproj", "The-Snaxers/"]
RUN dotnet restore "The-Snaxers/The-Snaxers.csproj"

# Kopiera resten av koden och publicera i Release-läge
COPY . .
WORKDIR "/src/The-Snaxers"
RUN dotnet publish "The-Snaxers.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===================================================
# Stage 2 — Runtime
# ASP.NET runtime-imagen (~220MB) — ingen SDK, ingen kompilator
# Endast det som behövs för att köra appen
# ===================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Kopiera endast publish-outputen från build-staget
# Allt annat (SDK, källkod, NuGet-cache) lämnas kvar och ingår inte i imagen
COPY --from=build /app/publish .

# Appen lyssnar på port 8080 (ASP.NET Core default i containers)
EXPOSE 8080

ENTRYPOINT ["dotnet", "The-Snaxers.dll"]