# Peniel Place Donation System

ASP.NET Core 8 MVC app for recording donations, issuing receipts and reporting.
Runtime project: "Backend/" (SQLite). Open "PenielPlace.Web.sln" or run from the command line.

## Run it

```bash
cd Backend
dotnet run --launch-profile http        # http://localhost:5091
```

First start creates "Backend/storage/" (SQLite database + encryption key ring) and seeds the
roles and the four gift categories.

### First sign-in

|Environment|Leadership account|
|-|-|
|**Development**|"leader@peniel.local" / "Dev-Only-Change-Me-123!" (seeded automatically, only in Development)|
|**Anything else**|Set "BootstrapAdmin\_\_Email" and "BootstrapAdmin\_\_Password" before first start|

Every account must set up an authenticator app (TOTP) on first sign-in (QR code or typed key).
Leadership can then add volunteers from **Add volunteer** in the side menu.

```bash
# example: secrets instead of the dev account
cd Backend
dotnet user-secrets init
dotnet user-secrets set "BootstrapAdmin:Email" "you@church.org"
dotnet user-secrets set "BootstrapAdmin:Password" "a-long-Passw0rd!-here"
```

## Roles

||Volunteer|Leadership|
|-|-|-|
|Record donations, issue/view receipts|yes|yes|
|Look up / add donors, browse donations|yes|yes|
|Dashboard, reports, CSV export|no|yes|
|Add volunteer accounts|no|yes|
|**Void a donation** (with a reason)|no|yes|

### Voiding donations

Donations are never deleted. Leadership can **void** one from the *Donations* list or its receipt page.
A reason (5-250 characters) is required and is stored encrypted, together with who voided it and when.
A voided donation stays visible, marked **VOID** (its receipt shows a VOID banner), and is excluded from donor totals,
the dashboard, reports and CSV exports. Voiding cannot be undone: to correct a mistake, void it and record a new donation.

## Data protection - read this

* Donor names/contacts and donation amounts/descriptions/notes are **encrypted at rest** with
ASP.NET Core Data Protection. The keys live in "Backend/storage/keys".

## Production checklist

* Serve over HTTPS (cookies are "Secure" outside Development).
* Set "AllowedHosts" in "appsettings.json" to your real host name(s).
* Set "BootstrapAdmin" credentials, then remove them from the environment after the first start.
* Schema is created with "EnsureCreated" (same approach as the original Backend). Changing entities later
needs EF migrations or a fresh database.

