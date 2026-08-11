# OtoParcam

## API Design Specification

Version 1.8

Author Nejdet Vural

Date 17.07.2026

---

## Changelog

- **Added `?period=` to `GET /admin/reports/statistics`** (`AllTime` (default) | `Daily` | `Weekly` | `Monthly`) — scopes
  the Sales Performance section (and its revenue/cost/profit totals) to a rolling window ending now, based on the new
  `Product.SoldAt` timestamp (set independently of `UpdatedAt`, alongside `SoldPrice`, on both sale paths — approving a
  purchase request and the direct "mark as sold" action — so an unrelated later edit can't shift a sale into a different
  period). Every other section (general counts, inventory value, acquisition batch part counts) is unaffected by the
  period and always reflects current state. Per BR-79, added 2026-08-11.
- **Added `PATCH /products/{id}/sell`** (Administrator only) — marks an Available or Hidden product Sold with an
  administrator-entered `soldPrice`, without any purchase request, since not every sale goes through the online
  request/negotiation flow (a walk-in or phone sale still needs recording since this system doubles as inventory
  tracking). `Product.SoldPrice` is now a real column (previously derived on read from the approving `PurchaseRequestItem`)
  set either by this action or by `PATCH /purchase-requests/{id}/confirm`, so reporting/inventory reflect both paths
  identically. Rejects `400` for a negative price, `409` if the product is already Sold. Per BR-78, added 2026-08-11.
- **`POST /products` accepts an optional `status`** (`Available` or `Hidden` only — `Sold` is rejected with `400`). Lets an
  admin create an inventory-only/not-for-sale part directly as Hidden instead of creating it Available and hiding it as a
  separate step. Omitting it still defaults to Available (BR-05 unchanged). Per BR-77, added 2026-08-11.
- **Closed a double-claim gap on `POST /purchase-requests` and `PATCH /purchase-requests/{id}/confirm`**: since a Product
  stays **Available** until a request reaches **Approved**, nothing previously stopped the same customer from opening a
  second active request on a product they already had one pending for, nor stopped confirming a request whose product had
  already been claimed by another approved request in the meantime. `POST /purchase-requests` now rejects (`409`) a new
  request for a product the caller already has an active (`Pending`/`WaitingForCustomerConfirmation`) request for;
  `PATCH .../confirm` now re-verifies every item's product is still `Available` and rejects (`409`) if not. Per BR-75/BR-76,
  added 2026-08-11.
- **Added Acquisition Batches** (`/admin/acquisition-batches`, Administrator only) and `Product.AcquisitionBatchId` — parts are
  often bought together as one lump-sum purchase (e.g. a whole insurance-total-loss vehicle) but always sold individually
  (BR-72). `Product.AcquisitionCost`/`AcquisitionSource` are unchanged and still work standalone for individually-purchased
  parts; a Product may additionally link to a batch, in which case its reported acquisition cost falls back to an even split
  of the batch's total cost across every Product currently linked to it (BR-73) unless a per-product override is set. Added
  2026-08-11 at the user's explicit request.
- **Added `Product.AcquisitionCost`/`Product.AcquisitionSource`** (v1.0 scope stayed at "no cost tracking"; added as an
  explicit, deliberate extension past that, not a silent scope change) — internal-only fields, always `null` for non-Administrator
  callers regardless of product status. Settable only via the Administrator-only `POST`/`PUT /products` endpoints.
- **Added Reports section** (`GET /admin/reports/statistics`, Administrator only) — generates an on-demand PDF covering
  product-status counts, per-item sales performance (acquisition cost/sold price/profit), and current inventory value. Implements
  the "Sales analytics"/"Advanced reporting" items from `03-Software-Requirements-Specification.md` §6 Future Enhancements,
  brought forward into v1.0 at the user's request rather than left for a later version.
- **Added `status` query parameter to `GET /products`**, honored only when the caller is an authenticated Administrator (ignored
  for public/Customer callers, who always see Available-only results per BR-below). Without a `status` value, an Administrator
  sees products in every status; a public caller still sees Available-only. This closes the gap where Hidden/Sold products had
  no way to be listed by an admin once they left the public catalog.
- **Added `PATCH /products/{id}/restore`** to move a Hidden product back to Available (Administrator only). Only valid from
  Hidden — Sold is a terminal state and cannot be restored this way.
- **Added `GET /products` query parameters** to support FR-03 (search) and FR-16 (filtering) � previously undocumented.
- **Added purchase request cancellation endpoint** (`PATCH /purchase-requests/{id}/cancel`) to support the customer-cancellation
  rule already stated in EntityProcess.md ("Customers may cancel a purchase request only while its status is Pending"), which had
  no corresponding endpoint.
