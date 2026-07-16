## OtoParcam - Entity Notes

Purpose:
This document records design decisions, discussions, and progress for each entity. It serves as the primary reference for
continuing the project in future conversations.

| Entity | Status |
|---------|--------|
| Product | ✅ Completed |
| Category | ✅ Completed |
| VehicleBrand | ✅ Completed |
| VehicleModel | ✅ Completed |
| ProductCompatibility | ✅ Completed |
| ProductImage | ✅ Completed |
| PurchaseRequest | 🟡 In Progress |
| PurchaseRequestItem | 🟡 In Progress |
| ApplicationUser | ✅ Completed |
| Favorite | ✅ Completed |


## Product

✔ Every physical spare part is a separate Product.

✔ Product names are generated dynamically and are not stored in the database.

✔ Stock quantity will not be used.

✔ Price is optional.

✔ If Price is null, UI displays "Fiyat İçin Arayın".

✔ Description stores scratches, dents and other physical details.

✔ Color uses enum.

✔ No Condition field.

✔ No ProductCode (v1.0).

✔ No InternalNote (v1.0).

✔ No PartOrigin (only original used parts are sold).

✔ One SourceVehicleModel.	

✔ A Product may have multiple CompatibleVehicleModels.

✔ Compatibility selection is optional.

✔ Compatibility is selected manually by the administrator.

✔ Product title represents the source vehicle.

✔ Customers are informed that compatibility is listed separately.

✔ The first uploaded image shall be used as the cover image.

✔ Images shall be displayed in the upload order.

✔ A Product may contain between 1 and 10 images.

✔ Side and Position can be nullable.

+----------------------+
| Product              |
+----------------------+
| Id                   |
| CategoryId           |
| SourceVehicleModelId |
| Price                |
| Status               |
| Side                 |
| Position             |
| Color                |
| Description          |
| CreatedAt            |
| UpdatedAt            |
+----------------------+


---

## Category

✔ A Category represents a spare part type.

✔ Category names must be unique.

✔ A Category may contain multiple Products.

✔ A Category cannot be deleted while it is referenced by one or more Products.

✔ Administrators may create new categories.

✔ Categories are displayed alphabetically.

✔ Categories do not use hierarchical relationships in v1.0.

+----------------------+
| Category             |
+----------------------+
| Id                   |
| Name                 |
| CreatedAt            |
| UpdatedAt            |
+----------------------+


Category (1)
      │
      └──────────────< Product (N)

---

## VehicleBrand

✔ A VehicleBrand represents a vehicle manufacturer.

✔ VehicleBrand names must be unique.

✔ A VehicleBrand may contain multiple VehicleModels.

✔ A VehicleBrand cannot be deleted while it is referenced by one or more VehicleModels.

✔ Administrators may create new vehicle brands.

✔ Vehicle brands are displayed alphabetically.


+----------------------+
| VehicleBrand         |
+----------------------+
| Id                   |
| Name                 |
| CreatedAt            |
| UpdatedAt            |
+----------------------+

VehicleBrand (1)
        │
        └──────────────< VehicleModel (N)


---

## VehicleModel

## VehicleModel

✔ A VehicleModel belongs to exactly one VehicleBrand.

✔ VehicleModel names are stored separately from the production years.

✔ StartYear and EndYear define the production period of the model.

✔ StartYear must be less than or equal to EndYear. 

✔ Variant is optional.

✔ Variant stores manufacturer-specific model identifiers (e.g., Megane 3, F30, W204, B7).

✔ Variant is used only for display purposes.

✔ Variant shall not be used for searching or filtering in v1.0.

✔ Administrators may create new vehicle models.

✔ Vehicle models are displayed alphabetically within their brands.

✔ A VehicleModel cannot be deleted while it is referenced by Products or ProductCompatibility records.

+----------------------+
| VehicleModel         |
+----------------------+
| Id                   |
| VehicleBrandId       |
| Name                 |
| StartYear            |
| EndYear              |
| Variant              |
| CreatedAt            |
| UpdatedAt            |
+----------------------+

VehicleModel (1)
        │
        ├──────────────< Product (SourceVehicleModel)
        │
        └──────────────< ProductCompatibility


---

## ProductCompatibility

