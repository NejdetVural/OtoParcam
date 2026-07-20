# OtoParcam

## API Design Specification

Version 1.1

Author Nejdet Vural

Date 17.07.2026

---

## Changelog

- **Added `GET /products` query parameters** to support FR-03 (search) and FR-16 (filtering) — previously undocumented.
- **Added purchase request cancellation endpoint** (`PATCH /purchase-requests/{id}/cancel`) to support the customer-cancellation
  rule already stated in EntityProcess.md ("Customers may cancel a purchase request only while its status is Pending"), which had
  no corresponding endpoint.
- **Added Dashboard section** (`GET /admin/dashboard`) to support FR-10, previously listed as core v1.0 scope in the Project Vision
  and SRS but absent from this spec.
- **Added Customer Profile section** (`GET/PUT /users/me`) to support the Project Vision's "Manage profile information" capability,
  previously absent from this spec.
- **Clarified `DELETE /products/{id}`** as a non-destructive status change, to avoid future ambiguity with a hard delete.
- **Clarified negotiation as optional** in Section 8 — confirmed the confirm/reject endpoints apply whether or not
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

Returns a paginated list of available products.

Supports filtering, searching, and sorting using optional query parameters.

| Parameter | Type | Description |
|------------|------|-------------|
| `categoryId` | UUID | Filter by category |
| `vehicleBrandId` | UUID | Filter by vehicle brand |
| `vehicleModelId` | UUID | Filter by vehicle model (source or compatible) |
| `keyword` | string | Searches product descriptions |
| `color` | integer | Filter by color |
| `page` | integer | Page number (default: 1) |
| `sortBy` | string | `priceAsc` or `priceDesc` |

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

Updates negotiated prices for one or more purchase request items. Negotiation is optional — a request may reach **Approved**
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

# 13. HTTP Response Codes

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

# 14. Authentication & Authorization

- JWT Bearer Authentication shall be used.
- ASP.NET Core Identity shall manage users.
- Guests may access only public endpoints.
- Customers may access only their own resources.
- Administrators may access all management endpoints.

---

# 15. Business Rules

- Purchase request statuses shall be managed automatically by the system.
- Clients shall never update purchase request statuses directly.
- Product prices shall never be modified during negotiation.
- Negotiated prices shall be stored only in `PurchaseRequestItem` (`negotiatedPrice`).
- The original product price at request creation is stored in `PurchaseRequestItem.originalPrice`.
- Hidden and Sold products shall not appear in public product listings.
- Email verification shall be required before login.
- Negotiation is optional; a purchase request may be approved via direct customer confirmation without a negotiated price.
- Final approval of a purchase request is performed by the Customer via `PATCH /purchase-requests/{id}/confirm`.

---

# Approval

This document defines the REST API exposed by the OtoParcam backend and shall remain consistent with the Business Requirements Document (BRD), Software Requirements Specification (SRS), Database Design Document (DDR), and Use Case Specification throughout the software development lifecycle.