# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Kopiera projektfilen från undermappen för att utnyttja Docker-cache för restore
COPY ["The-Snaxers/The-Snaxers.csproj", "The-Snaxers/"]
RUN dotnet restore "The-Snaxers/The-Snaxers.csproj"

# 2. Kopiera resten av koden (Docker ser nu hela Snaxers-Solution)
COPY . .

# 3. Gå in i mappen där koden faktiskt ligger och kör publish
WORKDIR "/src/The-Snaxers"
RUN dotnet publish "The-Snaxers.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Bake the Git commit SHA into the image at build time.
# The CI pipeline passes --build-arg BUILD_SHA=${{ github.sha }}.
# Falls back to "local" when building outside CI (e.g. docker build locally).
ARG BUILD_SHA=local
ENV BUILD_SHA=$BUILD_SHA

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "The-Snaxers.dll"]