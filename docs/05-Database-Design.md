
# OtoParcam

## Database Design Report (DDR)

**Version:** 1.0

**Author:** Nejdet Vural

**Date:** 16.07.2026

---

# 1. Purpose

This document defines the logical database design of the OtoParcam system.

It describes the database architecture, entity structures, relationships, constraints, naming conventions, and design decisions required for implementation.

This document serves as the primary reference for database development and maintenance.

---

# 2. Database Overview

OtoParcam uses a relational database designed according to Third Normal Form (3NF).

The database prioritizes:

* Data consistency
* Referential integrity
* Scalability
* Maintainability
* Performance

The system follows a Database-First domain design approach.

Database Engine

* PostgreSQL

---

# 3. Design Principles

The database is designed according to the following principles.

* Third Normal Form (3NF)
* One entity represents one business concept.
* Foreign key constraints enforce referential integrity.
* Nullable fields are used only when business rules allow missing values.
* Enumerations are stored as integers.
* Database schema remains language-independent (English).
* Business rules are enforced at the application layer unless database constraints are more appropriate.

---

# 4. Naming Conventions

## Tables

All table names use singular PascalCase.

Examples:

* Product
* Category
* VehicleBrand
* VehicleModel
* PurchaseRequest

---

## Primary Keys

Every table uses a single primary key.

Example:

* Id

---

## Foreign Keys

Foreign keys are named using the referenced entity name followed by **Id**.

Examples:

* ProductId
* CategoryId
* VehicleModelId
* ApplicationUserId
* PurchaseRequestId

---

## Date Fields

Timestamp fields follow a consistent naming convention.

Examples:

* CreatedAt
* UpdatedAt

---

## Boolean Fields

Boolean fields begin with **Is** whenever applicable.

Examples:

* IsActive
* IsDeleted

---

## Enumerations

Database stores enum values as integers.

Business logic converts integer values into strongly typed enums.

---

# 5. Database Entities

The database consists of the following entities.

* Product
* Category
* VehicleBrand
* VehicleModel
* ProductCompatibility
* ProductImage
* PurchaseRequest
* PurchaseRequestItem
* ApplicationUser
* Favorite

Each entity is described in the following sections.

---

# 6. Entity Definitions

Each entity includes:

* Purpose
* Columns
* Constraints
* Relationships
* Notes

# 6.1 Product

## Purpose

The Product entity represents a single unique physical used spare part available for sale.

Each Product corresponds to exactly one physical item and maintains its own category, source vehicle, compatibility information, pricing, inventory status, and images.

---

## Columns

| Column               | PostgreSQL Type          | Nullable | Description                                                              |
| -------------------- | ------------------------ | -------- | ------------------------------------------------------------------------ |
| Id                   | UUID                     | No       | Primary key.                                                             |
| CategoryId           | UUID                     | No       | References the product category.                                         |
| SourceVehicleModelId | UUID                     | No       | References the vehicle model from which the part was removed.            |
| Price                | DECIMAL(10,2)            | Yes      | Product selling price. If NULL, the UI displays **"Fiyat İçin Arayın"**. |
| StockStatus          | INTEGER                  | No       | Product availability status (Enum).                                      |
| Color                | INTEGER                  | No       | Product color (Enum).                                                    |
| Description          | TEXT                     | Yes      | Stores scratches, dents, missing parts and other physical details.       |
| CreatedAt            | TIMESTAMP WITH TIME ZONE | No       | Date and time when the product was created.                              |
| UpdatedAt            | TIMESTAMP WITH TIME ZONE | No       | Date and time of the last update.                                        |

---

## Primary Key

* PK_Product (Id)

---

## Foreign Keys

* FK_Product_Category → Category(Id)
* FK_Product_SourceVehicleModel → VehicleModel(Id)

---

## Constraints

