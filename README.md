# 🍫 The Snaxers — Luxury Chocolate Store

An ASP.NET Core MVC web application for managing and displaying luxury chocolate products, with authentication via ASP.NET Core Identity, optional Google OAuth, and JWT-secured API endpoints.

---

## 📋 About

The Snaxers is a product management system for a luxury chocolate brand. Admins can add, edit, and delete chocolate products. Logged in users can add favorite chocolate and add products to shopping cart. 

---

## ✨ Features

- **Product Management** — Create, read, update and delete (CRUD) chocolate products
- **Product details** — Name, description, price, category, and image
- **User Authentication** — Register and login with ASP.NET Core Identity
- **Google OAuth** — Optional external login
- **JWT Bearer API Security** — Token-based access for REST API
- **Google Authenticator (TOTP)** — Planned feature
- **Responsive UI** — Clean and elegant design with Bootstrap

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| Language | C# |
| Database | Entity Framework Core + SQLite / Azure CosmosDB |
| Storage | Azure Blob Storage |
| Auth | ASP.NET Core Identity + JWT Bearer |
| 2FA | Google Authenticator (TOTP) | *Planned feature*
| Frontend | Razor Views + Bootstrap 5 |
| Infrastructure | Docker / Azure Container Apps / Bicep |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)
- [Docker & Docker Compose](https://www.docker.com/) (Optional, for local container execution)

### Installation & Local Execution

```bash
# Clone the repo
git clone https://github.com/School-Be-Fun-They-said/The-Snaxers.git
cd The-Snaxers/The-Snaxers

# Restore dependencies
dotnet restore

# Run the app (HTTPS required for Google OAuth)
dotnet run --launch-profile https
```

Open your browser at https://localhost:7261


## 🐳 Running via Docker Locally

```bash
# Clone and navigate to the solution directory
git clone https://github.com/School-Be-Fun-They-said/The-Snaxers.git
cd The-Snaxers

# 1. Create .env from the template
cp The-Snaxers/docker/.env.example The-Snaxers/docker/.env
```
🔐 NOTE: Fill in the values in your newly created .env file (retrieve the secret strings from the team via a secure channel). The actual values are distributed separately and are never saved in Git.

### Environment Variables in .env.example:

```
BLOB_CONNECTION_STRING=         # Local development only: Azure Blob Storage connection string
# In Azure, Blob Storage uses Managed Identity and `AzureStorage__BlobEndpoint` instead of `BLOB_CONNECTION_STRING`.
COSMOS_ENDPOINT=                # Azure CosmosDB endpoint
COSMOS_KEY=                     # CosmosDB account key
COSMOS_DATABASE=                # Database name
COSMOS_CONTAINER=               # Container for products
COSMOS_CART_CONTAINER=          # Container for shopping cart (optional)
ADMIN_EMAIL=                    # Admin user seeded at startup
ADMIN_PASSWORD=                 # Admin password
GOOGLE_CLIENT_ID=               # Google OAuth Client ID (optional)
GOOGLE_CLIENT_SECRET=           # Google OAuth Client Secret (optional)
APP_INSIGHTS_CONNECTION_STRING= # Telemetry connection string (optional)
JWT_SECRET=                     # JWT signing secret for local API authentication
```

### Blob Storage configuration

For local Docker execution, Blob Storage can be configured with a connection string through:

```env
BLOB_CONNECTION_STRING=
```

This value is only intended for local development and should be retrieved through a secure team channel. It must never be committed to Git.
In Azure, the application uses Managed Identity instead of a Blob Storage connection string. The Container App receives the Blob endpoint through:
```
AzureStorage__BlobEndpoint=https://<storage-account>.blob.core.windows.net/
```

´Program.cs´ then creates the Blob client with ´DefaultAzureCredential´, allowing the Container App's managed identity to authenticate against Azure Blob Storage without storing static keys in the repository or application settings.
Product images are uploaded to the ´products´ container. The container is configured for public blob-level read access so product images can be rendered by the frontend without requiring users to authenticate against Blob Storage.

### Build and Start Containers

```bash
# Spin up the environment
docker compose -f The-Snaxers/docker/docker-compose.yml --env-file The-Snaxers/docker/.env up --build -d

# Verify that everything is running and follow logs
docker ps
docker logs docker-web-1 --follow
```

- Application: http://localhost:8080
- Health Checks: http://localhost:8080/health

#### Health endpoints:

- `/health/live` — liveness check
- `/health/ready` — readiness/dependency check for Cosmos DB and Blob Storage
- `/health` — JSON health summary

The app exposes `/health/ready`; the current Container Apps readiness probe may still be TCP-based unless configured separately.

### Shut Down the Environment

```bash
docker compose -f The-Snaxers/docker/docker-compose.yml down
```

## 🔐Google OAuth - Local setup

Google OAuth credentials are encrypted and stored in User Secrets and do **not** follow the repository to Git.

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_CLIENT_SECRET"
```

## 📱 Setting Up Google Authenticator (Planned feature)

1. Register a new account in the app

2. Go to Account Settings → Two-Factor Authentication

3. Scan the QR code with the Google Authenticator app

4. Enter the 6-digit code to confirm

5. 2FA is now active on your account ✅

## 🔐 JWT API-Security — Local setup

For the gateway guard (JWT middleware) to validate tokens locally, you must set a custom encryption key on your machine (minimum 32 characters):

```bash
dotnet user-secrets set "Jwt:Secret" "CreateYourOwnSecretKeyWithAtLeast32Characters123!"
```

## To call the product API:

1. POST credentials to `/api/v1/auth/login`
2. Copy the returned JWT token
3. Call `/api/v1/products` with `Authorization: Bearer <token>`

## 🤖 CI/CD & Deployment Automation

**Fully Automated Steps:**

- **Every Push**: Automatic build, test execution, Docker build, and smoke tests.

- **Merge (main/develop)**: Automatic deployment to Azure Container Apps.

- **Application Startup**: The SQLite database (Identity) is created automatically via EnsureCreated().

- **Data Seeding**: Admin account is automatically seeded at startup if AdminSettings:Email and Password are configured. The product cache is also pre-warmed automatically at startup.

**Manual Infrastructure Steps (Executed Once):**

- Create Resource Group, CosmosDB, Blob Storage, Container Registry, and Container App in Azure.

- Create App Registration in Entra ID with Federated Credentials targeting the GitHub repository (for OIDC).

- Assign RBAC Roles: AcrPush to the service principal, AcrPull + Storage Blob Data Contributor to the application's Managed Identity.

- Add these GitHub Secrets to the repository: 
    - AZURE_CLIENT_ID 
    - AZURE_TENANT_ID 
    - AZURE_SUBSCRIPTION_ID
    - ADMIN_EMAIL
    - ADMIN_PASSWORD
    - JWT_SECRET 
    - GOOGLE_CLIENT_ID (optional) 
    - GOOGLE_CLIENT_SECRET (optional)

- Infrastructure can be deployed with `infra/deploy.ps1`, which runs `security.bicep`, `monitoring.bicep`, and `main.bicep` in order and passes required outputs between stages.

## 📁 Project Structure

Here is an overview of how the project's folders and files are structured:

```
The-Snaxers/                  
│
├── .github/
├── .gitignore
├── .dockerignore
├── README.md
├── infra/
│   ├── deploy.ps1
│   ├── main.bicep
│   ├── monitoring.bicep
│   ├── security.bicep
│   ├── setup-oidc.ps1
│   └── README.md
│
└── The-Snaxers/              
    │
    ├── Program.cs
    ├── appsettings.json
    │
    ├── Controllers/         
    │   ├── AdminChocolateController.cs
    │   ├── AuthApiController.cs
    │   ├── CartController.cs
    │   ├── ChocolateController.cs
    │   ├── CountryApiController.cs
    │   ├── FavoriteController.cs
    │   ├── HomeController.cs
    │   ├── ProductController.cs
    │   ├── ProductsApiController.cs
    │   └── WhoAmIController.cs
    │
    ├── Data/
    │   └── ApplicationDbContext.cs
    │
    ├── DTOs/
    │   ├── LoginDto.cs
    │   └── ProductDto.cs
    │
    ├── Filters/
    │   └── ApiKeyFilter.cs
    │   
    ├── Models/
    │   ├── CartItem.cs
    │   ├── ErrorViewModel.cs
    │   ├── Favorite.cs
    │   ├── Product.cs
    │   └── ShoppingCart.cs
    │
    ├── Repositories/
    │   ├── CartRepository.cs
    │   ├── CosmosFavoriteRepository.cs
    │   ├── CosmosProductRepository.cs
    │   ├── CosmosCartRepository.cs
    │   ├── ICartRepository.cs
    │   ├── IFavoriteRepository.cs
    │   └── IProductRepository.cs
    │
    ├── Services/
    │   ├── BlobHealthCheck.cs
    │   ├── BlobService.cs
    │   ├── CartService.cs
    │   ├── CosmosHealthCheck.cs
    │   ├── CountryFactHelper.cs
    │   ├── CountryService.cs
    │   ├── FavoriteService.cs
    │   ├── IBlobService.cs
    │   ├── ICartService.cs
    │   ├── ICountryService.cs
    │   ├── IFavoriteService.cs
    │   ├── InventoryService.cs
    │   ├── IInventoryService.cs
    │   ├── IProductService.cs
    │   └── ProductService.cs
    │
    └── Views/
        ├── AdminChocolate/
        ├── Cart/
        ├── Chocolate/
        ├── Favorite/
        └── Shared/
```


## 🚀 Dream Team

 - **Martina** (CRUD Logic, JWT Security, API Authentication, Cookies, CSRF Protection & Azure Blob Storage)

- **Tom** (Infrastructure as Code (Bicep), Azure CosmosDB & Azure Container Registry)

- **Hanita** (Docker Configuration, Application Lifecycle Management, Caching, Health Checks & Structured Logging)