# OtoParcam

## Software Requirements Specification (SRS)

Version 1.0

Author Nejdet Vural

Date 13.07.2026

----

# 1. Introduction

## 1.1 Purpose

The purpose of this document is to define the functional and non-functional requirements of the OtoParcam system. This document serves as a
reference for the design, implementation, testing, and maintenance phases of the project.

----

## 1.2 Scope

OtoParcam is a web-based inventory and sales management system developed for businesses selling used auto spare parts.

The system allows customers to search for compatible spare parts based on vehicle information while providing administrators with tools to
manage products, inventory, categories, vehicle data, customer accounts, and purchase requests.

---

## 1.3 Definitions

| Term | Description |

| Admin | System administrator with full permissions |
| Customer | Registered user who can submit purchase requests |
| Guest | Unauthenticated visitor |
| Product | Used auto spare part |
| Purchase Request | Customer request submitted for administrator approval |

---

## 1.4 References

- Project Vision Document
- Business Requirements Document

---
# 2. Overall Description

## 2.1 Product Perspective

OtoParcam is a standalone web application consisting of a frontend client, a RESTful backend API, and a PostgreSQL database.

The system is designed using a layered architecture to ensure scalability and maintainability.

---

## 2.2 Product Functions

The system shall provide the following major functions:

- User authentication
- Product management
- Vehicle management
- Category management
- Inventory management
- Purchase request management
- Product search
- Dashboard and reporting

## 2.3 User Classes

### Guest

- Browse products
- Search products
- View product details

### Customer

- Register
- Login
- Submit purchase requests
- View request history
- Manage favorites

### Administrator

- Manage products
- Manage categories
- Manage vehicle information
- Manage inventory
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

---

## 2.5 Constraints

- Internet connection is required.
- JWT authentication will be used.
- PostgreSQL will be used as the primary database.
- The application will support responsive web design.

---

# 2.6 Assumptions

- Customers have internet access.
- Administrators maintain accurate inventory data.
- Product compatibility information is entered correctly.

 # 3. Functional Requirements

## FR-01 User Registration

The system shall allow visitors to create customer accounts.

---

## FR-02 Authentication

The system shall authenticate users using email and password.

---

## FR-03 Product Search

The system shall allow users to search products using:

- Vehicle Brand
- Vehicle Model
- Category
- Keyword

## FR-04 Product Details

The system shall display detailed product information including:

- Images
- Price
- Stock status
- Vehicle compatibility
- Description

---

## FR-05 Purchase Request

Registered customers shall be able to submit purchase requests.

---

## FR-06 Purchase Approval

Administrators shall review and approve or reject purchase requests.

---

## FR-07 Inventory Management

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
- Pending requests
- Low stock products

---

## FR-11 Purchase Request Price Update

The system shall allow administrators to update the purchase request price without modifying the original product price.

Related Business Requirements

- BR-30
- BR-31

---

## FR-12 Purchase Request Confirmation

The system shall allow customers to review and confirm an updated purchase request before administrator approval.

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

---

## FR-14
The system shall inform users that the vehicle model displayed in the product title represents the source
vehicle, while the compatibility list indicates all supported vehicle models.

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

Registered customers shall be able to add and remove products from their favorites.

---

## FR-18 Product Image Management

Administrators shall upload, update and delete product images.

---

## FR-19 Vehicle Compatibility Management

Administrators shall assign one or more compatible vehicle models to a product.

---

## FR-20 Source Vehicle Information

The system shall store the source vehicle model from which the spare part was removed.

---

## FR-21 Product Price Display

The system shall display "Fiyat Ýçin Arayýn" when no product price is specified.

---

## FR-22 User Login

The system shall allow customers to authenticate using either their email address or phone number together with their password.

---

## FR-23 Email Verification

The system shall require newly registered customers to verify their email address before activating their account.

---

## FR-24 Password Encryption

The system shall securely store user passwords using a strong password hashing algorithm.

---

# 4. Non-Functional Requirements

## NFR-01 Performance

The application should respond within two seconds under normal usage.

---

## NFR-02 Security

Passwords shall be encrypted.

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

---

## NFR-06 Scalability

The system shall support future modules such as:

- QR Code
- Barcode
- Mobile Application

---

## NFR-07 Security

Passwords shall never be stored in plain text.

User authentication shall follow industry-standard security practices.

Email addresses and phone numbers shall be unique.

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

---

# 7. Approval

This document represents the initial software requirements specification for the OtoParcam project and may be updated during development.