| Constraint                    | Description                                                              |
| ----------------------------- | ------------------------------------------------------------------------ |
| PK_Product                    | Primary Key                                                              |
| FK_Product_Category           | References Category                                                      |
| FK_Product_SourceVehicleModel | References VehicleModel                                                  |
| CK_Product_Price              | Price must be greater than or equal to zero when provided.               |
| CK_Product_ImageCount         | A Product must contain between 1 and 10 images (application validation). |

---

## Database Indexes

| Index                           | Purpose                             |
| ------------------------------- | ----------------------------------- |
| PK_Product                      | Primary key lookup                  |
| IX_Product_CategoryId           | Product filtering by category       |
| IX_Product_SourceVehicleModelId | Product filtering by source vehicle |
| IX_Product_StockStatus          | Availability filtering              |
| IX_Product_Color                | Color filtering                     |

---

## Relationships

| Related Entity       | Cardinality | Description                                               |
| -------------------- | ----------- | --------------------------------------------------------- |
| Category             | Many-to-One | Every Product belongs to one Category.                    |
| VehicleModel         | Many-to-One | Every Product originates from one VehicleModel.           |
| ProductImage         | One-to-Many | A Product contains between 1 and 10 images.               |
| ProductCompatibility | One-to-Many | A Product may be compatible with multiple vehicle models. |
| PurchaseRequestItem  | One-to-Many | A Product may appear in multiple purchase requests.       |
| Favorite             | One-to-Many | A Product may be added to multiple users' favorites.      |

---

## Notes

* Every Product represents one unique physical used spare part.
* Product names are generated dynamically at runtime and are not stored in the database.
* The displayed product title represents the source vehicle.
* Compatibility information is managed through the ProductCompatibility entity.
* Customers are informed that the displayed vehicle is the source vehicle and that compatibility is listed separately.
* Only original used spare parts are supported in Version 1.0.
* Product condition is described only in the Description field.
* Color is stored as an enumeration.
* The first uploaded image is automatically used as the cover image.
* Product images are displayed in their upload order.
* Business rules such as dynamic product name generation and image ordering are enforced by the application layer.


---

# 6.2 Category

## Purpose

The Category entity classifies used spare parts into logical groups to simplify product organization, inventory management, and product searching.

---

## Columns

| Column    | PostgreSQL Type          | Nullable | Description                                  |
| --------- | ------------------------ | -------- | -------------------------------------------- |
| Id        | UUID                     | No       | Primary key.                                 |
| Name      | VARCHAR(100)             | No       | Category name. Must be unique.               |
| CreatedAt | TIMESTAMP WITH TIME ZONE | No       | Date and time when the category was created. |
| UpdatedAt | TIMESTAMP WITH TIME ZONE | No       | Date and time of the last update.            |

---

## Primary Key

* PK_Category (Id)

---

## Foreign Keys

None.

---

## Constraints

| Constraint       | Description                    |
| ---------------- | ------------------------------ |
| PK_Category      | Primary Key                    |
| UQ_Category_Name | Category names must be unique. |

---

## Database Indexes

| Index            | Purpose                                         |
| ---------------- | ----------------------------------------------- |
| PK_Category      | Primary key lookup                              |
| UX_Category_Name | Fast category lookup and uniqueness enforcement |

---

## Relationships

| Related Entity | Cardinality | Description                             |
| -------------- | ----------- | --------------------------------------- |
| Product        | One-to-Many | One Category may contain many Products. |

---

## Notes

* Every Product must belong to exactly one Category.
* Categories are managed only by administrators.
* New categories may be created at any time.
* Categories are displayed alphabetically.
* Categories cannot be deleted while referenced by one or more Products.
* Category descriptions are not supported in Version 1.0.
* Product position (Left, Right, Front, Rear, etc.) is intentionally excluded from Category because it depends on the specific spare part and will be handled separately in future versions.

---

# 6.3 VehicleBrand

## Purpose

The VehicleBrand entity represents vehicle manufacturers and serves as the top-level classification for vehicle models.

It enables users to browse and filter products by vehicle brand.

---

## Columns

