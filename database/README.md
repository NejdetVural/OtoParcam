# database/

Seed/demo SQL for a local dev database. This folder is **not** the schema source of truth —
that's EF Core migrations under `backend/src/OtoParcam.Infrastructure/Migrations/` (Code-First,
despite `docs/05-Database-Design.md` §2 still describing Database-First — see CLAUDE.md's
"Known gaps" section). Nothing here creates or alters tables.

## Prerequisites

1. SQL Server LocalDB running (`sqllocaldb start MSSQLLocalDB` if needed).
2. Migrations applied:
   ```
   dotnet ef database update -p backend/src/OtoParcam.Infrastructure -s backend/src/OtoParcam.API
   ```

## Scripts (run in order)

| Script | What it does |
|---|---|
| `01-seed-reference-data.sql` | Categories, VehicleBrands, VehicleModels — the catalog taxonomy. Idempotent (per-row unique-name checks). |
| `02-seed-sample-products.sql` | One AcquisitionBatch plus 11 sample Products (with images and one compatibility link) spanning Available/Sold/Hidden status, priced/unpriced, and batch-linked/standalone acquisition cost. Requires `01` to have run first. Idempotent as a whole (no-ops if its marker batch already exists). |
| `03-promote-admin.sql` | One-time bootstrap: confirms the email and grants the Administrator role for an account you've already registered through the app. Requires editing one line first — see the comment at the top of that file for why a password hash isn't scripted here. |

Run each with `sqlcmd`, **always passing `-f 65001`** — without it, Turkish characters (ı, ş,
ğ, ç, ö, ü) get silently mangled even in `N'...'` literals, because the corruption happens at
file-read time, not in T-SQL:

```
sqlcmd -S "(localdb)\mssqllocaldb" -d OtoParcamDb -i database\01-seed-reference-data.sql -f 65001
sqlcmd -S "(localdb)\mssqllocaldb" -d OtoParcamDb -i database\02-seed-sample-products.sql -f 65001
```

Register an account through the frontend or `POST /api/v1/auth/register`, then edit and run
`03-promote-admin.sql` to make it an Administrator.

## Notes

- Image URLs in the sample data point at `placehold.co` (external placeholders) rather than
  real files under `wwwroot/uploads/` — good enough to see the catalog UI populated, not real
  product photography.
- These scripts assume a fresh/dev database. Re-running `01` is safe against real data (it only
  ever adds categories/brands/models that don't already exist by name); `02` is guarded as an
  all-or-nothing block and will simply skip if its sample data is already present, but wasn't
  designed to be layered onto a database that already has unrelated real products.
