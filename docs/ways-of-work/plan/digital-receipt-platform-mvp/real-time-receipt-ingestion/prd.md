# Feature PRD: Real-Time Square Transaction Ingestion & Claimable Receipt

## 1. Feature Name

Real-Time Square Transaction Ingestion & Claimable Receipt Processing

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "Square Webhook Ingestion Service" in the System Architecture Diagram (Service Layer)

## 3. Goal

### Problem

When a customer completes a purchase at a participating merchant, the receipt must be available for tap/scan claim before the customer leaves the register. Today, the platform has a basic webhook endpoint (`POST /api/webhooks/square/transactions`) that persists a receipt synchronously, but it has no idempotency protection against duplicate webhook deliveries, no explicit claimability state machine, and no latency telemetry — so there is no way to know whether the ingestion pipeline is fast enough to support the checkout tap/scan moment, and a retried webhook could create duplicate receipts for the same transaction.

### Solution

Harden the existing ingestion endpoint into a reliable, idempotent pipeline that validates the payload and merchant registration, persists exactly one receipt per Square transaction id (even under webhook retries), and explicitly transitions the receipt through a `Pending → Claimable` state with a recorded transition timestamp. Emit structured latency telemetry (webhook-received-at → claimable-at) so ingest-to-claimable performance can be measured by percentile and monitored operationally.

### Impact

- Enables reliable measurement of transaction-to-claimable latency (p50/p95/p99), a core platform success metric.
- Eliminates duplicate-receipt risk from webhook retry delivery, improving data integrity for downstream claim/history features.
- Provides the state-machine foundation (`Pending`/`Claimable`/`Claimed`) that the checkout claim feature and anonymous history feature both depend on.

## 4. User Personas

- **Merchant Admin**: Relies on transactions reliably becoming receipts without duplicates or drops.
- **Store Associate**: Needs the receipt to be claimable essentially immediately after the payment completes.
- **Customer (No Signup)**: Experiences this feature indirectly — a successful tap/scan depends entirely on the receipt already being claimable by the time they act.
- **Platform Operator** (internal): Monitors ingest-to-claimable latency and duplicate/error rates to validate pilot readiness.

## 5. User Stories

- As a **Store Associate**, I want the receipt to become claimable within moments of the transaction completing, so that the customer can tap/scan before leaving the register.
- As a **Platform Operator**, I want duplicate Square webhook deliveries for the same transaction to be ignored (not create a second receipt), so that receipt data stays accurate and customers don't see duplicate purchase history entries.
- As a **Platform Operator**, I want every ingested webhook to reject payloads from unregistered merchants or with missing required fields, so that malformed or unauthorized events never enter the receipt data set.
- As a **Platform Operator**, I want ingest-to-claimable latency recorded per transaction, so that I can report p50/p95/p99 against the checkout-window SLO.
- As a **Merchant Admin**, I want to see a receipt fail predictably (not silently) if my merchant account isn't fully connected, so that I can fix onboarding issues before pilot traffic starts.

## 6. Requirements

### Functional Requirements

- The webhook endpoint must validate that `TransactionId`, `MerchantId`, and `Currency` are present and non-empty; requests failing validation must return `400 Bad Request` with a deterministic error body.
- The webhook endpoint must return `404 Not Found` (or an equivalent clear error) when `MerchantId` does not correspond to a registered merchant.
- The system must enforce idempotency on `(MerchantId, TransactionId)`: a repeat webhook delivery for the same transaction must not create a second receipt. A retried request must return the original receipt's identity and location rather than erroring.
- The receipt entity must carry an explicit state field with at least the values `Pending` and `Claimable`, plus timestamps for `CreatedAtUtc` and `ClaimableAtUtc`.
- On successful ingestion and persistence, the system must set state to `Claimable` and record `ClaimableAtUtc` before returning the response.
- The system must persist itemized line entries (name, unit price, quantity) exactly as received, associated with the created receipt.
- The system must record a latency measurement (`ClaimableAtUtc - webhook received timestamp`) for each ingested transaction, retrievable via the telemetry/health surface described in the architecture spec.
- The system must log ingestion outcomes (success, duplicate-ignored, validation-failed, merchant-not-found) with enough context (merchant id, transaction id, outcome) to support operational troubleshooting and duplicate-event auditing.

### Non-Functional Requirements

- **Performance**: The ingestion pipeline (webhook receipt → persisted → claimable) must complete fast enough to support an immediate checkout tap/scan flow; p95 ingest-to-claimable latency is the primary metric to instrument (target to be set once pilot baseline data exists).
- **Reliability**: Receipt persistence and state transition must be atomic — a receipt must never be observable in a partially-written state (e.g., saved without line items, or without a claimable timestamp).
- **Idempotency/Data Integrity**: Duplicate webhook deliveries for the same transaction must be safely ignored without data corruption or duplicate rows.
- **Security**: The endpoint must reject payloads for merchants that are not registered; future hardening (e.g., verifying a Square webhook signature) must not require a schema change.
- **Observability**: Every ingestion attempt (success or failure) must be logged with a correlation-friendly identifier (transaction id) for tracing through the pipeline.

## 7. Acceptance Criteria

**Story: Receipt becomes claimable immediately after transaction**
- [ ] Given a valid webhook payload for a registered merchant, When the webhook is received, Then a receipt is persisted with state `Claimable` and a `ClaimableAtUtc` timestamp in the response.
- [ ] Given a successful ingestion, When latency is measured, Then the ingest-to-claimable duration is recorded and available for percentile reporting.

**Story: Duplicate webhook delivery is ignored**
- [ ] Given a receipt already exists for a given `(MerchantId, TransactionId)`, When the same webhook payload is delivered again, Then no second receipt is created and the response references the original receipt.
- [ ] Given a duplicate delivery occurs, When it is processed, Then the outcome is logged as "duplicate-ignored" rather than "success" or an unhandled error.

**Story: Malformed or unauthorized payloads are rejected**
- [ ] Given a webhook payload missing `TransactionId` or `Currency`, When it is submitted, Then the response is `400 Bad Request` and no receipt is created.
- [ ] Given a webhook payload referencing an unregistered `MerchantId`, When it is submitted, Then the response is `404 Not Found` and no receipt is created.

**Story: Line items are persisted accurately**
- [ ] Given a payload with N line items, When the receipt is created, Then exactly N line items are persisted with matching name, unit price, and quantity.

## 8. Out of Scope

- The checkout tap/scan claim action itself (assigning a customer stub identifier to a claimable receipt) — covered by a separate "Checkout Claim" feature.
- Anonymous history retrieval and possession/risk checks — covered by a separate feature.
- Square webhook signature/HMAC verification — noted as a future hardening item, not required for this feature's acceptance.
- Multi-POS provider support (only Square is handled).
- Receipt envelope (loyalty/ads/warranty/integration modules) — covered by a separate "Extensible Receipt Envelope" feature; this feature only guarantees the core receipt fields are correctly persisted.
- Alerting/dashboards on top of the latency telemetry (this feature only guarantees the measurement is recorded, not the presentation layer).