| Column    | PostgreSQL Type          | Nullable | Description                               |
| --------- | ------------------------ | -------- | ----------------------------------------- |
| Id        | UUID                     | No       | Primary key.                              |
| Name      | VARCHAR(100)             | No       | Vehicle brand name. Must be unique.       |
| CreatedAt | TIMESTAMP WITH TIME ZONE | No       | Date and time when the brand was created. |
| UpdatedAt | TIMESTAMP WITH TIME ZONE | No       | Date and time of the last update.         |

---

## Primary Key

* PK_VehicleBrand (Id)

---

## Foreign Keys

None.

---

## Constraints

| Constraint           | Description                         |
| -------------------- | ----------------------------------- |
| PK_VehicleBrand      | Primary Key                         |
| UQ_VehicleBrand_Name | Vehicle brand names must be unique. |

---

## Database Indexes

| Index                | Purpose                                |
| -------------------- | -------------------------------------- |
| PK_VehicleBrand      | Primary key lookup                     |
| UX_VehicleBrand_Name | Fast lookup and uniqueness enforcement |

---

## Relationships

| Related Entity | Cardinality | Description                                      |
| -------------- | ----------- | ------------------------------------------------ |
| VehicleModel   | One-to-Many | One VehicleBrand may contain many VehicleModels. |

---

## Notes

* Every VehicleModel belongs to exactly one VehicleBrand.
* Vehicle brands are managed only by administrators.
* Vehicle brands are displayed alphabetically.
* Vehicle brands cannot be deleted while referenced by one or more VehicleModels.
* Brand names follow their official manufacturer names.

---

# 6.4 VehicleModel

## Purpose

The VehicleModel entity represents a specific vehicle model within a vehicle brand.

It defines the production year range of the model and provides the source vehicle information used for product titles and compatibility matching.

---

## Columns

| Column         | PostgreSQL Type          | Nullable | Description                                                    |
| -------------- | ------------------------ | -------- | -------------------------------------------------------------- |
| Id             | UUID                     | No       | Primary key.                                                   |
| VehicleBrandId | UUID                     | No       | References the vehicle brand.                                  |
| Name           | VARCHAR(100)             | No       | Vehicle model name (e.g., Megane, Astra, Focus).               |
| StartYear      | SMALLINT                 | No       | First production year of the model.                            |
| EndYear        | SMALLINT                 | No       | Last production year of the model.                             |
| Variant        | VARCHAR(100)             | Yes      | Optional trim/package information (e.g., GT Line, Touch, Joy). |
| CreatedAt      | TIMESTAMP WITH TIME ZONE | No       | Date and time when the model was created.                      |
| UpdatedAt      | TIMESTAMP WITH TIME ZONE | No       | Date and time of the last update.                              |

---

## Primary Key

* PK_VehicleModel (Id)

---

## Foreign Keys

* FK_VehicleModel_VehicleBrand → VehicleBrand(Id)

---

## Constraints

| Constraint                   | Description                                        |
| ---------------------------- | -------------------------------------------------- |
| PK_VehicleModel              | Primary Key                                        |
| FK_VehicleModel_VehicleBrand | References VehicleBrand                            |
| CK_VehicleModel_YearRange    | EndYear must be greater than or equal to StartYear |

---

## Database Indexes

| Index                             | Purpose                                 |h
| --------------------------------- | --------------------------------------- |
| PK_VehicleModel                   | Primary key lookup                      |
| IX_VehicleModel_VehicleBrandId    | Fast filtering by brand                 |
| IX_VehicleModel_Name              | Fast model lookup                       |
| IX_VehicleModel_StartYear_EndYear | Efficient filtering by production years |

---

## Relationships

| Related Entity       | Cardinality | Description                                                   |
| -------------------- | ----------- | ------------------------------------------------------------- |
| VehicleBrand         | Many-to-One | Every VehicleModel belongs to one VehicleBrand.               |
| Product              | One-to-Many | One VehicleModel may be the source vehicle for many Products. |
| ProductCompatibility | One-to-Many | One VehicleModel may be compatible with many Products.        |

