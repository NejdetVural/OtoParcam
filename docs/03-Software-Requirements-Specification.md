# OtoParcam

## Software Requirements Specification (SRS)

Version 1.0

Author Nejdet Vural

Date 13.07.2026

---

# 1. Introduction

## 1.1 Purpose

The purpose of this document is to define the functional and non-functional requirements of the OtoParcam system. This document serves as a reference for the design, implementation, testing, and maintenance phases of the project.

---

## 1.2 Scope

OtoParcam is a web-based spare part catalog and sales management system developed for businesses selling used auto spare parts.

The system allows customers to search for compatible spare parts based on vehicle information while providing administrators with tools to manage products, categories, vehicle data, customer accounts, and purchase requests.

---

## 1.3 Definitions

| Term | Description |
|------|-------------|
| Admin | System administrator with full permissions |
| Customer | Registered user who can submit purchase requests |
| Guest | Unauthenticated visitor |
| Product | Used auto spare part |
| Purchase Request | Customer request submitted to purchase one or more spare parts |
| Favorite | Product bookmarked by a registered customer |
| Compatibility | Relationship indicating that a spare part is compatible with a vehicle model |
| Source Vehicle | The vehicle from which the spare part was removed |

---

## 1.4 References

- Project Vision Document
- Business Requirements Document (BRD)
- Database Design Document (DDR)

---

# 2. Overall Description

## 2.1 Product Perspective

OtoParcam is a standalone web-based spare part catalog and sales management system consisting of a React frontend, an ASP.NET Core Web API backend, and a PostgreSQL database.

The application follows Clean Architecture principles to ensure maintainability, scalability, and separation of concerns.

---

## 2.2 Product Functions

The system shall provide the following major functions:

- User authentication
- Product management
- Vehicle management
- Category management
- Product availability management
- Purchase request management
- Product search
- Dashboard and reporting

---

## 2.3 User Classes

### Guest

- Browse products
- Search products
- View product details

### Customer

- Register
- Login
- Submit purchase requests
- View purchase request history
- Manage favorites

### Administrator

- Manage products
- Manage categories
- Manage vehicle information
- Manage product availability
- Review purchase requests
- View dashboard statistics

---

## 2.4 Operating Environment

The system will operate on modern web browsers including:

- Google Chrome
- Microsoft Edge
- Mozilla Firefox

Backend

- ASP.NET Core Web API

Frontend

- React

Database

- PostgreSQL

Architecture

- Clean Architecture
- ASP.NET Core Identity
- RESTful API

---

## 2.5 Constraints

- Internet connection is required.
- JWT authentication shall be used.
- PostgreSQL shall be used as the primary database.
- The application shall support responsive web design.
- ASP.NET Core Identity shall be used for authentication and authorization.

---

## 2.6 Assumptions

- Customers have internet access.
- Administrators maintain accurate product information.
- Product compatibility information is entered correctly.

---

## 2.7 System Architecture

The system follows a layered architecture.

Presentation Layer

- React

Application Layer

- ASP.NET Core Web API

Persistence Layer

- PostgreSQL

Authentication

- ASP.NET Core Identity

Communication

- HTTPS REST API

---

# 3. Functional Requirements

## FR-01 User Registration

The system shall allow visitors to create customer accounts.

---

## FR-02 Authentication

The system shall authenticate users using either their email address or phone number together with their password.

---
5
## FR-03 Product Search

The system shall allow users to search products using:

- Vehicle Brand
- Vehicle Model
- Category
- Keyword

---

## FR-04 Product Details

The system shall display detailed product information including:

- Images
- Price
- Product status
- Vehicle compatibility
- Description

---

## FR-05 Purchase Request

Registered customers shall be able to submit purchase requests containing one or more products.

---

## FR-06 Purchase Request Negotiation

Administrators shall review purchase requests and may update negotiated prices after successful phone negotiations with customers.

Related Business Requirements

- BR-29
- BR-30
- BR-31

---

## FR-07 Product Management

Administrators shall create, update and remove products.

---

## FR-08 Category Management

Administrators shall manage product categories.

---

## FR-09 Vehicle Management

Administrators shall manage vehicle brands and models.

---

## FR-10 Dashboard

Administrators shall view statistics including:

