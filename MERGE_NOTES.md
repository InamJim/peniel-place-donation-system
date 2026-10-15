# Merge notes: `master-update` (Backend/) + `local`

**Rule applied:** `Backend/` from *master-update* is the runtime base; everything useful in *local* was
implemented on top of it.

## What came from where

| Area | Source | Result in merged `Backend/` |
|---|---|---|
| Project file, SQLite, folder layout, service-layer style, `Services/`, `Models/`, `Data/` | master Backend | kept (entities, services and DbContext rewritten on this base) |
| Login, TOTP two-factor, lockout, Identity, Volunteer/Leadership roles | local `PenielPlace.Web` | ported; replaces master's hand-rolled `Users` table and `admin`/`volunteer` roles |
| Field-level encryption, `IdentitySeed`, `SafeExceptionHandler` | local | ported, wired into `Program.cs` |
| Dashboard, grouped reports, CSV export, donor lookup, receipts, offline PWA UI | local views / viewmodels / wwwroot | ported |
| Donation **list with filters**, receipts as own table (+ issued-by), category/donor/date report filters | master Backend | kept, adapted to encrypted data |
| `PenielPlace.Web/` project (views only in master; full in local) | both | **folded into `Backend/`; project removed**. Solution now points at `Backend/` |
| PHP `Backend/` (local) | local | **not carried over**: a PHP port of the same domain logic already present in the C# Backend |

## Bugs found in the inputs and fixed

1. **master** `appsettings.json` had `"Default": "Data Source"` (invalid SQLite string) -> app could not start.
2. **master** `HomeController` had no `Index` but `Home/Index` is the default route -> `/` was a 404.
3. **local** `Program.cs` never registered encryption, reporting or exception-handler services, never ran the
   seed, and read connection string `DefaultConnection` which did not exist -> would not start.
4. **local** referenced views that did not exist (`SetupAuthenticator`, `CreateVolunteer`) -> created.
5. **local** "Don't ask again for 30 days" checkbox was never implemented -> removed (no misleading control).
6. **local** CSV claimed a UTF-8 BOM but never wrote one -> fixed.
7. **local** offline queue: a replayed donation rejected by validation came back as HTTP 200 (form re-shown)
   and an expired session came back as 200 (login page); the browser treated both as "sent" and **discarded
   the donation**. Server now returns 400 for rejected replays and `site.js` keeps items queued on login redirects.
8. **local** receipt page showed the *viewer* as "issued by"; receipts now store who actually issued them.
9. **local** money parsing depended on server culture (`en-ZA` expects a decimal comma but the form says `0.00`)
   -> request culture fixed to invariant.
10. Data-protection keys were not persisted -> a restart would have made all stored data unreadable. Now file-persisted.
11. Added (not present in either): rate limiting on sign-in endpoints, global "must be signed in" policy,
    security response headers, `Secure` cookies outside Development, and `storage/` in `.gitignore`.

## Added after the merge: voiding donations

Neither input could remove or correct a donation. Added a Leadership-only **void** (no hard delete, so issued
receipts never point at nothing): required reason (encrypted), who/when recorded, `VOID` badge and receipt banner,
excluded from every total/report/CSV, double-void blocked, audit line written to the application log.
New columns on `Donations`: `IsVoided`, `VoidedUtc`, `VoidedById`, `VoidedByName`, `VoidReasonProtected`
(delete any `peniel.sqlite` created from an earlier build of this merge).

## Things to know / decide

* **Existing SQLite files from master are incompatible** (old `Users` table, plaintext data). Delete
  `Backend/storage/peniel.sqlite` (or old equivalent) before first run. No data migration is included; if you
  have real donor data in the old schema, say so and a migration script can be written.
* Solution file keeps the name `PenielPlace.Web.sln` so your git history stays tidy; rename if you prefer.
* Gaps that exist in both inputs and were left alone: no change/reset-password screen, no way to deactivate a
  volunteer, no privacy-notice page (the placeholder `Privacy` template was dropped), amounts are summed in memory
  (fine for a church's volume, not for millions of rows).
* The offline queue stores pending donations in the browser's `localStorage` (unencrypted) until they sync.

## What was and was not verified

Verified here (.NET 8 SDK, real EF Core + Identity + ASP.NET Core assemblies):
* Entire project and **all Razor views compile with no warnings or errors**.
* EF model builds; generated schema checked (relations, restrict deletes, unique receipt numbers, seeded categories).
* Live HTTP tests in Development (HTTP) and Production (HTTPS): sign-in gating on every route, Volunteer vs
  Leadership access (403 -> AccessDenied), anti-forgery, validation messages, friendly error page with no leaked
  details, security headers, `Secure` cookies, rate limiting (429 after 10/min), static/PWA assets.
* All controllers and views rendered with data through test doubles; donation POST flows incl. offline replay.
* Void flow: Leadership-only on GET and POST (Volunteer and anonymous are redirected), validation, unknown id -> 404,
  double-void rejected, anti-forgery enforced, VOID shown on receipt, Void links hidden from volunteers.
* Encryption: survives restart with persisted keys, rejects wrong keys and tampered data, culture-safe; CSV output.

**Not verified (sandbox has no NuGet access):** `dotnet restore` of the two NuGet packages
(`Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, both 8.0.11), and
queries executing against a real SQLite file (including the new void update and the "exclude voided" filter). The queries are plain LINQ (equality/range/ordering/`Include`) with no
provider-specific constructs, but please do a first-run click-through: sign in -> set up authenticator -> add donor ->
record donation -> view receipt -> dashboard/report/CSV.
