# OtoParcam

## Business Requirements Document

Version 1.0

Author Nejdet Vural

Date 13.07.2026

---
# 1. Introduction


### 1.1 Purpose

This document defines the business requirements for OtoParcam project.It describes business needs, objectives, workflows and rules that
the system must support.

---

## 1.2 Business Background 

Many used auto spare part businesses still rely on manual inventory tracking, phone calls, and messaging applications to manage sales 
and communicate with customers.As the number of products increases, managing inventory becomes more difficult, customer response times 
increase, and the risk of selling incompatible parts becomes higher.OtoParcam aims to digitalize these processes through a centralized 
web-based management system.

---

# 2 Business Objectives

The main business objectives are:

- Digitalize inventory management
- Improve product accessibility
- Simplify order management
- Reduce manual workload
- Improve customer satisfaction
- Increase operational efficiency
- Reduce inventory errors

---

# 3 Business Scope

The system will support the following business operations.

- Product Management
- Category Management
- Vehicle Management
- Inventory Management
- Customer Management
- Order Management
- Product Search
- Reporting

---

# 4. Business Processes

The system supports two primary workflows.

## Product Management Process

1. Administrator logs into the system.
2. Administrator creates or updates product information.
3. Product images are uploaded.
4. Product inventory is updated.
5. Product becomes available for customers.

## Customer Purchase Process

1. Customer visits the website.
2. Customer searches for spare parts.
3. Customer reviews product details.
4. Customer may contact the store by phone for price negotiation.
5. Customer submits a purchase request.
6. Administrator reviews the purchase request.
7. If a negotiated price is agreed by phone, the administrator updates the purchase request.
8. The purchase request status becomes **Waiting for Customer Confirmation**.
9. Customer reviews and confirms the updated purchase request.
10. Administrator approves or rejects the purchase request.
11. If approved, product stock is updated.
12. Customer is informed about the final status.

---

# 5. Business Rules

The following business rules define the business logic and constraints of the OtoParcam system.

5.1 ## Product Rules

**BR-01** Every product must belong to at least one category.

**BR-02** Every product must have at least one image.

**BR-03** Every product must have a unique product code.

**BR-04** A product price is optional if its null than UI displays "Fiyat için arayýn".

**BR-05** Stock quantity cannot be negative.

**BR-06** A product may be compatible with multiple vehicle models.

**BR-07** Compatibility information must be specified before a product becomes available.

**BR-08** Products with zero stock are automatically marked as **Out of Stock**.

**BR-38** If no price is specified, the system shall display "Contact for Price".

**BR-39** Purchase requests may be created regardless of whether the product price is displayed.

**BR-40** Category names shall be unique.

**BR-41** A category may contain multiple products.

**BR-42** A category cannot be deleted while it is referenced by one or more products.

**BR-43** Administrators may create new categories.

**BR-60** Every product shall contain between one and ten images.

**BR-61** The first uploaded image shall be used as the product cover image.

**BR-62** Images shall be displayed according to their DisplayOrder.

**BR-63** When an image is deleted, the remaining images shall be reordered automatically.

**BR-64** Each product image belongs to exactly one product.

---

5.2 ## Vehicle Rules

**BR-09** Every vehicle model belongs to exactly one vehicle brand.

**BR-10** Compatibility records cannot reference non-existing vehicle models.

**BR-11** One product may be associated with multiple vehicle models.

**BR-44** Vehicle brand names shall be unique.

**BR-45** A vehicle brand may contain multiple vehicle models.

**BR-46** A vehicle brand cannot be deleted while it is referenced by one or more vehicle models.

**BR-47** Administrators may create new vehicle brands.

**BR-48** Vehicle model names shall be stored separately from their production years.

**BR-49** StartYear shall be less than or equal to EndYear.

**BR-50** Variant is optional.

**BR-51** Variant is used for display purposes only and shall not be used for searching or filtering.

**BR-52** A vehicle model cannot be deleted while it is referenced by Products or ProductCompatibility records.


---

5.3 ## Customer Rules

**BR-12** Visitors may browse products without authentication.

**BR-13** Registration is required before submitting a purchase request.

**BR-14** Customers may submit purchase requests only for products currently in stock.

**BR-15** Customers can view only their own purchase requests.

**BR-16** Customers may add products to their favorites after logging in.

**BR-65** Every customer shall provide a unique email address.

**BR-66** Every customer shall provide a unique phone number.

**BR-67** Customers may authenticate using either their email address or phone number.

**BR-68** Customer accounts shall require email verification before becoming active.

---

5.4 ## Order Rules

**BR-17** Every purchase request belongs to one customer.

**BR-18** A purchase request must contain at least one product.

**BR-19** Purchase requests are reviewed by an administrator.

**BR-20** Stock quantity is decreased only after the administrator approves the purchase request.

**BR-21** Cancelled or rejected purchase requests do not affect inventory.

**BR-22** Every purchase request shall have exactly one status at any given time.

---

5.5 ## Administration Rules

**BR-23** Only administrators can create, update, or delete products.

**BR-24** Only administrators can manage vehicle brands and models.

**BR-25** Only administrators can update inventory quantities.

**BR-26** Only administrators can approve or reject purchase requests.

**BR-27** Every inventory update shall be recorded in the system.

**BR-28** Every administrator action affecting products, inventory, or purchase requests shall be logged for auditing purposes.

5.6 ### Negotiation Rules

**BR-29** Customers may negotiate the product price outside the system (e.g., by phone).

**BR-30** Administrators may update a purchase request after a successful negotiation.

**BR-31** Updating a purchase request shall not modify the original product price.

**BR-32** An updated purchase request shall require customer confirmation before administrator approval.

**BR-33** Each purchase request shall store both the original product price and the negotiated price.

**BR-34** Each purchase request shall maintain its own independent pricing information.

**BR-35** A purchase request shall have one of the following statuses:

5.7 ### Product Compatibility Rules

**BR-53** A product may be compatible with multiple vehicle models.

**BR-54** A vehicle model may be compatible with multiple products.

**BR-55** The combination of ProductId and VehicleModelId shall be unique.

**BR-56** Compatibility records shall be created manually by administrators.

**BR-57** Compatibility information shall be based only on the vehicle model in version 1.0.

**BR-58** Engine type, fuel type, transmission, package and other technical differences shall not be stored as compatibility criteria.

**BR-59** Additional compatibility details may be provided in the product description or communicated outside the system.

- Pending
- WaitingForCustomer
- Confirmed
- Approved
- Rejected
- Cancelled

**BR-36** Product stock shall be decreased only after the purchase request reaches the **Approved** status.

---

# 6. Success Criteria

The project will be considered successful if:

- Customers can quickly locate compatible spare parts.
- Administrators can efficiently manage products and inventory.
- Order processing becomes faster and easier.
- Manual inventory tracking is eliminated.
- The application provides a modern and user-friendly experience.

---

# 7. Future Business Opportunities

Future versions may include:

- QR Code integration
- Barcode support
- AI-based spare part recommendations
- Mobile application
- Multi-branch inventory management
- Supplier management
- Sales analytics