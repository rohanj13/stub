# Feature PRD: Warranty Module Activation

## 1. Feature Name

Warranty Module Activation — Envelope-Based Warranty Registration Metadata

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Anonymous Access Hardening & Consent Lifecycle
- Architecture Spec: [arch.md](../arch.md) — see "Warranty Envelope Module Handler" in the System Architecture Diagram (Service Layer)

## 3. Goal

### Problem

The Phase 1 receipt envelope was explicitly designed to carry optional modules (loyalty, ads, warranty, integrations) without requiring a core schema rework, but no module has yet been activated. Merchants selling products with warranty coverage have no way to attach warranty registration metadata to a digital receipt, and customers have no way to see warranty terms associated with a purchase — meaning the envelope's extensibility remains theoretical rather than proven.

### Solution

Activate a versioned warranty module within the existing receipt envelope: merchants (or, where enabled, customers) can attach warranty metadata — product reference, coverage start/end dates, and a terms reference — to an existing receipt. The module is stored and versioned independently of the core receipt fields and other envelope modules, and is readable alongside the receipt without requiring a new authorization model.

### Impact

- Converts the Phase 1 envelope investment into a concrete, sellable merchant capability (warranty registration at point of digital receipt).
- Proves the envelope's "add a module without breaking core receipts" design promise with a real, non-trivial module.
- Establishes the pattern (versioned, independently addable module) that the loyalty module and future modules (ads, integrations) will follow.

## 4. User Personas

- **Merchant Admin**: Wants to attach warranty coverage information to a receipt so customers have a durable, accessible record of their warranty terms.
- **Customer (No Signup)**: Wants to see warranty coverage details tied to a specific purchase without needing to keep a paper warranty card.
- **Platform Operator** (internal): Wants confidence that adding this module didn't alter or risk core receipt data integrity.

## 5. User Stories

- As a **Merchant Admin**, I want to attach warranty terms (product reference, coverage window, terms link) to a receipt, so that the customer has a durable, digital record of their coverage.
- As a **Customer**, I want to view warranty details alongside my receipt, so that I know what's covered and until when without keeping a paper card.
- As a **Merchant Admin**, I want to update or replace warranty metadata on a receipt (e.g., correcting a coverage date), so that the record stays accurate, with old versions distinguishable from new ones.
- As a **Platform Operator**, I want the warranty module's storage to be fully independent of the core receipt schema, so that attaching or removing it can never corrupt or invalidate the underlying receipt.

## 6. Requirements

### Functional Requirements

- The system must expose an endpoint to attach a warranty module to an existing receipt, accepting: product reference, coverage start date, coverage end date, and a terms reference (URL or document identifier).
- The system must expose an endpoint to read the warranty module for a given receipt, returning `null`/absent (not an error) when no warranty module has been attached.
- The system must expose an endpoint to update an existing warranty module's fields, incrementing the module version and recording an updated-at timestamp.
- The warranty module must be stored as a distinct, namespaced section of the receipt's existing envelope column (e.g., `envelope.warranty`), never merged into or overwriting core receipt fields.
- The warranty module must include a `moduleVersion` field so future warranty schema changes do not break receipts created under an earlier version.
- Attaching, updating, or the absence of a warranty module must have no effect on the validity or retrievability of the core receipt or of other envelope modules (e.g., loyalty) on the same receipt.
- The system must validate that `coverageEndDate` is not earlier than `coverageStartDate`, rejecting invalid ranges with a deterministic error.
- The system must reject warranty attach/update requests targeting a receipt id that does not exist, returning a deterministic not-found error.

### Non-Functional Requirements

- **Data Integrity**: Warranty module writes must not require, trigger, or risk any modification to core receipt fields (totals, line items, currency, state).
- **Extensibility**: The module's storage pattern must be reusable as a template for future envelope modules (e.g., loyalty in this same epic, ads/integrations later) without further core schema changes.
- **Backward Compatibility**: Receipts created before this feature was activated (with no warranty module present) must continue to be retrievable and valid; the absence of a warranty module must be a normal, expected state, not an error condition.
- **Auditability**: Warranty module version history should be reconstructable at least to the extent of current version + updated-at timestamp (full historical version diffing is not required).
- **Authorization**: Warranty module read/write access follows the same authorization boundary already governing receipt-by-id access; no new authorization model is introduced for this feature.

## 7. Acceptance Criteria

**Story: Attach warranty to a receipt**

- [ ] Given an existing receipt with no warranty module, When warranty metadata (product reference, coverage window, terms reference) is submitted, Then the module is stored under the receipt's envelope with `moduleVersion` set to 1.
- [ ] Given a coverage end date earlier than the start date, When warranty metadata is submitted, Then the request is rejected with a deterministic validation error and no module is stored.

**Story: Read warranty metadata**

- [ ] Given a receipt with an attached warranty module, When the warranty module is requested, Then the product reference, coverage window, terms reference, and module version are returned.
- [ ] Given a receipt with no warranty module attached, When the warranty module is requested, Then a null/absent result is returned, not an error.

**Story: Update warranty metadata**

- [ ] Given a receipt with an existing warranty module, When updated warranty metadata is submitted, Then the module's fields are replaced, `moduleVersion` is incremented, and an updated-at timestamp is recorded.

**Story: Core receipt integrity preserved**

- [ ] Given a receipt with an attached warranty module, When the core receipt (totals, line items, currency, state) is retrieved, Then its values are identical to what they were before the warranty module was attached.
- [ ] Given a receipt with both a warranty and a loyalty module attached, When the warranty module is updated, Then the loyalty module's data remains unchanged.

**Story: Not-found handling**

- [ ] Given a receipt id that does not exist, When a warranty attach/update request targets it, Then the response is a deterministic not-found error and no module is created.

## 8. Out of Scope

- Warranty claim submission, adjudication, or fulfillment workflow — only registration/terms metadata storage is in scope.
- Automatic warranty expiration notifications or reminders to customers.
- Multi-party or transferable warranty ownership (e.g., transferring warranty coverage to a second owner).
- Integration with external warranty-provider systems or manufacturer registration APIs.
- Customer-facing self-service warranty attachment UI beyond a functional form to demonstrate the capability (merchant-initiated attachment is the primary path).
- Legal/regulatory validation of warranty terms content — the terms reference is stored as-is; content correctness is a merchant responsibility.
