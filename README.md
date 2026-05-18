# 🍫 The Snaxers — Luxury Chocolate Store

An ASP.NET Core MVC web application for managing and displaying luxury chocolate products, with secure login via Google Authenticator (2FA).

---

## 📋 About

The Snaxers is a product management system for a luxury chocolate brand. Admins can add, edit, and delete chocolate products. The app is secured with two-factor authentication (2FA) using Google Authenticator.

---

## ✨ Features

- 🍫 **Product Management** — Create, read, update and delete (CRUD) chocolate products
- 📦 **Product details** — Name, description, price, category, and image
- 🔐 **User Authentication** — Register and login with ASP.NET Core Identity
- 📱 **Google Authenticator (2FA)** — Extra security via TOTP (Time-based One-Time Password)
- 🎨 **Responsive UI** — Clean and elegant design with Bootstrap

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC (.NET 9) |
| Language | C# |
| Database | Entity Framework Core + SQLite / Azure CosmosDB |
| Storage | Azure Blob Storage |
| Auth | ASP.NET Core Identity + JWT Bearer |
| 2FA | Google Authenticator (TOTP) |
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
git clone [https://github.com/School-Be-Fun-They-said/The-Snaxers.git](https://github.com/School-Be-Fun-They-said/The-Snaxers.git)
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
git clone [https://github.com/School-Be-Fun-They-said/The-Snaxers.git](https://github.com/School-Be-Fun-They-said/The-Snaxers.git)
cd "Snaxers-Solution"

# 1. Create .env from the template
cp The-Snaxers/docker/.env.example The-Snaxers/docker/.env
```
🔐 NOTE: Fill in the values in your newly created .env file (retrieve the secret strings from the team via a secure channel). The actual values are distributed separately and are never saved in Git.

### Environment Variables in .env.example:

```
BLOB_CONNECTION_STRING=         # Azure Blob Storage connection string
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
```

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

## 📱 Setting Up Google Authenticator

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

- Add three GitHub Secrets to the repository: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID.

- Manually coordinate outputs between the Bicep deployment stages (security.bicep → monitoring.bicep → main.bicep). A future orchestrator script is a documented area for improvement.

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
│   ├── main.bicep
│   ├── monitoring.bicep
│   ├── security.bicep
│   └── README.md
│
└── The-Snaxers/              
    │
    ├── Program.cs
    ├── appsettings.json
    │
    ├── Controllers/         
    │   ├── AdminChocolateController.cs
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