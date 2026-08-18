# Product Requirements Document (PRD)
## Stub Digital Receipt Infrastructure (MVP + Near-Term Platform Extensions)

## 1. Document Overview
### 1.1 Purpose
This PRD defines the requirements for Stub as a digital receipt infrastructure platform that can issue receipts from POS transactions, support near-instant card tap/scan retrieval at checkout, and safely enable later receipt access for customers who may never create an account.

### 1.2 Product Stage
This document covers:
1. MVP capabilities needed for pilot deployment.
2. Immediate production-hardening requirements.
3. Next-phase platform requirements that must influence today’s data model and APIs.

### 1.3 Scope of this PRD
This PRD covers:
- Merchant onboarding and POS linkage (Square-first)
- Real-time webhook-driven receipt availability immediately after transaction
- NFC tap / QR scan customer claim flow at checkout
- Anonymous user receipt access protection and privacy controls
- Consent-based future identity opt-in (email/phone sharing)
- Receipt customization and receipt envelope extensibility
- Compliance-oriented receipt structure for multi-region expansion
- Future integration APIs for downstream platforms and programmable receipt sharing

---

## 2. Problem Statement
Merchants need to move away from paper receipts without slowing checkout. Customers need receipt access with minimal friction, including immediate post-payment retrieval and later history access without forced signup.

Current challenges:
1. If receipt generation is delayed, the customer tap/scan moment is lost.
2. If history is exposed by a weak identifier alone, spending data can be abused.
3. If receipt structure is too narrow, future loyalty/ads/warranty/compliance use cases require expensive rework.

Stub addresses this by combining:
- Event-driven receipt ingestion
- Fast tap/scan claimability at the point of sale
- Privacy-preserving access controls for anonymous users
- An extensible receipt envelope designed for future integrations

---

## 3. Product Vision
Build a globally extensible receipt platform that:
1. Makes digital receipts available immediately after checkout.
2. Preserves low-friction user experience without mandatory account creation.
3. Protects customer purchase data even in anonymous mode.
4. Supports merchant customization and post-purchase ecosystems (loyalty, ads, warranty, integrations).
5. Evolves toward verifiable, standards-aligned digital receipts across jurisdictions.

---

## 4. Goals and Non-Goals
### 4.1 Goals (MVP + Immediate Hardening)
1. Enable merchant creation and POS account registration.
2. Support merchant “connect to Square” action and webhook readiness state.
3. Accept Square transaction webhooks and persist receipts immediately.
4. Ensure receipt readiness fast enough for immediate NFC/QR tap flows at checkout.
5. Support assignment/linking of customer stub card identifier to a receipt.
6. Support secure retrieval of past receipts for customers without full signup.
7. Provide baseline receipt customization controls for merchant branding.
8. Model receipt data in an envelope that is extensible for loyalty/ads/warranty use cases.
9. Define future-safe interfaces for secure downstream integration and programmable sharing.

### 4.2 Non-Goals (Current MVP)
1. Real-time loyalty point calculation and campaign execution.
2. Full customer account lifecycle (password reset, profile management, account recovery).
3. Multi-POS support beyond Square in the first release.
4. Full global tax/legal compliance implementation for every jurisdiction at launch.
5. Complete cryptographic trust framework rollout at launch (must be designed for, but phased).
6. Advanced merchant analytics dashboards and reporting exports.

---

## 5. Stakeholders
- **Product Owner**: Owns roadmap and phased release boundaries.
- **Merchant Operations**: Onboards merchants and validates checkout/tap flow usability.
- **Backend Engineering**: Owns ingestion, data model, security, and APIs.
- **Frontend Engineering**: Owns operator tools and receipt presentation surfaces.
- **Security/Privacy**: Owns anonymous-access threat model and controls.
- **Compliance/Legal**: Owns receipt standard and retention/regulatory requirements by region.
- **Partnerships/Integrations**: Owns downstream platform enablement requirements.

---

## 6. Users and Personas
### 6.1 Merchant Admin
Needs to onboard store, configure integrations, customize receipt appearance, and enable optional engagement modules.

### 6.2 Store Associate
Needs receipt to be claimable immediately after payment so customers can tap NFC card or scan QR without delay.

### 6.3 Customer (No Signup)
Needs instant receipt claim and safe access to past receipts without creating a full account.

### 6.4 Customer (Opt-In)
May later choose to share email/phone with merchant for support, loyalty, or marketing consented channels.

### 6.5 Third-Party Integrator
Needs secure APIs to ingest or consume receipt data with explicit permissions and auditability.

---

## 7. Core User Journeys
### 7.1 Merchant Onboarding
1. Merchant profile is created with POS account details.
2. Merchant integration state moves to webhook-ready.
3. Merchant can configure baseline receipt branding preferences.

