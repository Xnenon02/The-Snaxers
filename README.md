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

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

### Installation

```bash
# Clone the repo
git clone https://github.com/Xnenon02/The-Snaxers.git
cd The-Snaxers

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the app
dotnet run
```

Open your browser at `http://localhost:5000`

---

## 📱 Setting Up Google Authenticator

1. Register a new account in the app
2. Go to **Account Settings → Two-Factor Authentication**
3. Scan the QR code with the **Google Authenticator** app
4. Enter the 6-digit code to confirm
5. 2FA is now active on your account ✅

---

## 📁 Project Structure
