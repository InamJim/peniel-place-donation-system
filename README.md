# Peniel Place Donation System

[![Build and test](https://github.com/Julie-portfolio/peniel-place-donation-system/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Julie-portfolio/peniel-place-donation-system/actions/workflows/dotnet.yml)

A donation-management system for **Peniel Place Ministries**, a church in the community. Volunteers record donations and issue receipts, and church leadership views dashboards and reports. Built for the IIE WIL module (XADAD7112/w) by a four-person team.

**Stack:** ASP.NET Core 8 MVC, Entity Framework Core, ASP.NET Identity, **Azure SQL Database** (cloud), a Progressive Web App (PWA) for mobile, GitHub Actions for CI.

## Features

- Record donations and issue receipts
- Donor records with search and lookup
- Dashboard, reports and CSV export (leadership only)
- Role-based access: Volunteer and Leadership
- Two-factor authentication (authenticator app) for every account
- Donor and donation data encrypted at rest
- Void a donation with a reason (donations are never deleted)
- Desktop and mobile: responsive design, installable as a PWA, offline page

## Project structure

```
Backend/
  Controllers/    Handle requests (Account, Dashboard, Donation, Donor, Receipt, Report)
  Services/       Business logic (donations, donors, receipts, reporting)
  Models/         Entities (database tables)
  ViewModels/     Data passed to the views
  Data/           AppDbContext (EF Core, Azure SQL)
  Security/       Field encryption, account seeding, safe error handling
  Views/          Razor pages
  wwwroot/        CSS, JS, icons, PWA manifest and service worker
.github/workflows/dotnet.yml    CI build pipeline
```

## Run it locally

You need the .NET 8 SDK. The app uses an Azure SQL database, so you need its connection string. **Never commit the connection string**: this repository is public. Store it in user secrets:

```bash
cd Backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "<your Azure SQL connection string>"
dotnet run --launch-profile https       # https://localhost:7120
```

Your IP address must be allowed in the Azure SQL server firewall. On first start the app creates the tables and seeds the roles and the four gift categories.

### First sign-in

| Environment | Leadership account |
|-|-|
| **Development** | `leader@peniel.local` / `Dev-Only-Change-Me-123!` (seeded