---

## Notes

* Vehicle models are managed only by administrators.
* Users search vehicles by production year rather than generation names.
* Generation names (e.g., Megane III, B8, MK4) are not stored separately in Version 1.0.
* Variant is optional and is not used for filtering.
* Variant may be included in dynamically generated product titles.
* Vehicle models cannot be deleted while referenced by Products or ProductCompatibility records.


---

# 6.5 ProductCompatibility

## Purpose

The ProductCompatibility entity defines compatibility between a Product and one or more VehicleModels.

It allows a single spare part to be associated with multiple compatible vehicle models while preserving the original source vehicle information stored by the Product entity.

---

## Columns

| Column         | PostgreSQL Type          | Nullable | Description                                              |
| -------------- | ------------------------ | -------- | -------------------------------------------------------- |
| Id             | UUID                     | No       | Primary key.                                             |
| ProductId      | UUID                     | No       | References the compatible product.                       |
| VehicleModelId | UUID                     | No       | References a compatible vehicle model.                   |
| CreatedAt      | TIMESTAMP WITH TIME ZONE | No       | Date and time when the compatibility record was created. |

---

## Primary Key

* PK_ProductCompatibility (Id)

---

## Foreign Keys

* FK_ProductCompatibility_Product → Product(Id)
* FK_ProductCompatibility_VehicleModel → VehicleModel(Id)

---

## Constraints

| Constraint                           | Description                                                                |
| ------------------------------------ | -------------------------------------------------------------------------- |
| PK_ProductCompatibility              | Primary Key                                                                |
| FK_ProductCompatibility_Product      | References Product                                                         |
| FK_ProductCompatibility_VehicleModel | References VehicleModel                                                    |
| UQ_ProductCompatibility              | The same Product and VehicleModel combination cannot exist more than once. |

---

## Database Indexes

| Index                                            | Purpose                                 |
| ------------------------------------------------ | --------------------------------------- |
| PK_ProductCompatibility                          | Primary key lookup                      |
| UX_ProductCompatibility_ProductId_VehicleModelId | Prevent duplicate compatibility records |
| IX_ProductCompatibility_ProductId                | Fast compatibility lookup by product    |
| IX_ProductCompatibility_VehicleModelId           | Fast product lookup by vehicle model    |

---

## Relationships

| Related Entity | Cardinality | Description                                             |
| -------------- | ----------- | ------------------------------------------------------- |
| Product        | Many-to-One | Every compatibility record belongs to one Product.      |
| VehicleModel   | Many-to-One | Every compatibility record references one VehicleModel. |

---

## Notes

* A Product may be compatible with multiple VehicleModels.
* A VehicleModel may be compatible with multiple Products.
* Compatibility records are created manually by administrators.
* The source vehicle is stored separately in the Product entity.
* Compatibility information is used for product search and filtering.
* Duplicate compatibility records are not allowed.

---

# 6.6 ProductImage

## Purpose

The ProductImage entity stores images associated with a Product.

It defines the display order of product images, where the first uploaded image is automatically used as the cover image.

---

## Columns

| Column       | PostgreSQL Type          | Nullable | Description                                    |
| ------------ | ------------------------ | -------- | ---------------------------------------------- |
| Id           | UUID                     | No       | Primary key.                                   |
| ProductId    | UUID                     | No       | References the associated Product.             |
| ImageUrl     | VARCHAR(500)             | No       | Relative or absolute path of the stored image. |
| DisplayOrder | SMALLINT                 | No       | Determines the display order of the image.     |
| CreatedAt    | TIMESTAMP WITH TIME ZONE | No       | Date and time when the image was uploaded.     |

---

## Primary Key

* PK_ProductImage (Id)

---

## Foreign Keys

* FK_ProductImage_Product → Product(Id)

---

## Constraints

