# Feature PRD: Loyalty Module Activation

## 1. Feature Name

Loyalty Module Activation — Envelope-Based Loyalty Program Participation Metadata

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Anonymous Access Hardening & Consent Lifecycle
- Architecture Spec: [arch.md](../arch.md) — see "Loyalty Envelope Module Handler" in the System Architecture Diagram (Service Layer)

## 3. Goal

### Problem

Merchants have no way to mark a receipt as participating in a loyalty program, and the platform's extensible envelope — designed to support exactly this kind of optional module — has not yet been proven with a loyalty-specific use case. Without this, merchants cannot begin building loyalty engagement on top of the digital receipt platform, and the envelope's second real-world module (after warranty) is unvalidated.

### Solution

Activate a versioned loyalty module within the existing receipt envelope: merchants can mark a receipt as associated with a named loyalty program and set a participation status (e.g., enrolled, pending, not-participating). The module stores program reference and participation metadata only — no point accrual, tiering, or campaign logic is calculated or stored by this feature. The module follows the same independently-addable, versioned pattern established by the warranty module in this epic.

### Impact

- Gives merchants a concrete way to flag loyalty participation on receipts today, ahead of full point/campaign logic in a later phase.
- Proves the envelope pattern generalizes across two independent modules (warranty and loyalty) without cross-module interference or core schema changes.
- Establishes the data foundation (program reference tied to a receipt) that future loyalty point-calculation and campaign features will build on.

## 4. User Personas

- **Merchant Admin**: Wants to flag which receipts are associated with their loyalty program and track participation status.
- **Customer (No Signup)**: Wants to see whether a purchase is counted toward a loyalty program they're part of.
- **Platform Operator** (internal): Wants confidence that the loyalty module, like the warranty module, cannot corrupt core receipt data or interfere with other envelope modules.

## 5. User Stories

- As a **Merchant Admin**, I want to mark a receipt as participating in a named loyalty program, so that I can later build point/reward logic on top of that record.
- As a **Merchant Admin**, I want to update a receipt's loyalty participation status (e.g., from "pending" to "enrolled"), so that the record reflects the current state of the customer's enrollment.
- As a **Customer**, I want to see whether my receipt is associated with a loyalty program, so that I know my purchase may count toward rewards.
- As a **Platform Operator**, I want the loyalty module's storage to be fully independent of the core receipt schema and of the warranty module, so that neither module can corrupt the other or the underlying receipt.

## 6. Requirements

### Functional Requirements

- The system must expose an endpoint to attach a loyalty module to an existing receipt, accepting: a loyalty program reference and a participation status value (from a defined, extensible set, e.g., `enrolled`, `pending`, `not-participating`).
- The system must expose an endpoint to read the loyalty module for a given receipt, returning `null`/absent (not an error) when no loyalty module has been attached.
- The system must expose an endpoint to update an existing loyalty module's participation status or program reference, incrementing the module version and recording an updated-at timestamp.
- The loyalty module must be stored as a distinct, namespaced section of the receipt's existing envelope column (e.g., `envelope.loyalty`), never merged into or overwriting core receipt fields or the warranty module's section.
- The loyalty module must include a `moduleVersion` field so future loyalty schema changes (e.g., adding point-accrual fields in a later phase) do not break receipts created under an earlier version.
- Attaching, updating, or the absence of a loyalty module must have no effect on the validity or retrievability of the core receipt or of other envelope modules (e.g., warranty) on the same receipt.
- The system must reject a loyalty attach/update request with an unrecognized participation status value, returning a deterministic validation error.
- The system must reject loyalty attach/update requests targeting a receipt id that does not exist, returning a deterministic not-found error.
- Point accrual, tiering, and reward-calculation values must not be computed, stored, or exposed by this feature — only program reference and participation status are in scope.

### Non-Functional Requirements

- **Data Integrity**: Loyalty module writes must not require, trigger, or risk any modification to core receipt fields or to the warranty module's data on the same receipt.
- **Extensibility**: The participation-status set must be defined in a way that allows adding new statuses later without a breaking change to existing stored records.
- **Backward Compatibility**: Receipts created before this feature was activated (with no loyalty module present) must continue to be retrievable and valid; the absence of a loyalty module must be a normal, expected state, not an error condition.
- **Consistency with Warranty Module Pattern**: The loyalty module's storage, versioning, and read/write endpoint shape must follow the same conventions established by the warranty module in this epic, to keep the envelope pattern uniform for future modules.
- **Authorization**: Loyalty module read/write access follows the same authorization boundary already governing receipt-by-id access; no new authorization model is introduced for this feature.

## 7. Acceptance Criteria

**Story: Attach loyalty participation to a receipt**

- [ ] Given an existing receipt with no loyalty module, When a valid program reference and participation status are submitted, Then the module is stored under the receipt's envelope with `moduleVersion` set to 1.
- [ ] Given an unrecognized participation status value, When submitted, Then the request is rejected with a deterministic validation error and no module is stored.

**Story: Read loyalty metadata**

- [ ] Given a receipt with an attached loyalty module, When the loyalty module is requested, Then the program reference, participation status, and module version are returned.
- [ ] Given a receipt with no loyalty module attached, When the loyalty module is requested, Then a null/absent result is returned, not an error.

**Story: Update loyalty participation status**

- [ ] Given a receipt with an existing loyalty module in "pending" status, When the status is updated to "enrolled", Then the module's status field is updated, `moduleVersion` is incremented, and an updated-at timestamp is recorded.

**Story: Core receipt and cross-module integrity preserved**

- [ ] Given a receipt with an attached loyalty module, When the core receipt (totals, line items, currency, state) is retrieved, Then its values are identical to what they were before the loyalty module was attached.
- [ ] Given a receipt with both a warranty and a loyalty module attached, When the loyalty module is updated, Then the warranty module's data remains unchanged.

**Story: Not-found handling**

- [ ] Given a receipt id that does not exist, When a loyalty attach/update request targets it, Then the response is a deterministic not-found error and no module is created.

## 8. Out of Scope

- Point accrual calculation, tier progression, or reward redemption logic (only program reference and participation status metadata are in scope).
- Loyalty campaign execution or promotional messaging.
- Integration with external loyalty-platform providers.
- Customer-facing self-service enrollment UI beyond a functional form to demonstrate the capability (merchant-initiated attachment is the primary path).
- Cross-receipt loyalty aggregation (e.g., computing a customer's total lifetime points across receipts) — this feature only stores per-receipt participation metadata.