- **Added Dashboard section** (`GET /admin/dashboard`) to support FR-10, previously listed as core v1.0 scope in the Project Vision
  and SRS but absent from this spec.
- **Added Customer Profile section** (`GET/PUT /users/me`) to support the Project Vision's "Manage profile information" capability,
  previously absent from this spec.
- **Clarified `DELETE /products/{id}`** as a non-destructive status change, to avoid future ambiguity with a hard delete.
- **Clarified negotiation as optional** in Section 8 � confirmed the confirm/reject endpoints apply whether or not
  `/negotiation` was called first, resolving the gap with BRD Alt Flow 5a (see BRD v1.1 changelog for the underlying decision).
- **Standardized purchase request statuses** to: `Pending`, `WaitingForCustomerConfirmation`, `Approved`, `Rejected`, `Cancelled`.
- **Standardized purchase request item field name**: the original product price snapshot is named `originalPrice`.
- **Clarified final approval ownership:** final approval (son onay) is performed by the Customer via the confirm endpoint.

---

# 1. Introduction

## 1.1 Purpose

This document defines the REST API endpoints exposed by the OtoParcam backend. It specifies the available resources, request formats, response formats, authentication requirements, and expected HTTP status codes.

---

## 1.2 Technology

- ASP.NET Core Web API
- REST
- JSON
- JWT Authentication
- HTTPS

---

## 1.3 Base URL

```
/api/v1

```
## 1.4 General Conventions

The API follows the conventions below:

- All requests and responses use JSON.
- All endpoints are prefixed with `/api/v1`.
- All communication is performed over HTTPS.
- JWT Bearer Authentication is used for protected endpoints.
- All text is encoded using UTF-8.
- Resource names use plural nouns (e.g., `/products`, `/categories`).

# 2. Authentication

## Register

```
POST /auth/register
```

Creates a new customer account.

Authentication Required

- no

---

## Login

```
POST /auth/login
```

Authenticates a customer using an email address or phone number together with a password.

Authentication Required

- No

---

## Confirm Email

```
GET /auth/confirm-email
```

Activates a customer account after successful email verification.

Authentication Required

- No

---


# 3. Products

## Get Products

```http
GET /products
```

Returns a paginated list of products.

Supports filtering, searching, and sorting using optional query parameters.

| Parameter | Type | Description |
|------------|------|-------------|
| `categoryId` | UUID | Filter by category |
| `vehicleBrandId` | UUID | Filter by vehicle brand |
| `vehicleModelId` | UUID | Filter by vehicle model (source or compatible) |
| `keyword` | string | Searches product descriptions |
| `color` | integer | Filter by color |
| `status` | integer | Filter by status (`1`=Available, `2`=Sold, `3`=Hidden). **Administrator callers only** — see below. |
| `page` | integer | Page number (default: 1) |
| `sortBy` | string | `priceAsc` or `priceDesc` |

### Status Visibility

- **Public / Customer callers**: always see Available products only. The `status` parameter is ignored for these callers.
- **Administrator callers** (recognized via a valid bearer token, even though this endpoint itself requires no authentication):
  see products in every status by default; passing `status` narrows the result to that one status (e.g. `status=3` to list
  only Hidden products for review).

### Pagination

- Products are returned in pages of **20** items.
- The page size is fixed and cannot be modified by clients.

### Sorting

Supported values:

- `priceAsc`
- `priceDesc`

Products without a specified price are always listed after products with a defined price.

Access

- Public

---

## Get Product Details

```
GET /products/{id}
```

Returns detailed information about a product.

Authentication Required

- No

---

## Create Product

```
POST /products
```

Creates a new product.

Authentication Required

- Administrator

---

## Update Product

```
PUT /products/{id}
```

Updates product information.

Authentication Required

- Administrator

---

## Hide Product

```
DELETE /products/{id}
```

Changes the product status to **Hidden**. This is a non-destructive status change, not a database deletion; the underlying
Product record is retained.

Authentication Required

- Administrator

---

## Restore Product

```
PATCH /products/{id}/restore
```

Changes the product status from **Hidden** back to **Available**. Only valid while the product is Hidden — returns a conflict
if attempted on a product in any other status (Sold is a terminal state and is not restorable this way).

Authentication Required

- Administrator

---

## Mark Product Sold

```
PATCH /products/{id}/sell
```