| Constraint                   | Description                                                              |
| ---------------------------- | ------------------------------------------------------------------------ |
| PK_ProductImage              | Primary Key                                                              |
| FK_ProductImage_Product      | References Product                                                       |
| CK_ProductImage_DisplayOrder | DisplayOrder must be greater than zero.                                  |
| UQ_ProductImage_DisplayOrder | DisplayOrder must be unique within the same Product.                     |
| CK_ProductImage_Count        | A Product must contain between 1 and 10 images (application validation). |

---

## Database Indexes

| Index                                  | Purpose                                  |
| -------------------------------------- | ---------------------------------------- |
| PK_ProductImage                        | Primary key lookup                       |
| IX_ProductImage_ProductId              | Fast image retrieval by Product          |
| UX_ProductImage_ProductId_DisplayOrder | Ensures unique display order per Product |

---

## Relationships

| Related Entity | Cardinality | Description                                |
| -------------- | ----------- | ------------------------------------------ |
| Product        | Many-to-One | Every ProductImage belongs to one Product. |

---

## Notes

* Every Product must have at least one image.
* A Product may contain up to ten images.
* Images are displayed according to DisplayOrder.
* The image with DisplayOrder = 1 is automatically used as the cover image.
* Cover images are determined by display order; no separate IsCover field is stored.
* Images are managed only by administrators.

---
# 6.7 Favorite

## Purpose

The Favorite entity allows registered users to save products for future reference.

Each favorite record represents a relationship between one ApplicationUser and one Product.

---

## Columns

| Column            | PostgreSQL Type          | Nullable | Description                                            |
| ----------------- | ------------------------ | -------- | ------------------------------------------------------ |
| Id                | UUID                     | No       | Primary key.                                           |
| ApplicationUserId | UUID                     | No       | References the user who favorited the product.         |
| ProductId         | UUID                     | No       | References the favorited product.                      |
| CreatedAt         | TIMESTAMP WITH TIME ZONE | No       | Date and time when the product was added to favorites. |

---

## Primary Key

* PK_Favorite (Id)

---

## Foreign Keys

* FK_Favorite_ApplicationUser → ApplicationUser(Id)
* FK_Favorite_Product → Product(Id)

---

## Constraints

| Constraint                  | Description                                             |
| --------------------------- | ------------------------------------------------------- |
| PK_Favorite                 | Primary Key                                             |
| FK_Favorite_ApplicationUser | References ApplicationUser                              |
| FK_Favorite_Product         | References Product                                      |
| UQ_Favorite                 | A user cannot favorite the same product more than once. |

---

## Database Indexes

| Index                                   | Purpose                                      |
| --------------------------------------- | -------------------------------------------- |
| PK_Favorite                             | Primary key lookup                           |
| IX_Favorite_ApplicationUserId           | Fast retrieval of a user's favorites         |
| IX_Favorite_ProductId                   | Fast lookup of users who favorited a product |
| UX_Favorite_ApplicationUserId_ProductId | Prevents duplicate favorite records          |

---

## Relationships

| Related Entity  | Cardinality | Description                                    |
| --------------- | ----------- | ---------------------------------------------- |
| ApplicationUser | Many-to-One | Every Favorite belongs to one ApplicationUser. |
| Product         | Many-to-One | Every Favorite references one Product.         |

---

## Notes

* Only authenticated users may create favorites.
* Favorite products are private to each user.
* A favorite may be removed at any time.
* Removing a favorite permanently deletes the record.
* The same product cannot be added to favorites more than once by the same user.


---

# 6.8 ApplicationUser

## Purpose

The ApplicationUser entity represents authenticated users of the system.

It extends ASP.NET Core Identity and stores customer-specific information required by the application.

Authorization is handled through ASP.NET Core Identity Roles rather than separate Customer and Administrator entities.

---

## Columns

| Column    | PostgreSQL Type          | Nullable | Description            |
| --------- | ------------------------ | -------- | ---------------------- |
| Id        | UUID                     | No       | Primary key.           |
| FirstName | VARCHAR(100)             | No       | User first name.       |
| LastName  | VARCHAR(100)             | No       | User last name.        |
| CreatedAt | TIMESTAMP WITH TIME ZONE | No       | Account creation date. |
| UpdatedAt | TIMESTAMP WITH TIME ZONE | No       | Last profile update.   |

