# OtoParcam

# Architecture Decision Log

---

## DECISION-001

### Title

Each physical spare part is stored as a separate Product.

### Status

Accepted

### Reason

Every used spare part has its own physical condition,
photos, price and history.

Therefore quantity management is unnecessary.

### Consequences

- Stock quantity is removed.
- Each item has its own Product record.
- AvailabilityStatus replaces Stock.

---

## DECISION-002

### Title

Product Price is optional.

### Status

Accepted

### Reason

Auto spare part prices frequently change.
Many businesses do not publish fixed prices.

### Consequences

- Price is nullable.
- UI displays "Fiyat İçin Arayın" if Price is null.

---

## DECISION-003

### Title

Condition is stored as Description.

### Status

Accepted

### Reason

Physical defects differ depending on the part.

Examples:

- Scratch
- Dent
- Broken tab
- Lens oxidation

A generic Condition field would not be meaningful.

### Consequences

No Condition column will exist.

---

## DECISION-004

### Title

Compatible Vehicles are selected manually.

### Status

Accepted

### Reason

The system is not responsible for determining compatibility.

Administrators already know which vehicles are compatible.

### Consequences

Many-to-Many relationship

Product

↓

ProductCompatibility

↓

VehicleModel

---

## DECISION-005

### Title

The displayed vehicle represents the source vehicle.

### Status

Accepted

### Reason

Customers should understand that
the product title refers to the vehicle
from which the part was removed.

Compatibility is shown separately.

### Consequences

Product stores one SourceVehicleModel.

Compatible vehicles are managed separately.

---

## DECISION-006

### Title

Database engine changed from PostgreSQL to Microsoft SQL Server

### Status

Accepted

### Reason

Mentor guidance during internship; team/deployment environment standardizes on SQL Server. SQL Server Developer Edition is free and has full feature parity with production editions, suitable for development before real data is introduced.

### Consequences

All VARCHAR/TEXT columns become NVARCHAR/NVARCHAR(MAX) for Turkish character support. UUID becomes UNIQUEIDENTIFIER. A licensed edition (Express, Standard, or Azure SQL) must be selected before the system handles real production data — Developer Edition is licensed for development/test only.

---

## DECISION-007

### Title

Added AcquisitionBatch to support lump-sum purchases of many parts, without changing individual per-product acquisition tracking.

### Status

Accepted

### Reason

Real purchasing does not happen one part at a time. A shop typically buys an entire insurance-total-loss vehicle (e.g.
"200 parts from a Ford") for one lump sum, then dismantles it into many individual Products that get listed and sold
separately — sometimes 50 sell immediately, the rest sit in stock. The existing `Product.AcquisitionCost`/`AcquisitionSource`
fields assumed a cost was known per part, which forces an admin to either leave 200 costs blank or manually guess a split
by hand. DECISION-001 (each part is its own Product, no quantity field) still holds — this is purely about how *cost* is
recorded, not about merging parts back into a batched/quantity model.

### Consequences

- New `AcquisitionBatch` entity: `Source`, `TotalCost`, `PurchaseDate`, `Notes`. Managed via `/admin/acquisition-batches`
  (Administrator only).
- `Product` gets an optional `AcquisitionBatchId`. `Product.AcquisitionCost`/`AcquisitionSource` are untouched in meaning —
  still a per-part override, still fully usable standalone with no batch at all (BR-72).
- A Product's *effective* acquisition cost/source (used by Reports and the Dashboard) resolves to its own override if set,
  otherwise the batch's `TotalCost` divided evenly across every Product currently linked to that batch, regardless of status
  (BR-73). This recalculates dynamically as parts are added to or removed from a batch — it is never baked into a stored value.
  An admin can still override the split for a specific part known to be worth disproportionately more or less (e.g. an
  engine vs. a bumper from the same wreck).
  This is a case where a business need existed but no doc committed to a specific mechanism — see BR-72/73/74 in
  `02-Business-Requirements.md` and the "Acquisition Batches" section in `06-Api-Design-Specification.md` for the resulting spec.
- Deleting a batch is blocked (409) while any Product still references it, matching the delete-blocking pattern already used
  for Category/VehicleBrand/VehicleModel (BR-42/46/52).

---

## DECISION-008

### Title

`Product.SoldPrice` became a real stored column instead of being derived from the approving PurchaseRequestItem, and gained a direct admin "mark as sold" action.

### Status

Accepted

### Reason

The user pointed out the site is also used as an inventory tracker, not just an online storefront — plenty of parts get
sold in person or by phone, entirely outside the purchase-request/negotiation flow. Before this, `SoldPrice` only ever
existed as a computed value (`ProductService`/`ReportService` walked `Product.PurchaseRequestItem` looking for an
`Approved` request), so a product marked Sold any other way would report a `NULL` price forever and never show up in
revenue/profit reporting. There was no way to mark something sold at all outside that flow.

### Consequences

- `Product.SoldPrice` (`DECIMAL(10,2)`, nullable) is a real column now (§6.1 in `05-Database-Design.md`).
- New administrator-only `PATCH /products/{id}/sell` (`MarkProductSoldAsync`) sets `Status = Sold` and `SoldPrice` directly,
  with no `PurchaseRequest` involved at all. Rejects an already-Sold product and a negative price.
- `ConfirmPurchaseRequestAsync` now also writes `SoldPrice` explicitly (negotiated price, falling back to original) at the
  moment a request is approved, instead of leaving it to be recomputed later.
- Every reader that used to derive the sold price (`ProductService.ToDto`, `ReportService`'s sold-items and
  acquisition-batch revenue rollups, `AcquisitionBatchService`'s revenue rollup) now just reads `Product.SoldPrice`
  directly — simpler code, and the two sale paths (online-approved vs. admin-marked) are indistinguishable to every
  consumer, which is the intended behavior (BR-78).