# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

OtoParcam is a web-based inventory and sales management system for businesses that sell **used auto spare parts**. Customers find compatible parts by vehicle brand/model; administrators manage products, inventory, and orders through a purchase-request workflow with optional phone-negotiated pricing. Full domain context lives in `docs/` — read `docs/01-Project-Vision.md` and `docs/02-Business-Requirements.md` before implementing business logic, since the numbered `BR-*` rules there are the source of truth for validation/behavior decisions.

Planned stack (per `docs/01-Project-Vision.md`): ASP.NET Core Web API + EF Core + SQL Server backend; React + TypeScript + Tailwind frontend; JWT auth via ASP.NET Core Identity, FluentValidation, Serilog, Docker.

Current state: the backend has a working Clean Architecture skeleton — domain entities, an EF Core `DbContext` with Fluent API configurations for every entity, ASP.NET Identity + JWT bearer authentication wired into DI, and role-based authorization policies — but **no controllers/endpoints exist yet** and **no EF Core migration has been generated or applied**. `frontend/`, `database/`, and `assets/` are empty placeholder directories (each holds only a blank `README.md`).

## Commands

Run from the repository root (`OtoParcam.sln` lives at the repo root, not under `backend/`):

```
dotnet build OtoParcam.sln                                   # build all projects
dotnet run --project backend/src/OtoParcam.API                # run the API
dotnet watch --project backend/src/OtoParcam.API              # run with hot reload
```

EF Core migrations (none exist yet — this is the next step before the app can hit a real database):

```
dotnet ef migrations add <Name> -p backend/src/OtoParcam.Infrastructure -s backend/src/OtoParcam.API
dotnet ef database update      -p backend/src/OtoParcam.Infrastructure -s backend/src/OtoParcam.API
```

`-p` (project) must be `Infrastructure` (it owns `ApplicationDbContext`); `-s` (startup project) must be `API` (it owns the connection string in `appsettings.Development.json` and the DI wiring). SQL Server LocalDB (`(localdb)\mssqllocaldb`) is the dev target — it ships with Visual Studio, no separate install needed; start it manually with `sqllocaldb start MSSQLLocalDB` if a command needs it running (it also auto-starts on first connection).

There are no test projects yet (`**/*Test*` has no matches). When adding one, use `dotnet test`.

`git` is not on PATH in this environment; if a shell `git` invocation fails with "not recognized", fall back to a bundled copy, e.g. `"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe"`.

## Architecture

Clean Architecture, 4 projects under `backend/src/`, all targeting net9.0:

- **OtoParcam.Domain** — entities, enums, `BaseEntity` (Id/CreatedAt/UpdatedAt), and `Constants/Roles.cs` (`Administrator`/`Customer` string constants used by both ASP.NET Identity role checks and future `[Authorize(Roles = ...)]` attributes). No project references (innermost layer).
- **OtoParcam.Application** — references Domain only. Currently just a placeholder `Class1.cs`; this is where use cases/services/interfaces are meant to go.
- **OtoParcam.Infrastructure** — references Application + Domain, plus a `FrameworkReference` to `Microsoft.AspNetCore.App` (required because it's a plain class library, not `Sdk.Web`, so ASP.NET Identity/JWT types like `AddIdentity`/`AddJwtBearer` aren't available by default). Holds:
  - `Persistence/ApplicationDbContext.cs` — `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`, auto-stamps `CreatedAt`/`UpdatedAt` on every `BaseEntity` in an overridden `SaveChangesAsync`.
  - `Persistence/Configurations/*.cs` — one `IEntityTypeConfiguration<T>` per entity, applied via `ApplyConfigurationsFromAssembly`. This is where BR-driven schema rules live (column types/precision, FK delete behavior, unique/check constraints) — see `05-Database-Design.md` for the values each one is meant to match.
  - `DependencyInjection.cs` — `AddInfrastructureServices(IServiceCollection, IConfiguration)` extension method: registers `ApplicationDbContext` (SQL Server), ASP.NET Identity, JWT bearer authentication (reads the `Jwt` config section: `Secret`/`Issuer`/`Audience`/`ExpiryMinutes`), and the `Administrator`/`Customer` authorization policies. Called once from `API/Program.cs`.
