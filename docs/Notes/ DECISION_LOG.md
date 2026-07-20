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