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
| Database | Entity Framework Core + SQLite |
| Auth | ASP.NET Core Identity |
| 2FA | Google Authenticator (TOTP) |
| Frontend | Razor Views + Bootstrap 5 |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

### Installation

```bash
# Clone the repo
git clone https://github.com/Xnenon02/The-Snaxers.git
cd The-Snaxers/The-Snaxers

# Restore dependencies
dotnet restore

# Run the app (HTTPS krävs för Google OAuth)
dotnet run --launch-profile https
```

Open your browser at `https://localhost:7261`

---

## 🔐 Google OAuth — Lokal setup

Google OAuth-credentials lagras i User Secrets och följer **inte** med i git.

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "DITT_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "DITT_CLIENT_SECRET"
```

> Kontakta en teammedlem för att få ClientID och ClientSecret.

**OBS:** Starta alltid appen med `dotnet run --launch-profile https` — Google OAuth kräver HTTPS lokalt.

---

## 📱 Setting Up Google Authenticator

1. Register a new account in the app
2. Go to **Account Settings → Two-Factor Authentication**
3. Scan the QR code with the **Google Authenticator** app
4. Enter the 6-digit code to confirm
5. 2FA is now active on your account ✅

---

## 📁 Project Structure
