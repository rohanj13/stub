# Epic PRD: Digital Receipt Platform MVP

## 1. Epic Name

Digital Receipt Platform MVP — Real-Time Square Ingestion, Checkout Claim, and Anonymous-Safe History Access

## 2. Goal

### Problem

Merchants need to move away from paper receipts without slowing down checkout, but existing digital receipt approaches either require customers to hand over personal contact information at the register or delay receipt availability past the moment the customer is standing at the point of sale. If receipt generation lags behind the transaction, the tap/scan moment at checkout is lost and the customer walks away without a receipt. If purchase history is protected only by a weak, guessable identifier, a customer's spending history can be exposed to anyone who obtains that identifier. If the receipt data model is too narrow, every future capability (loyalty, warranty, ads, compliance, integrations) requires expensive schema rework.

### Solution

Deliver an event-driven digital receipt platform that (1) ingests Square transaction webhooks and persists receipts as **claimable** within a tight latency budget so customers can tap/scan a stub identifier at checkout, (2) lets customers retrieve their receipt history without creating a full account while enforcing possession/risk checks beyond a bare identifier, and (3) stores every receipt as a core, compliance-safe object plus an extensible envelope so loyalty, warranty, ads, and integration metadata can be added later without breaking existing consumers. The MVP also establishes merchant onboarding/POS linkage (Square-first) and baseline receipt branding controls.

### Impact

- Reduced checkout friction: no forced signup, immediate receipt availability.
- Lower risk of customer data exposure through anonymous-access abuse.
- Reduced future engineering cost for loyalty/warranty/ads/integration features due to envelope-first data modeling.
- Establishes the operational telemetry (ingest-to-claimable latency) needed to prove the platform is viable for pilot merchants.

## 3. User Personas

- **Merchant Admin**: Onboards their store, connects Square, configures baseline receipt branding, and enables optional engagement modules later.
- **Store Associate**: Needs the receipt claimable immediately after payment so the customer can tap/scan without waiting at the register.
- **Customer (No Signup)**: Wants an instant receipt claim and safe access to past receipts without creating an account.
- **Customer (Opt-In)**: May later choose to share email/phone with a merchant for support, loyalty, or marketing, under explicit consent.
- **Third-Party Integrator** _(future-facing, not built in MVP)_: Needs secure, scoped APIs to consume receipt data with auditability; the MVP data model and API surface must not preclude this.

## 4. High-Level User Journeys

1. **Merchant Onboarding**: Merchant profile is created with POS account details → merchant connects to Square → integration state becomes webhook-ready → merchant configures baseline receipt branding.
2. **Real-Time Transaction to Claimable Receipt**: POS completes a transaction → Square webhook is delivered → Stub validates the payload and merchant registration → receipt is persisted and marked claimable → customer taps/scans at checkout and receives confirmation.
3. **Anonymous History Access**: Customer requests past receipts using their stub identifier → system applies possession/risk checks beyond the identifier alone → customer views their receipt history without a full account.
4. **Optional Identity Opt-In** _(design now, phase-aware rollout)_: Customer explicitly opts in to share email/phone → consent is captured with purpose, version, timestamp → merchant can use the approved contact channel per consent policy.
5. **Future Programmatic Sharing** _(out of scope for build, in scope for data model)_: Customer authorizes a downstream platform → platform receives a scoped access grant → receipt data is shared via a secure, audited API.

## 5. Business Requirements

### Functional Requirements

- The system must create merchants with required identifying fields (name, POS account id) and list/retrieve merchant integration state.
- The system must support a merchant "connect to Square" action that marks the merchant as webhook-ready.
- The system must expose a webhook ingestion endpoint for Square transaction events that rejects malformed payloads and unknown/unregistered merchants.
- The system must persist ingested transactions as digital receipts and transition them to a **claimable** state optimized for immediate post-transaction tap/scan retrieval.
- The system must emit ingest-to-claimable telemetry for operational monitoring.
- The system must support assigning/linking a customer's stub identifier to a specific receipt (the checkout claim action).
- The system must return a specific receipt by id for authorized callers and a not-found result for unknown ids.
- The system must support retrieval of a customer's receipt history by stub identifier, with additional possession/risk controls beyond the identifier alone (this MVP establishes the baseline; hardened challenge flows are a later phase).
- The system must support rate limiting, abuse detection hooks, and audit logging for receipt retrieval attempts.
- The system must store receipt customization preferences at the merchant level (name, logo, optional messaging) without breaking compliance-required receipt fields.
- The system must store each receipt as a core receipt object plus an extensible, versioned envelope container that can carry optional loyalty, ads/placement, warranty, and integration-reference modules without those modules affecting core receipt validity.
- The system must preserve fields required for regional tax/compliance profiles and maintain immutable lineage for receipt creation and modification events.
- The system must model optional customer contact fields (email/phone) separately from the anonymous stub identifier, and only store/share them on explicit opt-in with consent purpose, version, timestamp, and revocation tracking.
- The system must expose a health endpoint reflecting service status.

### Non-Functional Requirements

- **Performance**: Ingest-to-claimable latency must be measured by percentile (p50/p95/p99) and be low enough to support an immediate checkout tap/scan flow.
- **Reliability**: APIs must return deterministic errors for invalid payloads/unauthorized access; receipt state transitions must be durable and recoverable.
- **Security & Privacy**: Anonymous customers must have protected retrieval pathways for historical spending data; data minimization and least-privilege must apply to all retrieval APIs; consent/contact data must be segregated from the core anonymous receipt flow; retrieval pathways must support security logging and abuse detection.
- **Compliance & Trust**: The receipt data model must accommodate configurable regional compliance profiles and must support adding cryptographic verification (signatures/hashes) later without a schema redesign.
- **Maintainability & Extensibility**: The boundary between the core receipt object and the envelope must be explicit; new envelope modules (loyalty/ads/warranty/integrations) must be addable without breaking existing core APIs or consumers.
- **Deployability**: All services must run in Docker containers suitable for both self-hosted and pilot SaaS deployment (per current docker-compose-based architecture).

## 6. Success Metrics

1. Transaction-to-claimable receipt latency (p50/p95/p99).
2. Tap/scan claim success rate within the checkout window.
3. Receipt ingestion success rate and duplicate-event handling quality.
4. Anonymous history retrieval success rate with low unauthorized-disclosure incident count.
5. Opt-in conversion rate for email/phone sharing (once enabled).
6. Zero critical incidents involving anonymous-access data exposure during pilot.

## 7. Out of Scope

- Real-time loyalty point calculation and campaign execution logic.
- Full customer account lifecycle (password reset, profile management, account recovery).
- Multi-POS support beyond Square.
- Full global tax/legal compliance implementation for every jurisdiction.
- Complete cryptographic verification rollout (design-for-later only in this epic).
- Advanced merchant analytics dashboards and reporting exports.
- Hardened anonymous-access challenge flows (e.g., step-up verification), full consent lifecycle UI, warranty/loyalty module activation, and partner integration APIs — these belong to later phases (Phase 2/3) per the platform PRD and are explicitly not built in this epic, though the data model must not block them.
- NFC/QR wallet-pass issuance UX and native mobile app work beyond what's needed to prove the claim flow.

## 8. Business Value

**High.** This epic unlocks the core value proposition of the platform — merchants can go paperless without slowing checkout, and customers get frictionless, privacy-preserving receipt access. It also de-risks the platform's biggest long-term costs (data model rework and anonymous-access data exposure) by getting the envelope structure and access-control posture right from the first release, which directly protects the ability to sell into pilot merchants and expand into loyalty/warranty/integration revenue streams later.
