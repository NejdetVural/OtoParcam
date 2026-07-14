# OtoParcam

## Use Case Specification

Version 1.0

Author Nejdet Vural

Date 13.07.2026

# 1. Introduction

## 1.1 Purpose

This document describes how different users interact with the OtoParcam system. Each use case represents a business scenario supported by
the application.

---

## 1.2 Actors

| Actor | Description |

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

- BR-12
- FR-03
- FR-04

## Preconditions

- The system is online.
- Products exist in the inventory.

## Main Flow

1. Guest opens the website.
2. Guest navigates to the Products page.
3. The system displays available products.
4. Guest searches or filters products.
5. Guest selects a product.
6. The system displays detailed product information.

## Alternative Flow

4a. No matching products are found.

The system displays an informative message.

## Postconditions

The selected product details are displayed.

---

# UC-02 Submit Purchase Request

## Goal

Allow a registered customer to submit a purchase request.

## Primary Actor

Customer

## Related Requirements

- BR-13
- BR-17
- BR-18
- BR-19
- BR-20

- FR-05
- FR-06

## Preconditions

- Customer is authenticated.
- Selected products are in stock.

## Main Flow

1. Customer selects one or more products.
2. Customer submits a purchase request.
3. Administrator reviews the request.
4. Customer and administrator may negotiate the price by phone.
5. Administrator updates the purchase request if necessary.
6. The purchase request status becomes **Waiting for Customer Confirmation**.
7. Customer reviews the updated request.
8. Customer confirms the request.
9. Administrator approves the request.
10. Product stock is updated.
	
---

## Alternative Flow

5a. Customer accepts the original product price.

The administrator approves the purchase request without updating the price.

## Postconditions

The purchase request status becomes **Pending**.

---

# UC-03 Manage Products

## Goal

Allow administrators to manage product information.

## Primary Actor

Administrator

## Related Requirements

- BR-01
- BR-02
- BR-03
- BR-23

- FR-07

## Preconditions

- Administrator is authenticated.

## Main Flow

1. Administrator opens Product Management.
2. Administrator creates a new product.
3. Product information is entered.
4. Images are uploaded.
5. Vehicle compatibility is selected.
6. Product is saved.
7. The system validates all information.
8. Product becomes available.

## Alternative Flow

7a. Required information is missing.

The system displays validation errors.

## Postconditions

The product is stored successfully.

---

# UC-04 Approve Purchase Request

## Goal

Allow administrators to approve or reject purchase requests.

## Primary Actor

Administrator

## Related Requirements

- BR-19
- BR-20
- BR-21
- BR-26

- FR-06

## Preconditions

- Administrator is authenticated.
- At least one pending request exists.

## Main Flow

1. Administrator opens Purchase Requests.
2. Administrator selects a pending request.
3. Administrator reviews request details.
4. Administrator approves or rejects the request.
5. The system updates the request status.
6. If approved, inventory is updated.
7. Customer is notified.

## Alternative Flow

4a. Administrator rejects the request.

The request status becomes **Rejected**.

## Postconditions

The request reaches its final status.