- **OtoParcam.API** — references **both Application and Infrastructure** (`Program.cs` calls `AddInfrastructureServices`). Pipeline has `UseAuthentication()`/`UseAuthorization()` wired, but there is no token-issuing endpoint yet (no `/auth/register` or `/auth/login` controller) — when building those, pull the user's roles via `UserManager.GetRolesAsync()` and embed them as role claims when signing the JWT, since stateless bearer auth doesn't populate roles from the DB automatically per request the way cookie-based Identity sign-in does.

Domain model (`backend/src/OtoParcam.Domain/Entities`): `Product`, `Category`, `VehicleBrand`, `VehicleModel`, `ProductImage`, `ProductCompatibility` (product↔vehicle model many-to-many), `ApplicationUser` (extends `IdentityUser<Guid>`), `Favorite`, `PurchaseRequest`, `PurchaseRequestItem`. Key relationships:
- A `Product` belongs to one `Category` and has a source `VehicleModel`, plus many compatible `VehicleModel`s via `ProductCompatibility`.
- A `VehicleModel` belongs to one `VehicleBrand`.
- A `PurchaseRequest` belongs to one `ApplicationUser` and holds many `PurchaseRequestItem`s, each snapshotting both `OriginalPrice` and `NegotiatedPrice` for a `Product` — the `Product.Price` itself is never mutated during negotiation (BR-31/BR-33/BR-34).

Purchase request status flow (`PurchaseRequestStatus` enum) drives the core order workflow and must only be transitioned by the system, never set directly by clients: `Pending → WaitingForCustomerConfirmation → Approved | Rejected`, plus `Cancelled` (only while `Pending`). Stock/status changes on the `Product` happen only when a request reaches `Approved`.

## API design

`docs/06-Api-Design-Specification.md` is the authoritative REST contract (base path `/api/v1`, JSON, JWT bearer auth) — implement controllers to match its endpoint list, status codes, and access rules (Public / Customer / Administrator) rather than inventing new routes. Notable conventions from that spec:
- `DELETE /products/{id}` is non-destructive — it sets status to `Hidden`, it does not delete the row.
- Purchase request status transitions are exposed as dedicated `PATCH .../cancel|confirm|reject` actions and an admin `PATCH .../negotiation` action — there is no generic "update status" endpoint.
- Product listing (`GET /products`) is paginated at a fixed page size of 20 and supports `categoryId`, `vehicleBrandId`, `vehicleModelId`, `keyword`, `color`, `page`, `sortBy` (`priceAsc`/`priceDesc`, with unpriced products sorted last).

## Docs

`docs/` contains the full spec set and should be treated as living requirements documentation, kept consistent with the code:
- `01-Project-Vision.md`, `02-Business-Requirements.md` (numbered `BR-*` business rules), `03-Software-Requirements-Specification.md`, `04-Use-Case-Specification.md`, `05-Database-Design.md`, `06-Api-Design-Specification.md`, `07-ERdiagram.drawio`.
- `docs/Notes/EntityProcess.md` is a working-notes log of per-entity design decisions and progress — check it for context/rationale not repeated in the numbered specs (e.g. it records that `Product.Side`/`Position` were decided as nullable fields but not yet implemented in the entity or DB design doc).

Known gaps between docs and current code/decisions, worth checking before trusting either as ground truth:
- `05-Database-Design.md` §2 says the project follows a **Database-First** approach, but the actual implementation is **Code-First** (entities + Fluent API configs → EF migrations). Treat Code-First as the real approach until that line is corrected.
- `07-ERdiagram.drawio` is not an editable diagram — it's a single embedded raster image (~319KB) pasted onto a draw.io canvas, with no real shape/text nodes. Text search/replace against it will silently find nothing; regenerating it means building a new diagram from scratch, not editing the existing file.
