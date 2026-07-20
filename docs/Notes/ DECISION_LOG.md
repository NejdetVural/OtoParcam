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