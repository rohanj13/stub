# Product Requirements Document (PRD)
## Stub Digital Receipt Infrastructure (MVP)

## 1. Document Overview
### 1.1 Purpose
This PRD defines the MVP requirements for Stub, a digital receipt infrastructure product that allows merchants to issue digital receipts from point-of-sale (POS) transactions and allows customers to access receipt history through a lightweight customer identifier (a scanned stub card ID).

### 1.2 Product Stage
This document describes the current MVP and the immediate production-hardening direction needed to make it fit for pilot usage.

### 1.3 Scope of this PRD
This PRD covers:
- Merchant onboarding and POS linkage (Square-focused)
- Webhook-based transaction ingestion
- Digital receipt creation and retrieval
- Customer assignment flow via card scan simulation
- Customer receipt history lookup
- API and frontend behaviors required for end-to-end operation

---

## 2. Problem Statement
Merchants and customers need a simple way to transition from paper receipts to digital receipts without adding high-friction sign-up steps at checkout. Existing solutions often require customer accounts, app installs, or brittle integrations that reduce adoption and increase operational overhead.

Stub solves this by:
- Letting merchants connect a POS source (Square in MVP)
- Ingesting transactions as receipts automatically
- Associating receipts to a reusable customer identifier
- Returning customer receipt history on demand

---

## 3. Product Vision
Create a reliable, extensible digital receipt backbone that can:
1. Integrate quickly with merchant POS systems
2. Minimize checkout friction for customers
3. Provide immediate and searchable receipt history
4. Form a base for loyalty, analytics, and post-purchase engagement features

---

## 4. Goals and Non-Goals
### 4.1 Goals (MVP)
1. Enable merchant creation and POS account registration.
2. Support merchant “connect to Square” action that represents webhook setup readiness.
3. Accept Square transaction webhook payloads and persist digital receipts.
4. Allow assignment of a customer identifier to a receipt after transaction.
5. Allow retrieval of receipt history by customer identifier.
6. Provide a simple frontend workflow to exercise all core flows end-to-end.

### 4.2 Non-Goals (MVP)
1. Real-time loyalty point calculation.
2. Customer authentication, password management, or account recovery.
3. Multi-POS support beyond Square.
4. Full webhook signature validation and anti-replay hardening.
5. Analytics dashboards, exports, or BI reporting.
6. Production-scale observability and SLO governance.

---

## 5. Stakeholders
- **Product Owner**: Defines rollout priorities and pilot criteria.
- **Merchant Operations**: Onboards merchants and validates receipt delivery behavior.
- **Backend Engineering**: Maintains API, data model, and integration points.
- **Frontend Engineering**: Maintains operator-facing demo/control interface.
- **QA / Pilot Support**: Verifies user journeys, edge cases, and regressions.

---

## 6. Users and Personas
### 6.1 Merchant Admin
Needs to register store details, connect POS integration, and ensure receipts are being generated.

### 6.2 Store Associate
Needs to quickly associate a customer card scan identifier to a new receipt.

### 6.3 Customer
Needs a low-friction way to later retrieve purchase receipts by customer identifier.

---

## 7. Core User Journeys
### 7.1 Merchant Onboarding
1. User creates merchant profile with name and POS account ID.
2. System persists merchant and marks webhook not yet registered.
3. User triggers connect-to-Square action.
4. System updates merchant as webhook-ready.

### 7.2 Transaction Ingestion
1. Square sends transaction webhook payload with merchant ID and line items.
2. System verifies merchant exists.
3. System creates digital receipt record with normalized currency and item list.
4. System returns created receipt metadata.

### 7.3 Customer Assignment
1. Associate scans or enters customer card ID.
2. System assigns customer ID to receipt.
3. Receipt is now discoverable via customer history lookup.

### 7.4 Receipt History Lookup
1. Customer ID is submitted.
2. System returns all matching receipts ordered newest first.
3. Caller can inspect each receipt’s amount, currency, item lines, and timestamps.

---

## 8. Functional Requirements
### 8.1 Merchant Management
- The system must allow creating a merchant with `name` and `posAccountId`.
- The system must reject merchant creation when required fields are missing.
- The system must support listing existing merchants.
- The system must support merchant POS connection state update for Square.

### 8.2 Webhook Ingestion
- The system must expose an endpoint to receive Square transaction webhooks.
- The system must reject payloads missing `transactionId` or `currency`.
- The system must reject payloads referencing unknown merchants.
- The system must persist receipt metadata and itemized line entries.

### 8.3 Receipt Retrieval
- The system must return a specific receipt by receipt ID.
- The system must return not found when a receipt ID does not exist.

