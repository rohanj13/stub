# Epic PRD: Anonymous Access Hardening & Consent Lifecycle

## 1. Epic Name

Anonymous Access Hardening & Consent Lifecycle — Step-Up Verification, Consent Management, and Warranty/Loyalty Envelope Activation

## 2. Goal

### Problem

Phase 1 established a baseline: customers can retrieve receipt history using only their stub identifier, protected by rate limiting and audit logging, and every receipt is stored with an extensible envelope alongside its core fields. That baseline is not sufficient for production scale. A stub identifier alone is still a single, static secret — if it leaks (shared screenshot, shoulder-surfed QR code, log exposure), rate limiting only slows abuse, it does not stop a determined actor from viewing a customer's full spending history. Separately, customers have no supported way to voluntarily share an email or phone number with a merchant for support/loyalty/marketing purposes, and no way to see or revoke that sharing once granted — so merchants cannot legally or safely build contact-based engagement. Finally, the receipt envelope schema exists but carries no activated modules yet, so merchants cannot register warranty coverage or loyalty participation against a receipt even though the platform was explicitly designed to support this without a schema rework.

### Solution

Harden the anonymous history pathway with a step-up/challenge flow that requires proof of purchase possession (e.g., last transaction amount, last-4 of a linked payment reference, or a time-boxed one-time code delivered through a channel the customer already controls) before returning full history beyond a minimal baseline view. Introduce a first-class consent lifecycle: customers can grant, view, and revoke consent to share email/phone with a specific merchant for a specific purpose, with every grant and revocation permanently recorded (purpose, version, timestamp). Activate the warranty and loyalty envelope modules defined in Phase 1's extensible envelope: merchants can attach warranty terms/registration metadata to a receipt, and receipts can carry loyalty program participation metadata, both stored as versioned envelope modules that never affect core receipt validity.

### Impact

- Closes the platform's highest remaining privacy risk (Risk 2 in the PRD) by moving beyond "identifier + rate limit" to genuine possession/risk-based verification for full history access.
- Unlocks merchant contact-based engagement (support, loyalty, marketing) on a legally defensible, auditable consent record instead of ad hoc data collection.
- Activates two concrete revenue/retention paths (warranty registration, loyalty participation) using the envelope foundation built in Phase 1, with no core schema rework.
- Produces the audit and revocation trail needed to satisfy privacy/compliance review before wider rollout.

## 3. User Personas

- **Merchant Admin**: Wants to offer warranty registration and loyalty participation on receipts, and wants a lawful basis (consent) to contact customers who opt in.
- **Customer (No Signup)**: Wants their purchase history to stay private even if their stub identifier is exposed, without being forced into a full account.
- **Customer (Opt-In)**: Wants clear, revocable control over whether a merchant can contact them by email/phone, and for what purpose.
- **Security/Privacy Stakeholder**: Needs proof that anonymous history access requires more than a static identifier, and that every consent grant/revocation is auditable.
- **Compliance/Legal Stakeholder**: Needs consent purpose/version/timestamp/revocation tracking sufficient to answer "what did this customer agree to, and when."
- **Third-Party Integrator** _(future-facing, not built in this epic)_: Will eventually consume warranty/loyalty envelope data via scoped APIs; this epic must not preclude that.

## 4. High-Level User Journeys

1. **Step-Up Challenge on History Access**: Customer requests full receipt history using their stub identifier → system returns a minimal baseline result and a challenge requirement → customer supplies a possession proof (e.g., last purchase total, or a one-time code sent to a channel they already have from a prior consented interaction) → system verifies the proof and returns full history, logging the challenge outcome.
2. **Consent Grant**: Customer chooses to share an email/phone with a merchant for a stated purpose (e.g., "receipt delivery," "loyalty updates") → system records the consent grant with purpose, version, and timestamp → merchant can subsequently use that contact channel strictly within the granted purpose.
3. **Consent Review and Revocation**: Customer views their active consent grants for a merchant → customer revokes a grant → system marks it revoked with a timestamp and immediately stops surfacing that contact channel to the merchant for that purpose.
4. **Warranty Registration**: Merchant (or customer, where enabled) attaches warranty terms to a receipt (product reference, coverage period, terms link) → system stores this as a versioned warranty module in the receipt envelope → warranty status is retrievable alongside the receipt without altering core receipt fields.
5. **Loyalty Participation Activation**: Merchant enables loyalty participation for a receipt (program reference, accrual/points metadata placeholder) → system stores this as a versioned loyalty module in the receipt envelope → loyalty metadata is retrievable alongside the receipt, with actual point calculation explicitly out of scope.

## 5. Business Requirements

### Functional Requirements

