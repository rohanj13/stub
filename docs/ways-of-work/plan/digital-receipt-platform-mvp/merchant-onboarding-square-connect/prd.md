# Feature PRD: Merchant Onboarding & Square Connect

## 1. Feature Name

Merchant Onboarding & Square Connect

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "Merchant Management Service" in the System Architecture Diagram

## 3. Goal

### Problem

A merchant cannot receive any transaction-derived receipts until their store exists in Stub and is linked to their Square account. Today the platform supports creating a merchant record and a "connect to Square" action that flips a `WebhookRegistered` flag, but there is no guided onboarding flow, no visibility into _why_ a merchant isn't webhook-ready, and no validation that the POS account id is well-formed before it's relied upon by ingestion.

### Solution

Provide a clear onboarding flow — create merchant profile, connect to Square, confirm webhook-ready state — with validation and explicit status surfaced to the Merchant Admin, so merchants can self-diagnose onboarding issues before relying on the platform for pilot transactions.

### Impact

- Reduces onboarding support burden by making integration state self-evident.
- Prevents downstream ingestion failures caused by misconfigured or incomplete merchant records.
- Establishes the merchant record as the authoritative source for webhook validation used by the ingestion feature.

## 4. User Personas

- **Merchant Admin**: Creates the merchant profile and initiates the Square connection.
- **Platform Operator**: Needs to confirm which merchants are webhook-ready ahead of a pilot.

## 5. User Stories

- As a **Merchant Admin**, I want to create a merchant profile with my store name and POS account id, so that Stub can start associating transactions with my store.
- As a **Merchant Admin**, I want to connect my merchant profile to Square, so that transaction webhooks become active for my store.
- As a **Merchant Admin**, I want to see whether my store is webhook-ready, so that I know when it's safe to start relying on digital receipts at checkout.
- As a **Platform Operator**, I want to list all merchants and their integration state, so that I can validate pilot readiness across stores.

## 6. Requirements

### Functional Requirements

- The system must accept merchant creation requests with required fields `Name` and `PosAccountId`; requests missing either must return `400 Bad Request`.
- The system must reject merchant creation with a duplicate `PosAccountId` for the same POS provider (Square) to prevent accidental double-registration.
- The system must default new merchants to `PosProvider = "square"` and `WebhookRegistered = false`.
- The system must expose a "connect to Square" action per merchant that sets `WebhookRegistered = true` and returns `404 Not Found` for an unknown merchant id.
- The system must expose a list endpoint returning all merchants with their id, name, POS account id, provider, and webhook-registered state.
- The frontend must provide a merchant creation form and a way to trigger/display the Square connect action and resulting state.

### Non-Functional Requirements

- **Validation**: Field validation errors must be deterministic and returned before any database write.
- **Idempotency**: Re-invoking "connect to Square" on an already-connected merchant must be a safe no-op returning the same success state.
- **Auditability**: Merchant creation and connect actions should be logged with merchant id and timestamp for onboarding troubleshooting.

## 7. Acceptance Criteria

**Story: Create merchant profile**

- [ ] Given valid `Name` and `PosAccountId`, When a merchant is created, Then a merchant record is persisted with `WebhookRegistered = false` and returned with `201 Created`.
- [ ] Given a missing `Name` or `PosAccountId`, When creation is attempted, Then the response is `400 Bad Request` and no record is created.
- [ ] Given a `PosAccountId` already registered for Square, When creation is attempted again, Then the request is rejected without creating a duplicate.

**Story: Connect to Square**

- [ ] Given an existing merchant, When "connect to Square" is invoked, Then `WebhookRegistered` becomes `true` and the response confirms `webhookRegistered: true`.
- [ ] Given an unknown merchant id, When "connect to Square" is invoked, Then the response is `404 Not Found`.
- [ ] Given an already-connected merchant, When "connect to Square" is invoked again, Then the response still confirms `webhookRegistered: true` without error.

**Story: List merchants and integration state**

- [ ] Given one or more merchants exist, When the list endpoint is called, Then all merchants are returned ordered by name with their current webhook-registered state.

## 8. Out of Scope

- Actual Square OAuth handshake / credential exchange (MVP simulates the connect action; real OAuth is a future hardening item).
- Merchant receipt branding/customization (covered by a separate feature).
- Multi-POS-provider selection UI (Square-only for this feature).
- Merchant user authentication/authorization (who is allowed to manage which merchant) — assumed out of scope for MVP operator tooling.