✔ ProductCompatibility represents the compatibility between a Product and a VehicleModel.

✔ A Product may be compatible with multiple VehicleModels.

✔ A VehicleModel may be compatible with multiple Products.

✔ Compatibility records are created manually by administrators.

✔ Duplicate compatibility records are not allowed.

✔ Compatibility information is based only on the vehicle model in v1.0.

✔ Engine type, fuel type, transmission, package and other technical differences are not stored.

✔ Additional compatibility details may be written in the Product description or communicated by phone.

+-------------------------------+
| ProductCompatibility          |
+-------------------------------+
| ProductId (PK)(FK)            |
| VehicleModelId (PK)(FK)       |
+-------------------------------+

---

## ProductImage

✔ A Product may contain between 1 and 10 images.

✔ Every Product must have at least one image.

✔ The first uploaded image is used as the cover image.

✔ Images are displayed according to their upload order.

✔ Image order is stored using DisplayOrder.

✔ Images belong to exactly one Product.

+----------------------+
| ProductImage         |
+----------------------+
| Id                   |
| ProductId            |
| ImageUrl             |
| DisplayOrder         |
| CreatedAt            |
+----------------------+    

Product (1)
      │
      └──────────────< ProductImage (N)

## PurchaseRequest

✔ A PurchaseRequest represents a customer's purchase request.

✔ A PurchaseRequest belongs to exactly one Customer.

✔ A PurchaseRequest may contain one or more PurchaseRequestItems.

✔ Multiple customers may submit purchase requests for the same Product.

✔ PurchaseRequest status is managed at the request level.

✔ Total price is calculated from PurchaseRequestItems and is not stored in version 1.0.

✔ Customers may cancel a purchase request only while its status is Pending.

✔ Administrators review every purchase request.

✔ Purchase requests are never physically deleted.

✔ Approved, Rejected and Cancelled are terminal statuses.

✔ Customer notes are not supported in version 1.0.

✔ Administrator notes are not supported in version 1.0.

+--------------------------+
| PurchaseRequest          |
+--------------------------+
| Id                       |
| CustomerId               |
| Status                   |
| CreatedAt                |
| UpdatedAt                |
+--------------------------+

ApplicationUser (1)
      │
      └──────────────< PurchaseRequest (N)
                              │
                              │
                              ▼
                    PurchaseRequestItem (N)
                       ▲               ▲
                       │               │
                       │               │
          PurchaseRequest (1)     Product (1)

---

## PurchaseRequestItem

✔ Each PurchaseRequestItem represents one requested Product.

✔ Each item references exactly one Product.

✔ OriginalPrice stores the product price at the time the request is created.

✔ NegotiatedPrice is optional.

✔ NegotiatedPrice does not modify the Product price.

✔ Price negotiation is performed per item.

✔ Total request price is calculated by summing item prices.

✔ If a requested Product becomes unavailable, the administrator contacts the customer to decide how to proceed.

✔ Quantity is not stored because each Product represents one unique physical spare part.

+--------------------------------+
| PurchaseRequestItem            |
+--------------------------------+
| Id                             |
| PurchaseRequestId              |
| ProductId                      |
| OriginalPrice                  |
| NegotiatedPrice                |
+--------------------------------+

ApplicationUser (1)
      │
      └──────────────< PurchaseRequest (N)
                              │
                              │
                              ▼
                    PurchaseRequestItem (N)
                       ▲               ▲
                       │               │
                       │               │
          PurchaseRequest (1)     Product (1)

---

## ApplicationUser

✔ ApplicationUser represents every authenticated user in the system.

✔ Administrators and customers are distinguished by roles.

✔ Registration is required before creating a customer account.

✔ Guests may browse products without authentication.

✔ Email is required.

✔ PhoneNumber is required.

✔ FirstName and LastName are stored separately.

✔ Email shall be unique.

✔ PhoneNumber shall be unique.

✔ Users may authenticate using either their email address or phone number.

✔ Passwords shall be stored as hashed values.

✔ Customer users may have multiple PurchaseRequests.

✔ Customer users may have multiple Favorite products.

✔ User accounts are never physically deleted.

✔ Profile photos are not supported.

✔ Address information is not stored.

✔ Username is not used.