### 7.2 Real-Time Transaction to Claimable Receipt
1. POS completes transaction.
2. Square webhook is delivered immediately.
3. Stub validates and persists receipt.
4. Stub marks receipt as claimable for customer tap/scan.
5. Customer taps NFC wallet pass or scans QR at checkout and receives receipt confirmation.

### 7.3 Anonymous History Access
1. Customer requests past receipts using their stub identifier.
2. System enforces possession/risk controls before returning spending history.
3. Customer views receipts without full account signup.

### 7.4 Optional Identity Opt-In
1. Customer explicitly opts in to provide email/phone.
2. Consent is captured with timestamp and scope.
3. Merchant can use approved contact channel based on consent policy.

### 7.5 Future Programmatic Sharing
1. Customer authorizes a downstream platform.
2. Platform obtains scoped access token/consent grant.
3. Receipt data is shared via secure API with traceability.

---

## 8. Functional Requirements
### 8.1 Merchant Management
- The system must create merchants with required identifying fields.
- The system must support listing and retrieving merchant integration state.
- The system must store receipt customization preferences at merchant level.

### 8.2 Real-Time Ingestion and Claimability
- The system must expose webhook ingestion endpoints for POS transaction events.
- The system must reject malformed payloads and unknown merchants.
- The system must persist receipts and transition them to a **claimable** state.
- The system must optimize for immediate post-transaction tap/scan flow.
- The system must provide ingest-to-claimability telemetry for operational monitoring.

### 8.3 Checkout Tap/Scan Experience
- The system must support customer lookup/claim initiation via NFC or QR-linked identifiers.
- The tap/scan journey must occur immediately after transaction completion without manual operator delay.
- The system must handle cases where receipt is not yet ready and provide deterministic retry guidance.

### 8.4 Receipt Retrieval
- The system must return a specific receipt by ID for authorized callers.
- The system must return not found for unknown receipt IDs.

### 8.5 Anonymous Customer History Access Controls
- The system must not rely solely on public/stable customer identifiers for unrestricted history access.
- The system must apply additional possession/risk checks for past receipt retrieval.
- The system must support rate limiting, abuse detection, and audit logging for retrieval attempts.
- The system must support configurable data minimization/redaction for anonymous views.

### 8.6 Consent and Contact Opt-In (Phase-Aware)
- The system must model optional customer contact fields separately from anonymous identifier.
- The system must only store/share email/phone on explicit opt-in.
- The system must track consent purpose, version, timestamp, and revocation status.

### 8.7 Receipt Customization
- The system must support baseline merchant branding elements (name, logo, optional messaging).
- The system must support configurable receipt sections that do not break compliance-required fields.

### 8.8 Extensible Receipt Envelope
- Receipt must be stored in a core receipt object plus an extensible envelope.
- Envelope must support optional modules for:
  - Loyalty and campaign metadata
  - Third-party merchant placements/ads (subject to policy)
  - Warranty registration and claim-support metadata
  - Integration references for downstream systems
- Optional modules must not compromise core receipt validity.

### 8.9 Compliance and Verification Readiness
- The receipt model must preserve fields needed for regulatory/tax compliance by market.
- The system must support immutable event lineage for receipt creation and modification.
- The design must allow phased cryptographic verification (e.g., signatures/hashes) for customer and merchant trust.

### 8.10 Integration APIs (Future-Compatible)
- The platform must expose secure APIs for approved downstream platforms.
- APIs must support scoped authorization, consent-aware access, and audit trails.
- APIs must allow customer-controlled sharing of receipt data programmatically.

### 8.11 Health and Operability
- The system must expose a health endpoint.
- The system must expose readiness and core flow metrics, including ingest-to-claimable latency.

---

## 9. API Requirements
### 9.1 Current MVP Endpoints
- `GET /api/health`
- `GET /api/merchants`
- `POST /api/merchants`
- `POST /api/merchants/{merchantId}/connect/square`
- `POST /api/webhooks/square/transactions`
- `GET /api/receipts/{receiptId}`
- `POST /api/receipts/{receiptId}/assign-customer`
- `GET /api/customers/{customerId}/receipts`

### 9.2 Planned Endpoint Classes (Next Phases)
- Tap/scan claim endpoints for NFC/QR flows
- Anonymous history retrieval endpoints with possession/risk challenges
- Consent management endpoints (grant/revoke/view)
- Merchant receipt customization endpoints
- Partner integration endpoints with scoped tokens and user-authorized sharing

All endpoints must return clear status codes, deterministic error contracts, and audit-friendly metadata where security-sensitive.

---

## 10. Data Requirements
### 10.1 Merchant Data
- Merchant identifier and POS linkage
- Integration readiness states
- Receipt customization settings

### 10.2 Core Receipt Data
- Receipt identifier, merchant reference, transaction reference
- Order/totals/currency/timestamps
- Customer linkage reference (anonymous-first)
- Itemized line entries
- Compliance-required receipt attributes per region profile