- The system must classify anonymous history requests into at least two response tiers: a minimal baseline view (no challenge required, consistent with Phase 1 behavior) and a full-history view (challenge required).
- The system must support at least one possession-based challenge mechanism (e.g., verify a submitted value against a recent transaction attribute, or issue and validate a short-lived one-time code) before releasing full history.
- The system must record every challenge attempt (issued, succeeded, failed, expired) with identifier, timestamp, and outcome, without logging the raw challenge secret in plaintext audit records.
- The system must rate-limit challenge issuance and verification attempts per stub identifier, independent of the Phase 1 history rate limit.
- The system must provide endpoints to grant, list, and revoke consent for a customer/merchant/purpose combination.
- Each consent record must capture: customer stub identifier reference, merchant reference, contact channel (email or phone), purpose, consent version, granted-at timestamp, and revoked-at timestamp (nullable).
- The system must reject any attempt to use a contact channel for a purpose that has no active (non-revoked) consent record covering that purpose.
- The system must treat consent revocation as immediate and non-reversible by the merchant (a new grant, not an un-revoke, is required to restore access).
- The system must store contact data (email/phone) separately from the core anonymous receipt/customer linkage, consistent with the Phase 1 data-segregation requirement.
- The system must support attaching a warranty module to a receipt envelope, including at minimum: product reference, coverage start/end, and a terms reference (link or document id).
- The system must support attaching a loyalty module to a receipt envelope, including at minimum: loyalty program reference and a participation flag/status; point/accrual calculation logic is explicitly not required.
- Both warranty and loyalty modules must be independently addable/removable from a receipt's envelope without modifying or invalidating the core receipt fields or other envelope modules.
- The system must version each envelope module so future module schema changes do not break receipts created under an earlier module version.
- The system must expose read access to warranty and loyalty module data alongside the existing receipt-by-id retrieval, without requiring a separate authorization model beyond what already governs receipt access.

### Non-Functional Requirements

- **Security & Privacy**: Full history access must require verifiable possession evidence beyond the bare stub identifier; challenge secrets/one-time codes must never be persisted in plaintext and must expire within a short, configurable window.
- **Auditability**: Every challenge attempt and every consent grant/revocation must be independently auditable, with enough context to reconstruct "who was granted what, when, and by which action" for compliance review.
- **Data Minimization**: Consent-gated contact fields must remain excluded from anonymous/baseline receipt and history responses; they must only surface through consent-aware pathways.
- **Reliability**: Consent state (granted/revoked) must be strongly consistent — a revocation must not be visible as "still active" due to caching or read staleness.
- **Extensibility**: Warranty and loyalty modules must follow the same versioned-envelope pattern established in Phase 1 so additional modules (e.g., ads/placement, integrations) can be added later without touching this epic's schema.
- **Performance**: Challenge issuance/verification must not materially degrade the history-retrieval latency budget established in Phase 1 for the baseline (non-challenged) path.
- **Deployability**: All new services/tables must run within the existing Docker Compose topology (api/db/frontend) without introducing new required infrastructure for the MVP of this epic (e.g., no hard dependency on an external SMS/email provider beyond a pluggable interface).

## 6. Success Metrics

1. Full-history challenge success rate (legitimate customers passing the challenge on first or second attempt).
2. Reduction in anonymous-access abuse indicators (rate-limit trips, repeated failed challenges) relative to the Phase 1 baseline.
3. Consent grant completion rate and revocation responsiveness (time from revocation request to enforced effect).
4. Number of merchants activating warranty and/or loyalty envelope modules.
5. Zero incidents of contact-channel use without an active, matching consent record.
6. Zero critical incidents involving full-history disclosure without a successfully verified challenge.

## 7. Out of Scope

- Real-time loyalty point calculation, tiering, and campaign execution logic (only participation metadata storage is in scope).
- Warranty claim processing/adjudication workflow (only registration/terms metadata storage is in scope).
- Full customer account lifecycle (password reset, profile management, account recovery).
- Multi-channel challenge delivery beyond a minimal viable set (e.g., building a full SMS/email provider integration is not required; a pluggable interface is sufficient).
- Cryptographic receipt verification (signatures/hashes) — remains a Phase 3 concern per the platform PRD.
- Partner/integrator programmatic access APIs — remains a Phase 3 concern.
- Merchant-facing UI polish for warranty/loyalty configuration beyond functional forms needed to demonstrate the capability.
- Marketing campaign delivery mechanics that would consume granted consent (this epic only guarantees consent is capturable, auditable, and enforceable — not that marketing sends are built).

## 8. Business Value

**High.** This epic closes the platform's most significant remaining privacy exposure (anonymous history access secured by nothing more than a static identifier) and unlocks the lawful basis merchants need to engage customers by contact channel. It also converts the Phase 1 envelope investment into two concrete, sellable merchant capabilities — warranty registration and loyalty participation — without any core schema rework, directly validating the extensibility bet made in the MVP epic and expanding what the platform can be sold on for pilot and post-pilot merchants.