Body: `soldPrice` (required, `>= 0`). Directly marks an Available or Hidden product as **Sold**, recording the given price on
`Product.SoldPrice`, without creating or requiring a purchase request — for sales made outside the online flow (in person,
by phone). Returns `409` if the product is already Sold, `400` for a negative price.

Authentication Required

- Administrator

---

# 4. Categories

## Get Categories

```
GET /categories
```

---

## Create Category

```
POST /categories
```

---


## Update Category

```
PUT /categories/{id}
```

---

## Delete Category

```
DELETE /categories/{id}
```

Authentication Required

- Administrator

---

# 5. Vehicle Brands

## Get Vehicle Brands

```
GET /vehicle-brands
```

---

## Create Vehicle Brand

```
POST /vehicle-brands
```

---

## Update Vehicle Brand

```
PUT /vehicle-brands/{id}
```

---

## Delete Vehicle Brand

```
DELETE /vehicle-brands/{id}
```

Authentication Required

- Administrator

---

# 6. Vehicle Models

## Get Vehicle Models

```
GET /vehicle-models
```

---

## Create Vehicle Model

```
POST /vehicle-models
```

---

## Update Vehicle Model

```
PUT /vehicle-models/{id}
```

---

## Delete Vehicle Model

```
DELETE /vehicle-models/{id}
```

Authentication Required

- Administrator

---

# 7. Favorites

## Get Favorites

```
GET /favorites
```

Returns the authenticated customer's favorite products.

Authentication Required

- Customer

---

## Add Favorite

```
POST /favorites
```

Adds a product to favorites.

Authentication Required

- Customer

---

## Remove Favorite

```
DELETE /favorites/{productId}
```

Removes a product from favorites.

Authentication Required

- Customer

---

# 8. Purchase Requests

## Get Purchase Requests

```
GET /purchase-requests
```

Returns purchase requests belonging to the authenticated customer.

Authentication Required

- Customer

---

## Get Purchase Request Details

```
GET /purchase-requests/{id}
```

Returns detailed information about a purchase request.

Authentication Required

- Customer

---

## Create Purchase Request

```
POST /purchase-requests
```

Creates a new purchase request containing one or more products. The initial status is **Pending**.

Authentication Required

- Customer

---

## Cancel Purchase Request

```
PATCH /purchase-requests/{id}/cancel
```

Cancels a purchase request. Only permitted while the request status is **Pending**.

Authentication Required

- Customer

---

## Confirm Purchase Request

```
PATCH /purchase-requests/{id}/confirm
```

Confirms the purchase request, whether or not it has been negotiated. Permitted while status is **Pending** or
**WaitingForCustomerConfirmation**.

The system automatically changes the status to **Approved**.

Authentication Required

- Customer

---

## Reject Purchase Request

```
PATCH /purchase-requests/{id}/reject
```

Rejects the purchase request. Permitted while status is **Pending** or **WaitingForCustomerConfirmation**.

The system automatically changes the status to **Rejected**.

Authentication Required

- Customer

---

## Review Purchase Requests

```
GET /admin/purchase-requests
```

Returns all purchase requests.

Authentication Required

- Administrator

---

## Update Negotiated Prices

```
PATCH /admin/purchase-requests/{id}/negotiation
```

Updates negotiated prices for one or more purchase request items. Negotiation is optional � a request may reach **Approved**
without ever calling this endpoint, if the customer confirms the original pricing directly.

The system automatically changes the purchase request status to **WaitingForCustomerConfirmation**.

Authentication Required

- Administrator

---

# 9. Product Images

## Upload Product Image

```
POST /products/{id}/images
```

Authentication Required

- Administrator

---

## Delete Product Image

```
DELETE /products/{id}/images/{imageId}
```

Authentication Required

- Administrator

---

# 10. Product Compatibility

## Get Compatible Vehicles

```
GET /products/{id}/compatibility
```

Returns all compatible vehicle models.

Authentication Required

- No

---

## Add Compatibility

```
POST /products/{id}/compatibility
```

Authentication Required

- Administrator

---

## Remove Compatibility

```
DELETE /products/{id}/compatibility/{vehicleModelId}
```

Authentication Required

- Administrator

---

# 11. Customer Profile

## Get Own Profile

```
GET /users/me
```

Returns the authenticated customer's profile information.

Authentication Required

- Customer

---

## Update Own Profile

```
PUT /users/me
```

Updates the authenticated customer's profile information (FirstName, LastName). Email and PhoneNumber changes may require
re-verification and are out of scope for v1.0.

Authentication Required

- Customer

---

# 12. Dashboard

## Get Dashboard Statistics

```
GET /admin/dashboard
```

Returns summary statistics: total products, total customers, pending purchase requests, and products awaiting attention.