### 8.4 Customer Assignment
- The system must allow assigning a `customerId` to a receipt.
- The system must reject missing `customerId` values.
- The system must return not found when assigning to a missing receipt.

### 8.5 Customer History
- The system must return all receipts associated with a customer ID.
- Matching must be case-insensitive for customer lookup.
- Returned receipts must be ordered by creation time descending.

### 8.6 Health and Operability
- The system must expose a simple health endpoint for environment checks.

---

## 9. API Requirements (MVP Endpoints)
- `GET /api/health`
- `GET /api/merchants`
- `POST /api/merchants`
- `POST /api/merchants/{merchantId}/connect/square`
- `POST /api/webhooks/square/transactions`
- `GET /api/receipts/{receiptId}`
- `POST /api/receipts/{receiptId}/assign-customer`
- `GET /api/customers/{customerId}/receipts`

All endpoints must return clear HTTP status codes for success, bad request, not found, and created-resource states where applicable.

---

## 10. Data Requirements
### 10.1 Merchant Data
- Unique merchant identifier (GUID)
- Merchant name
- POS account identifier
- POS provider (Square for MVP)
- Webhook registration flag
- Creation timestamp

### 10.2 Receipt Data
- Unique receipt identifier (GUID)
- Merchant reference
- Transaction identifier
- Optional order identifier
- Total amount
- Currency (normalized)
- Optional customer identifier
- Creation timestamp
- One-to-many receipt line items

### 10.3 Receipt Line Item Data
- Item name
- Unit price
- Quantity

---

## 11. Frontend Requirements
The frontend must provide a straightforward operational console that can:
1. Create merchants.
2. Trigger transaction webhook simulation.
3. Assign customer IDs to generated receipts.
4. Perform customer receipt history lookups.
5. Display API responses or errors in a readable output panel.

The frontend must remain configurable via environment variable for API base URL.

---

## 12. Non-Functional Requirements
### 12.1 Reliability
- Core APIs should handle malformed requests safely and return deterministic errors.
- Data should persist across restarts when backed by persistent PostgreSQL storage.

### 12.2 Performance
- Typical single-request operations for MVP should complete within interactive latency suitable for admin workflows.

### 12.3 Security (MVP Baseline)
- Input validation on required fields must be enforced.
- CORS should only allow intended frontend origin(s) for local development.
- Sensitive secrets must not be hardcoded in source.

### 12.4 Maintainability
- Business logic should remain encapsulated in service layer abstractions.
- Data model should be extensible for additional POS providers and customer channels.

---

## 13. Assumptions and Constraints
### 13.1 Assumptions
- Merchants provide valid POS account identifiers.
- Webhook payload shape is trusted in MVP simulation flow.
- Customer identifiers are externally generated and unique enough for retrieval use.

### 13.2 Constraints
- Initial provider support is Square only.
- API is implemented as a .NET minimal API backend.
- Frontend is React + Vite and not a customer-facing production portal.

---

## 14. Success Metrics (MVP)
1. Merchant onboarding completion rate for pilot users.
2. Receipt creation success rate from webhook events.
3. Customer assignment success rate.
4. Receipt history lookup success rate.
5. End-to-end flow completion time in pilot operations.

---

## 15. Risks and Mitigations
### Risk 1: Incomplete webhook hardening
- **Impact**: Invalid or spoofed events could pollute data.
- **Mitigation**: Add signature verification, idempotency keys, and replay protection post-MVP.

### Risk 2: Customer ID quality inconsistency
- **Impact**: Receipt lookup failures due to malformed IDs.
- **Mitigation**: Add stricter input normalization and card-scanner validation rules.

### Risk 3: Limited provider coverage
- **Impact**: Slow adoption for merchants on other POS platforms.
- **Mitigation**: Abstract provider adapter interface for future integrations.

---

## 16. Future Enhancements (Post-MVP)
1. Multi-provider POS adapters (e.g., Stripe Terminal, Clover).
2. Verified webhook security and event idempotency.
3. Customer-facing portal and authenticated receipt access.
4. Loyalty and rewards linkage tied to customer ID.
5. Search, export, and analytics for merchant insights.
6. Alerting and monitoring with operational dashboards.

---

## 17. Acceptance Criteria
This PRD is fit for purpose when:
1. Product, engineering, and operations teams can execute MVP implementation and pilot onboarding directly from this document.
2. Core flows, boundaries, and non-goals are unambiguous.
3. API, data, and user journey expectations are clearly defined.
4. Known risks and next-phase priorities are explicitly documented.

