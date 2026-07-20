# OtoParcam

## Use Case Specification

Version 1.0

Author Nejdet Vural

Date 13.07.2026

---

# 1. Introduction

## 1.1 Purpose

This document describes how different users interact with the OtoParcam system. Each use case represents a business scenario supported by the application.

---

## 1.2 Actors

| Actor | Description |
|--------|-------------|
| Guest | Unauthenticated visitor |
| Customer | Registered user |
| Administrator | System administrator |

---

# UC-01 Browse Products

## Goal

Allow visitors to browse available spare parts without authentication.

## Primary Actor

Guest

## Related Requirements

### Business Requirements

- BR-12

### Functional Requirements

- FR-03
- FR-04
- FR-16
- FR-21

## Preconditions

- The system is online.
- At least one product with the status **Available** exists.

## Main Flow

1. Guest opens the website.
2. Guest navigates to the Products page.
3. The system displays available products.
4. Guest searches or filters products.
5. Guest selects a product.
6. The system displays detailed product information.

## Alternative Flow

### 4a. No matching products are found.

The system displays an informative message.

## Postconditions

The selected product details are displayed.

---

# UC-02 Register Account

## Goal

Allow a visitor to create a customer account.

## Primary Actor

Guest

## Related Requirements

### Functional Requirements

- FR-01
- FR-23
- FR-24

## Preconditions

- The visitor is not authenticated.

## Main Flow

1. Visitor opens the registration page.
2. Visitor enters personal information.
3. Visitor enters email, phone number and password.
4. The system validates the entered information.
5. A new customer account is created.
6. A verification email is sent.
7. The customer verifies the email address.
8. The account becomes active.

## Alternative Flow

### 4a. Email or phone number already exists.

The system displays a validation error.

## Postconditions

A verified customer account exists.

---

# UC-03 Login

## Goal

Allow registered customers to authenticate.

## Primary Actor

Customer

## Related Requirements

### Functional Requirements

- FR-02
- FR-22

## Preconditions

- The customer account exists.
- The email address has been verified.

## Main Flow

1. Customer opens the login page.
2. Customer enters an email address or phone number.
3. Customer enters a password.
4. The system validates the credentials.
5. The customer is authenticated.
6. Access is granted.

## Alternative Flow

### 4a. Invalid credentials.

The system displays an authentication error.

## Postconditions

The customer is authenticated.

---

# UC-04 Submit Purchase Request

## Goal

Allow a registered customer to submit a purchase request.

## Primary Actor

Customer

## Related Requirements

### Business Requirements

- BR-13
- BR-17
- BR-18
- BR-19
- BR-20

### Functional Requirements

- FR-05
- FR-11
- FR-12
- FR-13

## Preconditions

- Customer is authenticated.
- Selected products have the status **Available**.

## Main Flow

1. Customer selects one or more products.
2. Customer submits a purchase request.
3. The purchase request status becomes **Pending**.
4. Administrator reviews the request.
5. Customer and administrator negotiate by phone if necessary.
6. Administrator records negotiated prices.
7. The purchase request status becomes **WaitingForCustomerConfirmation**.
8. Customer reviews the updated request.
9. Customer accepts or rejects the purchase request.
10. If accepted, the request status becomes **Approved**.
11. Administrator changes the related product statuses to **Sold**.

## Alternative Flow

### 5a. No negotiation is required.

The administrator keeps the original prices.

The request proceeds directly to customer confirmation.

### 9a. Customer rejects the negotiated prices.

The purchase request status becomes **Rejected**.

## Postconditions

The purchase request reaches one of its final statuses.

- Approved
- Rejected
- Cancelled

---

# UC-05 Manage Products

## Goal

Allow administrators to manage products.

## Primary Actor

Administrator

## Related Requirements

### Business Requirements

- BR-01
- BR-02
- BR-03
- BR-23

### Functional Requirements

- FR-07
- FR-18
- FR-19
- FR-27

## Preconditions

- Administrator is authenticated.

## Main Flow

1. Administrator opens Product Management.
2. Administrator creates or edits a product.
3. Product information is entered.
4. Product images are uploaded.
5. Compatible vehicle models are assigned.
6. Product information is saved.
7. The system validates all entered data.
8. Administrator assigns the initial product status.

## Alternative Flow

### 7a. Validation fails.

The system displays validation errors.

## Postconditions

The product information is successfully stored.

---

# UC-06 Review Purchase Request

## Goal

Allow administrators to review purchase requests and record negotiated prices.

## Primary Actor

Administrator

## Related Requirements

### Business Requirements

- BR-19
- BR-20
- BR-21
- BR-26

### Functional Requirements

- FR-06
- FR-11
- FR-13

## Preconditions

- Administrator is authenticated.
- At least one pending purchase request exists.

## Main Flow

1. Administrator opens Purchase Requests.
2. Administrator selects a pending request.
3. Administrator reviews the request.
4. Administrator contacts the customer by phone.
5. Negotiated prices are recorded if necessary.
6. The request status becomes **WaitingForCustomerConfirmation**.

## Alternative Flow

### 5a. Customer accepts the original prices.

The administrator keeps the original prices.

The request proceeds directly to customer confirmation.

## Postconditions

The purchase request awaits customer confirmation.

---

# UC-07 Manage Favorites

## Goal

Allow customers to manage favorite products.

## Primary Actor

Customer

## Related Requirements

### Functional Requirements

- FR-17

## Preconditions

- Customer is authenticated.

## Main Flow

1. Customer opens a product.
2. Customer adds the product to favorites.
3. Customer views the favorite list.
4. Customer removes a product if desired.

## Alternative Flow

### 2a. Product is already in favorites.

The system informs the customer.

## Postconditions

The favorite list is updated.

---

# UC-08 Manage Categories

## Goal

Allow administrators to manage product categories.

## Primary Actor

Administrator

## Related Requirements

### Functional Requirements

- FR-08

## Preconditions

- Administrator is authenticated.

## Main Flow

1. Administrator opens Category Management.
2. Administrator creates, updates or deletes categories.
3. The system validates the changes.
4. Changes are saved.

## Postconditions

Category information is updated.

---

# UC-09 Manage Vehicle Data

## Goal

Allow administrators to manage vehicle brands and models.

## Primary Actor

Administrator

## Related Requirements

### Functional Requirements

- FR-09

## Preconditions

- Administrator is authenticated.

## Main Flow

1. Administrator opens Vehicle Management.
2. Administrator manages vehicle brands.
3. Administrator manages vehicle models.
4. Changes are validated.
5. Changes are saved.

## Postconditions

Vehicle information is updated.

---

# 2. Business Rules Traceability

| Use Case | Related BR | Related FR |
|----------|------------|------------|
| UC-01 | BR-12 | FR-03, FR-04, FR-16, FR-21 |
| UC-02 | — | FR-01, FR-23, FR-24 |
| UC-03 | — | FR-02, FR-22 |
| UC-04 | BR-13, BR-17, BR-18, BR-19, BR-20 | FR-05, FR-11, FR-12, FR-13 |
| UC-05 | BR-01, BR-02, BR-03, BR-23 | FR-07, FR-18, FR-19, FR-27 |
| UC-06 | BR-19, BR-20, BR-21, BR-26 | FR-06, FR-11, FR-13 |
| UC-07 | — | FR-17 |
| UC-08 | — | FR-08 |
| UC-09 | — | FR-09 |

---

# Approval

This document describes the interaction scenarios supported by the OtoParcam system and shall be maintained together with the Business Requirements Document (BRD), Software Requirements Specification (SRS), and Database Design Document (DDR).