✔ Customers create their own accounts.

✔ Favorites require authentication.

✔ Authentication and role management are handled by ASP.NET Core Identity.


+--------------------------+
| ApplicationUser          |
+--------------------------+
| Id                       |
| FirstName                |
| LastName                 |
| Email                    |
| PhoneNumber              |
| PasswordHash             |
| CreatedAt                |
| UpdatedAt                |
+--------------------------+

                    1
ApplicationUser -----------< PurchaseRequest
        │
        │1
        │
        └-----------< Favorite
                    *


## Open Questions

- Should email verification be required before login?
- Should ASP.NET Identity be used as the authentication system?

---

## Favorite

✔ Favorite represents a product saved by a customer for future reference.

✔ Favorites require authentication.

✔ A customer may have multiple favorite products.

✔ A product may be favorited by multiple customers.

✔ The same product cannot be added to favorites more than once by the same customer.

✔ Removing a favorite shall not affect the associated product.

✔ Deleting a product shall also remove its favorite records.

✔ Favorite records store the creation date.

+---------------------------+
| Favorite                  |
+---------------------------+
| Id                        |
| ApplicationUserId         |
| ProductId                 |
| CreatedAt                 |
+---------------------------+

ApplicationUser (1)
        │
        │
        ▼
   Favorite (*)
        ▲
        │
        │
Product (1)     


## Constraints

✔ (ApplicationUserId, ProductId) shall be unique.

✔ ApplicationUserId is a foreign key.

✔ ProductId is a foreign key.

✔ CreatedAt is automatically assigned when the favorite is created.

---
    
## PurchaseRequest

✔ A PurchaseRequest represents a customer's request to purchase one or more products.

✔ Every PurchaseRequest belongs to exactly one ApplicationUser.

✔ A PurchaseRequest contains one or more PurchaseRequestItems.

✔ PurchaseRequests use predefined status values.

✔ Negotiation is performed outside the system (e.g., by phone).

✔ Administrators record the negotiated price after a successful negotiation.

✔ If the negotiated price is updated, the PurchaseRequest status becomes WaitingForCustomer.

✔ Customers confirm that the negotiated price entered into the system matches the agreed price.

✔ PurchaseRequests are never physically deleted.

✔ PurchaseRequest items cannot be modified after the request is created.

✔ Total price is calculated from PurchaseRequestItems and is not stored.

+---------------------------+
| PurchaseRequest           |
+---------------------------+
| Id                        |
| ApplicationUserId         |
| Status                    |
| CreatedAt                 |
| UpdatedAt                 |
+---------------------------+

ApplicationUser (1)
        │
        │
        ▼
PurchaseRequest (*)
        │
        │
        ▼
PurchaseRequestItem (*)
        │
        │
        ▼
Product (1)

----

## PurchaseRequestItem

✔ Every PurchaseRequestItem belongs to exactly one PurchaseRequest.

✔ Every PurchaseRequestItem references exactly one Product.

✔ Quantity is not stored because each Product represents one unique physical spare part.

✔ OriginalPrice stores the product price at the time the request is created.

✔ NegotiatedPrice is optional.

✔ NegotiatedPrice does not modify the original Product price.

✔ Each PurchaseRequestItem maintains its own independent pricing information.

+---------------------------+
| PurchaseRequestItem       |
+---------------------------+
| Id                        |
| PurchaseRequestId         |
| ProductId                 |
| OriginalPrice             |
| NegotiatedPrice           |
+---------------------------+
PurchaseRequest (1)
        │
        │
        ▼
PurchaseRequestItem
        ▲
        │
        │
Product (1)


# Global Design Decisions

✔ Every physical spare part is a separate Product.

✔ Product names are generated dynamically and are not stored.

✔ Only original used parts are supported in version 1.0.

✔ All user-facing text is in Turkish.

✔ All source code, database schema and documentation are written in English.

✔ The application follows a database-first domain design approach.

✔ Business decisions are finalized before implementation.

✔ Authentication is implemented using ASP.NET Core Identity.

✔ Administrators and customers are distinguished by roles rather than separate entities.

✔ Every foreign key is named using the referenced entity name followed by "Id" (e.g., ProductId, CategoryId, ApplicationUserId).