---

## Primary Key

* PK_AspNetUsers (Id)

---

## Foreign Keys

None.

---

## Constraints

| Constraint     | Description                     |
| -------------- | ------------------------------- |
| PK_AspNetUsers | Primary Key                     |
| UQ_Email       | Email addresses must be unique. |
| UQ_PhoneNumber | Phone numbers must be unique.   |

---

## Database Indexes

| Index          | Purpose                     |
| -------------- | --------------------------- |
| PK_AspNetUsers | Primary key lookup          |
| UX_Email       | User authentication         |
| UX_PhoneNumber | Phone number authentication |

---

## Relationships

| Related Entity  | Cardinality | Description                                 |
| --------------- | ----------- | ------------------------------------------- |
| Favorite        | One-to-Many | One user may have many favorite products.   |
| PurchaseRequest | One-to-Many | One user may create many purchase requests. |

---

## Notes

* ApplicationUser extends ASP.NET Core Identity.
* Authentication is provided by ASP.NET Core Identity.
* Authorization is role-based.
* Users may authenticate using either their email address or phone number.
* Passwords are stored only as hashed values.
* Usernames are not used in Version 1.0.
* Profile photos are not supported.
* Address information is not stored.
* Customer accounts are never physically deleted.

---

# 6.9 PurchaseRequest

## Purpose

The PurchaseRequest entity represents a customer's purchase request submitted for one or more products.

It manages the complete lifecycle of a purchase request, including price negotiation, customer confirmation, approval, rejection, and cancellation.

---

## Columns

| Column            | PostgreSQL Type          | Nullable | Description                                               |
| ----------------- | ------------------------ | -------- | --------------------------------------------------------- |
| Id                | UUID                     | No       | Primary key.                                              |
| ApplicationUserId | UUID                     | No       | References the customer who created the purchase request. |
| Status            | INTEGER                  | No       | Current purchase request status (Enum).                   |
| CreatedAt         | TIMESTAMP WITH TIME ZONE | No       | Date and time when the purchase request was created.      |
| UpdatedAt         | TIMESTAMP WITH TIME ZONE | No       | Date and time of the last update.                         |

---

## Primary Key

* PK_PurchaseRequest (Id)

---

## Foreign Keys

* FK_PurchaseRequest_ApplicationUser → ApplicationUser(Id)

---

## Constraints

| Constraint                         | Description                |
| ---------------------------------- | -------------------------- |
| PK_PurchaseRequest                 | Primary Key                |
| FK_PurchaseRequest_ApplicationUser | References ApplicationUser |

---

## Database Indexes

| Index                                | Purpose                                 |
| ------------------------------------ | --------------------------------------- |
| PK_PurchaseRequest                   | Primary key lookup                      |
| IX_PurchaseRequest_ApplicationUserId | Retrieve purchase requests by customer  |
| IX_PurchaseRequest_Status            | Filter purchase requests by status      |
| IX_PurchaseRequest_CreatedAt         | Sort purchase requests by creation date |

---

## Relationships

| Related Entity      | Cardinality | Description                                                    |
| ------------------- | ----------- | -------------------------------------------------------------- |
| ApplicationUser     | Many-to-One | Every PurchaseRequest belongs to one ApplicationUser.          |
| PurchaseRequestItem | One-to-Many | One PurchaseRequest contains one or more PurchaseRequestItems. |

---

## Notes

* A PurchaseRequest represents a customer's purchase request rather than a completed sale.
* A PurchaseRequest must contain at least one PurchaseRequestItem.
* Customers may create multiple PurchaseRequests.
* Purchase requests are reviewed by administrators.
* When a negotiated price is entered by the administrator, the purchase request status becomes **WaitingForCustomerConfirmation**.
* If the customer accepts the negotiated price, the purchase request is automatically marked as **Approved**.
* Product availability is updated only after the purchase request reaches the **Approved** status.
* Status transitions are controlled by the application layer.

