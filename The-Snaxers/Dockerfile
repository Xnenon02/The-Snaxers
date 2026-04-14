# TODO: Lägg till miljövariabler för Azure Key Vault URL och App Insights när produktion är klar
# TODO: Byt ASPNETCORE_ENVIRONMENT till Production vid deploy till Azure Container Apps
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY *.csproj .
RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "The-Snaxers.dll"]