- Total products
- Total customers
- Pending purchase requests
- Products grouped by status

---

## FR-11 Purchase Request Price Update

The system shall allow administrators to update the purchase request price without modifying the original product price.

Related Business Requirements

- BR-30
- BR-31

---

## FR-12 Purchase Request Confirmation

The system shall allow customers to review and confirm an updated purchase request before it becomes approved.

Related Business Requirements

- BR-32
- BR-33

---

## FR-13 Purchase Request Status Management

The system shall manage purchase requests using predefined status values throughout their lifecycle.

Related Business Requirements

- BR-29
- BR-30
- BR-31
- BR-35

---

## FR-14 Source Vehicle Information

The system shall inform users that the vehicle model displayed in the product title represents the source vehicle, while the compatibility list indicates all supported vehicle models.

---

## FR-15 Authorization

The system shall restrict system functions according to user roles.

- Guests may browse products only.
- Customers may manage only their own purchase requests and favorites.
- Administrators shall have full access to management functions.

---

## FR-16 Product Filtering

The system shall allow users to filter products by:

- Vehicle Brand
- Vehicle Model
- Category
- Availability
- Color (if applicable)

---

## FR-17 Favorite Management

Registered customers shall be able to add, remove and view products in their favorites.

---

## FR-18 Product Image Management

Administrators shall upload, update and delete product images.

---

## FR-19 Vehicle Compatibility Management

Administrators shall assign one or more compatible vehicle models to a product.

---

## FR-20 Source Vehicle Information Storage

The system shall store the source vehicle model from which the spare part was removed.

---

## FR-21 Product Price Display

The system shall display "Fiyat Ýçin Arayýn" when no product price is specified.

Related Business Requirements

- BR-04
- BR-38

---

## FR-22 User Login

The system shall allow customers to authenticate using either their email address or phone number together with their password.

---

## FR-23 Email Verification

The system shall require newly registered customers to verify their email address before activating their account.

---

## FR-24 Password Security

The system shall securely store user passwords using ASP.NET Core Identity password hashing mechanisms.

---

## FR-25 Product Availability

The system shall display only products whose status is Available.

Sold or Hidden products shall not be displayed to customers.

---

## FR-26 Audit Logging

The system shall record administrator actions affecting products and purchase requests for auditing purposes.

Related Business Requirements

- BR-27
- BR-28

---

## FR-27 Product Status Management

Administrators shall be able to change the status of products.

Supported statuses include:

- Available
- Sold
- Hidden

---

# 4. Non-Functional Requirements

## NFR-01 Performance

The application should respond within two seconds under normal usage.

---

## NFR-02 Security

Passwords shall never be stored in plain text.

Passwords shall be securely hashed using ASP.NET Core Identity.

JWT shall be used for authentication.

Role-based authorization shall be implemented.

---

## NFR-03 Usability

The user interface shall be simple, responsive and user-friendly.

---

## NFR-04 Reliability

The application shall maintain data consistency during all operations.

---

## NFR-05 Maintainability

The project shall follow Clean Architecture principles.

Dependency Injection shall be used throughout the application.

The system shall follow SOLID design principles.

Application layers shall remain loosely coupled.

---

## NFR-06 Scalability

The system shall support future modules such as:

- QR Code
- Barcode
- Mobile Application
- Docker deployment

---

## NFR-07 Data Integrity

Email addresses shall be unique.

Phone numbers shall be unique.

The system shall maintain referential integrity between related entities.

---

# 5. External Interface Requirements

## User Interface

The application shall provide responsive web pages optimized for desktop and mobile devices.

---

## Software Interface

Backend

- ASP.NET Core Web API

Frontend

- React

Database

- PostgreSQL

Authentication

- ASP.NET Core Identity

---

## Communication Interface

Communication between frontend and backend shall be performed using REST APIs over HTTPS.

---

# 6. Future Enhancements

Future versions may include:

- QR Code support
- Barcode support
- AI-powered compatibility recommendations
- Multi-branch inventory
- Supplier management
- Cargo tracking
- Sales analytics
- Advanced reporting

---

# 7. Approval

This document represents the initial Software Requirements Specification for the OtoParcam project and may be updated during development.

This document shall be used together with the Business Requirements Document (BRD) and the Database Design Document (DDR) throughout the software development lifecycle.