Authentication Required

- Administrator

---

# 13. Reports

## Get Statistics Report

```
GET /admin/reports/statistics
```

Generates and returns a PDF statistics report: product counts by status, total customers, pending purchase requests,
per-item sales performance (acquisition cost, sold price, profit) for every Sold product, and the list-price/acquisition-cost
value of current Available inventory. Generated on demand, not cached or persisted.

Query parameter `period` (`AllTime` | `Daily` | `Weekly` | `Monthly`, default `AllTime`) scopes the sales performance
section to a rolling window ending now (BR-79) — every other section always reflects current state.

Response Content-Type: `application/pdf`.

Authentication Required

- Administrator

---

# 14. Acquisition Batches

## List Acquisition Batches

```
GET /admin/acquisition-batches
```

Returns every acquisition batch with its rollup (part count, available/sold/hidden counts, estimated cost per part,
revenue so far, profit so far).

Authentication Required

- Administrator

## Get Acquisition Batch by ID

```
GET /admin/acquisition-batches/{id}
```

Authentication Required

- Administrator

## Create Acquisition Batch

```
POST /admin/acquisition-batches
```

Body: `source`, `totalCost`, `purchaseDate`, `notes` (optional). Records a single lump-sum purchase (BR-72); Products are
linked to it afterward via `POST`/`PUT /products` (`acquisitionBatchId`).

Authentication Required

- Administrator

## Update Acquisition Batch

```
PUT /admin/acquisition-batches/{id}
```

Authentication Required

- Administrator

## Delete Acquisition Batch

```
DELETE /admin/acquisition-batches/{id}
```

Hard delete. Rejected with `409 Conflict` while any Product still references the batch (BR-74).

Authentication Required

- Administrator

---

# 15. HTTP Response Codes

| Code | Description |
|------|-------------|
| 200 OK | Request completed successfully |
| 201 Created | Resource created successfully |
| 204 No Content | Resource deleted or updated successfully |
| 400 Bad Request | Invalid request |
| 401 Unauthorized | Authentication required |
| 403 Forbidden | Access denied |
| 404 Not Found | Resource not found |
| 409 Conflict | Duplicate resource |
| 500 Internal Server Error | Unexpected server error |

---

# 16. Authentication & Authorization

- JWT Bearer Authentication shall be used.
- ASP.NET Core Identity shall manage users.
- Guests may access only public endpoints.
- Customers may access only their own resources.
- Administrators may access all management endpoints.

---

# 17. Business Rules

- Purchase request statuses shall be managed automatically by the system.
- Clients shall never update purchase request statuses directly.
- Product prices shall never be modified during negotiation.
- Negotiated prices shall be stored only in `PurchaseRequestItem` (`negotiatedPrice`).
- The original product price at request creation is stored in `PurchaseRequestItem.originalPrice`.
- Hidden and Sold products shall not appear in public product listings.
- `Product.AcquisitionCost` and `Product.AcquisitionSource` (what the shop paid for the part and where it came from) are
  internal-only fields: they are always `null` in API responses to non-Administrator callers (public or Customer), regardless
  of the product's status, and are only settable by Administrators via `POST`/`PUT /products`.
- An Acquisition Batch records one lump-sum purchase of multiple parts (BR-72); Products link to it optionally via
  `acquisitionBatchId` and are still bought, priced, and sold individually — batches never gate or replace individual sale flow.
- A Product's reported acquisition cost is its own `AcquisitionCost` if set, else an even split of its linked batch's
  `totalCost` across every Product currently linked to that batch (BR-73).
- An Acquisition Batch cannot be deleted while referenced by one or more Products (BR-74).
- A customer cannot open a second active purchase request on a product they already have a `Pending`/
  `WaitingForCustomerConfirmation` request for (BR-75).
- Confirming a purchase request re-verifies every item's product is still `Available`, since only one purchase request can
  ever win a given unique physical Product (BR-76).
- An administrator may mark an Available or Hidden product Sold directly, with a sold price, without a purchase request —
  this system also functions as inventory tracking, and not every sale happens online (BR-78).
- Email verification shall be required before login.
- Negotiation is optional; a purchase request may be approved via direct customer confirmation without a negotiated price.
- Final approval of a purchase request is performed by the Customer via `PATCH /purchase-requests/{id}/confirm`.

---

# Approval

This document defines the REST API exposed by the OtoParcam backend and shall remain consistent with the Business Requirements Document (BRD), Software Requirements Specification (SRS), Database Design Document (DDR), and Use Case Specification throughout the software development lifecycle.