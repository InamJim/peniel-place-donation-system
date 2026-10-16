# Peniel Place Donation System

Donation-management application for Peniel Place Ministries. Volunteers capture donors and donations and issue receipts; Leadership has access to administrative dashboards and reporting.

**Technology:** ASP.NET Core 8 MVC, Entity Framework Core, ASP.NET Core Identity, SQLite by default, and a responsive/PWA presentation layer.

## Main features

- Donor capture and lookup
- Donation capture and voiding with a reason
- Receipt generation and viewing
- Leadership dashboard, reports and CSV export
- Role-based authorization: `Volunteer` and `Leadership`
- Authenticator-app two-factor authentication
- Protection of donor and donation fields using ASP.NET Core Data Protection
- Antiforgery protection, validation, authentication rate limiting and safe exception handling

## Project layout

See [`Backend/Documentation/ARCHITECTURE.md`](Backend/Documentation/ARCHITECTURE.md) for the requested layer-by-layer breakdown.

- `Backend/PresentationLayer/` — controllers, Razor views, view models and static UI assets
- `Backend/BusinessLogicLayer/` — donor/donation/receipt/report services and security helpers
- `Backend/DataAccessLayer/` — EF Core `AppDbContext`
- `Backend/Models/` — domain and application models
- `Backend/Database/` — database setup and migration guidance
- `Backend/Documentation/` — architecture, roles and setup documentation

This is a folder-based layered architecture in a single web project, not a multi-project deployment.

## Run locally

Install the .NET 8 SDK, then from the repository root run:

```powershell
dotnet restore Backend/PenielPlaceMinistries.csproj
dotnet build Backend/PenielPlaceMinistries.csproj
dotnet run --project Backend/PenielPlaceMinistries.csproj
```

By default, the application uses SQLite at `Backend/storage/peniel.sqlite`. You can override the connection string with the `ConnectionStrings:Default` configuration key. Keep the database file and `Backend/storage/keys` Data Protection key ring backed up together. Neither belongs in source control.

For local development, the first Leadership account is seeded as `leader@peniel.local` with the development-only password `Dev-Only-Change-Me-123!` if no Leadership account exists and no bootstrap credentials are configured. **Do not use this credential outside local development.** For an explicitly configured first account, see [`Backend/Documentation/ROLES-AND-ACCOUNT-SETUP.md`](Backend/Documentation/ROLES-AND-ACCOUNT-SETUP.md).

## Security notes

- Public volunteer self-registration is disabled. Leadership creates volunteer accounts because the application handles personal and financial data.
- All accounts must complete authenticator-app two-factor setup before receiving an authenticated session.
- Never commit real passwords, database connection strings, donor data, SQLite databases, or Data Protection keys.
- Database schema creation currently uses `EnsureCreated`. Before production schema upgrades, introduce and review EF Core migrations; see `Backend/Database/README.md`.

## Verification

Run `dotnet build Backend/PenielPlaceMinistries.csproj` and manually test login, initial 2FA setup, subsequent 2FA login, volunteer creation, role restrictions, donor capture, donation capture, receipt generation, reports, and logout. A build/test result should be confirmed on a machine with the .NET 8 SDK before this is described as verified.