### 10.3 Receipt Envelope Data (Extensible)
- Module container/versioning fields
- Loyalty and promotion metadata (optional)
- Ad/placement metadata (optional and policy-controlled)
- Warranty metadata (optional, including product references and warranty terms links)
- Third-party integration references and provenance metadata

### 10.4 Access and Security Data
- Claimability state and state transition timestamps
- Retrieval risk controls (rate-limit counters, challenge status, audit logs)
- Consent records for contact sharing (email/phone)
- Cryptographic verification fields reserved for phased rollout (hash/signature references)

---

## 11. Frontend Requirements
The frontend/operator surface must:
1. Support full merchant setup and test transaction flow.
2. Reflect real-time receipt claimability status after webhook ingestion.
3. Support tap/scan simulation workflow in test mode.
4. Support secure anonymous receipt lookup flow and error states.
5. Support baseline receipt customization management.
6. Display clear operational feedback for latency and retrieval failures.

---

## 12. Non-Functional Requirements
### 12.1 Reliability
- APIs must return deterministic errors for invalid payloads and unauthorized access.
- Receipt state transitions must be durable and recoverable.

### 12.2 Performance
- System must support near-immediate receipt claimability after transaction ingestion.
- Ingest-to-claimable latency targets must be defined and monitored by percentile.

### 12.3 Security and Privacy
- Anonymous users must have protected access pathways for historical spending data.
- Data minimization and least-privilege principles must apply to all retrieval APIs.
- Consent and contact data must be segmented from core anonymous receipt flow.
- Security logging and abuse detection must be built into receipt retrieval pathways.

### 12.4 Compliance and Trust
- Receipt data model must align with configurable regional compliance profiles.
- Architecture must support future cryptographic verification without schema redesign.

### 12.5 Maintainability and Extensibility
- Core receipt and envelope boundaries must be explicit.
- Feature modules (loyalty/ads/warranty/integrations) must be addable without breaking core APIs.

---

## 13. Assumptions and Constraints
### 13.1 Assumptions
- Checkout staff and merchant devices can support NFC/QR interactions.
- POS webhook delivery is timely enough for immediate customer claim flow.
- Customers may remain anonymous indefinitely and still expect secure history access.

### 13.2 Constraints
- Initial implementation remains Square-first.
- Current backend is .NET minimal API with PostgreSQL storage.
- Some hardening features may be phased, but data model must be future-ready now.

---

## 14. Success Metrics
1. Transaction-to-claimable receipt latency (p50/p95/p99).
2. Tap/scan success rate within checkout window.
3. Receipt ingestion success rate and duplicate-event handling quality.
4. Anonymous history retrieval success rate with low unauthorized disclosure incidents.
5. Opt-in conversion rate for email/phone sharing (when enabled).
6. Integration readiness and successful downstream API consumption.

---

## 15. Risks and Mitigations
### Risk 1: Delayed webhook pipeline breaks tap moment
- **Impact**: Customer cannot retrieve receipt during checkout.
- **Mitigation**: Latency SLOs, async pipeline monitoring, retry design, and fallback UX.

### Risk 2: Weak anonymous history access controls
- **Impact**: Exposure of spending history despite no account model.
- **Mitigation**: Possession/risk checks, rate limiting, anomaly detection, and redaction controls.

### Risk 3: Compliance gaps across regions
- **Impact**: Receipts unusable for legal/tax workflows in some markets.
- **Mitigation**: Region-profiled receipt schema and legal review checkpoints.

### Risk 4: Extensibility debt in receipt model
- **Impact**: Expensive rework when adding loyalty/ads/warranty/integrations.
- **Mitigation**: Envelope-first model with versioned optional modules.

### Risk 5: Trust concerns without verification
- **Impact**: Disputes over receipt authenticity.
- **Mitigation**: Design now for cryptographic verification; phase implementation with clear milestones.

---

## 16. Phase Guidance
### Phase 1 (MVP Pilot)
- Merchant onboarding, Square webhook ingestion, receipt creation, customer assignment, baseline lookup.
- Basic protection controls for anonymous history access.
- Baseline receipt customization and envelope schema foundation.

### Phase 2
- Hardened anonymous access controls and challenge flows.
- Contact opt-in and consent lifecycle.
- Warranty and loyalty module activation.

### Phase 3
- Cryptographic receipt verification rollout.
- Partner ecosystem APIs for secure programmatic sharing.
- Expanded compliance packs and additional POS providers.

---

## 17. Acceptance Criteria
This PRD is fit for purpose when:
1. It explicitly covers immediate post-transaction tap/scan requirements and latency expectations.
2. It defines secure access principles for no-signup customer receipt history.
3. It captures phased identity opt-in and consent requirements.
4. It defines a receipt envelope that supports customization, loyalty/ads, warranty, and downstream integrations.
5. It includes compliance-readiness and verification-oriented design requirements.
6. Product, engineering, security, and compliance teams can execute phased delivery from this document.