---

# 6.10 PurchaseRequestItem

## Purpose

The PurchaseRequestItem entity represents an individual product within a PurchaseRequest.

It stores the product price at the time of the request and any negotiated price agreed during the purchase process.

---

## Columns

| Column            | PostgreSQL Type | Nullable | Description                                          |
| ----------------- | --------------- | -------- | ---------------------------------------------------- |
| Id                | UUID            | No       | Primary key.                                         |
| PurchaseRequestId | UUID            | No       | References the associated PurchaseRequest.           |
| ProductId         | UUID            | No       | References the requested Product.                    |
| PriceSnapshot     | DECIMAL(10,2)   | Yes      | Product price when the purchase request was created. |
| NegotiatedPrice   | DECIMAL(10,2)   | Yes      | Final negotiated price for this product.             |

---

## Primary Key

* PK_PurchaseRequestItem (Id)

---

## Foreign Keys

* FK_PurchaseRequestItem_PurchaseRequest → PurchaseRequest(Id)
* FK_PurchaseRequestItem_Product → Product(Id)

---

## Constraints

| Constraint                             | Description                                                                    |
| -------------------------------------- | ------------------------------------------------------------------------------ |
| PK_PurchaseRequestItem                 | Primary Key                                                                    |
| FK_PurchaseRequestItem_PurchaseRequest | References PurchaseRequest                                                     |
| FK_PurchaseRequestItem_Product         | References Product                                                             |
| UQ_PurchaseRequestItem                 | The same product cannot appear more than once within the same PurchaseRequest. |

---

## Database Indexes

| Index                                              | Purpose                                                     |
| -------------------------------------------------- | ----------------------------------------------------------- |
| PK_PurchaseRequestItem                             | Primary key lookup                                          |
| IX_PurchaseRequestItem_PurchaseRequestId           | Retrieve items of a purchase request                        |
| IX_PurchaseRequestItem_ProductId                   | Find purchase history of a product                          |
| UX_PurchaseRequestItem_PurchaseRequestId_ProductId | Prevent duplicate products within the same purchase request |

---

## Relationships

| Related Entity  | Cardinality | Description                                               |
| --------------- | ----------- | --------------------------------------------------------- |
| PurchaseRequest | Many-to-One | Every PurchaseRequestItem belongs to one PurchaseRequest. |
| Product         | Many-to-One | Every PurchaseRequestItem references one Product.         |

---

## Notes

* Every PurchaseRequestItem references exactly one Product.
* A PurchaseRequest must contain at least one PurchaseRequestItem.
* PriceSnapshot stores the product price when the purchase request is created.
* NegotiatedPrice is optional and is entered by the administrator after a successful phone negotiation.
* Updating NegotiatedPrice never changes the Product price.
* Final payable amounts are calculated from PurchaseRequestItems and are not stored in the PurchaseRequest entity.
* Quantity is not supported because every Product represents a unique physical spare part.



# 7. Relationships

Entity relationships are defined using foreign keys.

Relationship cardinality is documented in the ER Diagram.

---

# 8. Constraints

The database enforces:

* Primary Keys
* Foreign Keys
* Unique Constraints
* NOT NULL Constraints
* CHECK Constraints where appropriate

Business-specific validation remains the responsibility of the application layer.

---

# 9. Index Strategy

Indexes are created only where necessary to improve query performance.

Typical indexed columns include:

* Foreign Keys
* Frequently searched fields
* Unique fields
* Status fields

---

# 10. Future Database Considerations

Future versions may introduce:

* Soft Delete
* Audit Tables
* Supplier Management
* Multi-Branch Support
* Barcode Integration
* QR Code Integration
* Sales History
* Archive Tables
* Database Partitioning

---

# 11. Approval

This document represents the initial logical database design for OtoParcam and may evolve as new system requirements are introduced.

