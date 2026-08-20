# Implementation Sequence — Digital Receipt Platform

## How to Use This Document

This document provides a **single, dependency-ordered sequence** for implementing every feature across all three epics of the Digital Receipt Platform. Each feature is positioned so that its hard dependencies are built first, with ordering within each tier optimized for:

1. **Unlocking downstream features** — features that many others depend on come first
2. **De-risking core assumptions** — proving the fundamental capture/claim mechanism works before building engagement layers
3. **Incremental value delivery** — smallest buildable units that validate the platform's viability

**Implementation plans will be generated one feature at a time, in this order.** Each feature should be built, tested, and validated before moving to the next.

---

## Epic Summary

- **Epic 1 — Digital Receipt Platform MVP**: 6 features establishing real-time ingestion, claim, history access, and extensible data model
- **Epic 2 — Anonymous Access Hardening & Consent**: 4 features adding possession-based security, consent management, and warranty/loyalty modules
- **Epic 3 — Trust & Partner Ecosystem**: 4 features enabling cryptographic verification, partner APIs, compliance profiling, and multi-POS support

**Total: 14 features** across 3 epics

---

## Foundational Infrastructure Prerequisites

Before building any feature, the following infrastructure must be in place:

1. **Database schema baseline** — PostgreSQL with EF Core, migration-ready
2. **API scaffold** — ASP.NET Core Minimal API with health endpoint
3. **Frontend scaffold** — React + Vite operator SPA with CORS-enabled API calls
4. **Docker Compose topology** — api, db, frontend containers running locally
5. **Base entity models** — Merchant, Receipt, Customer (stub identifier reference) with state enums
6. **Structured logging** — Correlation IDs, log levels, audit-trail foundation
7. **Rate limiting abstraction** — In-process counter implementation, pluggable for Redis later

These are not "features" — they are the baseline needed for feature #1 to have something to build on.

---

## Implementation Sequence

### **Feature 1: Merchant Onboarding & Square Connect**

**Epic**: Digital Receipt Platform MVP

**Why Here**: Zero dependencies. Every other feature requires merchant records to exist and be linkable to a POS provider. This establishes the authoritative merchant registry used by ingestion, branding, and all downstream features.

**What It Depends On**:
- Foundational infrastructure only (database, API scaffold, frontend)

**What Depends On It**:
- Real-Time Square Transaction Ingestion (needs registered merchants to validate against)
- Receipt Branding Customization (merchant-level branding preferences)
- All Epic 2/3 features (consent, compliance profiles tied to merchants)

**What It Exposes/Unlocks**:
- Merchant registry with POS account linkage
- Webhook-ready state flag for ingestion validation
- Merchant list/detail retrieval for operators

**Build Size**: **S** — Basic CRUD for merchant entity, Square connect action (simulated OAuth), list/detail endpoints, frontend merchant management screen

**Infra/Foundation Work Alongside**:
- EF Core migration for Merchant entity
- Merchant entity with fields: Id, Name, PosAccountId, PosProvider, WebhookRegistered
- Validation middleware for required fields
- Duplicate POS account ID prevention

---

### **Feature 2: Extensible Receipt Envelope (Foundation)**

**Epic**: Digital Receipt Platform MVP

**Why Here**: Must exist before any feature writes receipt data. The envelope is the platform's core extensibility bet — establishing it now (before any concrete module is built) prevents expensive rework. No runtime logic yet, just the schema foundation.

**What It Depends On**:
- Foundational infrastructure only

**What Depends On It**:
- Real-Time Square Transaction Ingestion (persists receipts with envelope column)
- All envelope module features (Warranty, Loyalty in Epic 2)
- All Epic 3 features (verification signs core+envelope, partner APIs scope envelope modules)

**What It Exposes/Unlocks**:
- JSONB envelope column on Receipt entity, versioned structure `{ "version": 1, "modules": {} }`
- Validation that envelope presence/absence never affects core receipt validity
- Reserved module keys (loyalty, ads, warranty, integrations) documented but unimplemented

**Build Size**: **S** — Schema addition, validation logic, no UI or runtime business logic

**Infra/Foundation Work Alongside**:
- EF Core migration for Receipt entity with Envelope JSONB column
- Envelope shape validation (must be JSON object with version field)
- Documentation of reserved module namespace

---

### **Feature 3: Real-Time Square Transaction Ingestion & Claimable Receipt Processing**

**Epic**: Digital Receipt Platform MVP

**Why Here**: This is the platform's **core value delivery mechanism**. Without ingestion, there are no receipts to claim, retrieve, brand, or extend. Positioned after Merchant Onboarding (needs to validate merchant registration) and Envelope (persists receipts with envelope from day 1). This is the highest-priority feature to prove the platform works.

**What It Depends On**:
- Merchant Onboarding & Square Connect (validates against registered merchants)
- Extensible Receipt Envelope (persists receipts with envelope column from the start)

**What Depends On It**:
- Checkout Claim (assigns customers to claimable receipts)
- Anonymous History Baseline Access (retrieves ingested receipts)
- Receipt Branding Customization (brands ingested receipts)
- All Epic 2/3 features (operate on ingested receipts)

**What It Exposes/Unlocks**:
- Webhook endpoint: `POST /api/webhooks/square/transactions`
- Receipt state machine: Pending → Claimable → Claimed
- Idempotency on (MerchantId, TransactionId)
- Ingest-to-claimable latency telemetry (p50/p95/p99)
- Line items persisted alongside receipt

**Build Size**: **M** — Webhook validation, idempotency handling, state machine, latency measurement, audit logging, Square payload parsing

**Infra/Foundation Work Alongside**:
- EF Core migrations for Receipt, LineItem entities
- Receipt entity: Id, MerchantId (FK), TransactionId, Currency, TotalAmount, State (enum), CreatedAtUtc, ClaimableAtUtc, ClaimedAtUtc, CustomerId (nullable), Envelope (JSONB)
- LineItem entity: Id, ReceiptId (FK), Name, UnitPrice, Quantity
- Idempotency table or unique constraint on (MerchantId, TransactionId)
- Structured latency logging (webhook received → claimable timestamp delta)
- Telemetry/health endpoint exposing p50/p95/p99 ingest-to-claimable duration

---

### **Feature 4: Checkout Claim (Customer-to-Receipt Assignment)**

**Epic**: Digital Receipt Platform MVP

**Why Here**: Depends on ingestion existing to produce claimable receipts. Enables the core "tap NFC / scan QR at checkout" journey. Without this, customers can't link receipts to their stub identifier. Must come before history retrieval (which needs claimed receipts).

**What It Depends On**:
- Real-Time Square Transaction Ingestion (produces claimable receipts)

**What Depends On It**:
- Anonymous History Baseline Access (retrieves claimed receipts by customer)
- All Epic 2/3 features (consent, challenges, partner APIs all reference customer-claimed receipts)

**What It Exposes/Unlocks**:
- Claim endpoint: `POST /api/receipts/{receiptId}/assign-customer`
- State transition: Claimable → Claimed with CustomerId and ClaimedAtUtc
- Idempotent claim (re-claiming with same customer is success)
- Conflict handling (different customer attempts to claim already-claimed receipt)

**Build Size**: **S** — Endpoint, state-machine guard logic, conflict/retry response handling

**Infra/Foundation Work Alongside**:
- Update Receipt entity to enforce state-transition rules (Claimable → Claimed only)
- Validation that CustomerId is non-empty
- ClaimedAtUtc timestamp population on successful claim

---

### **Feature 5: Anonymous History Baseline Access**

**Epic**: Digital Receipt Platform MVP

**Why Here**: Depends on ingestion and claim existing. Delivers the customer-facing "view my receipts" capability. Positioned before branding so customers can retrieve receipts even without merchant branding configured. Includes MVP security (rate limiting, audit logging) that Epic 2's hardened challenge flow will build on.

**What It Depends On**:
- Real-Time Square Transaction Ingestion (receipts exist)
- Checkout Claim (receipts are linked to customer identifiers)

**What Depends On It**:
- Hardened Anonymous Access & Challenge Flows (Epic 2, extends this with step-up verification)
- All partner/integrations features (Epic 3, consume history data)

**What It Exposes/Unlocks**:
- History endpoint: `GET /api/customers/{customerId}/receipts`
- Rate limiting per stub identifier
- Audit logging for every retrieval attempt
- Data minimization (excludes future consent-only fields from response)

**Build Size**: **S** — Query endpoint, rate limiter integration, audit log writes, response ordering (most recent first)

**Infra/Foundation Work Alongside**:
- Rate limiter abstraction (in-process counter with configurable window/threshold)
- Audit log table: identifier, timestamp, outcome (success/rate-limited/error), result count
- Configuration for rate-limit window/threshold (externally configurable)

---

### **Feature 6: Merchant Receipt Branding & Customization**

**Epic**: Digital Receipt Platform MVP

**Why Here**: Depends only on merchant records existing. Independent of ingestion/claim/history logic. Can be built in parallel with or after Feature 5. Positioned last in Epic 1 because it's additive (improves merchant satisfaction) but not blocking for core platform validation.

**What It Depends On**:
- Merchant Onboarding & Square Connect (branding tied to merchant)

**What Depends On It**:
- None directly, but receipt retrieval responses (in Feature 5 and all later features) will include branding

**What It Exposes/Unlocks**:
- Merchant branding preferences: display name, logo reference (URL), optional message
- Branding GET/PUT endpoints per merchant
- Receipt retrieval includes merchant branding alongside core receipt data

**Build Size**: **S** — Merchant branding entity, GET/PUT endpoints, validation (max length for message), frontend branding config form

**Infra/Foundation Work Alongside**:
- EF Core migration for MerchantBranding entity or branding fields on Merchant entity
- Fields: DisplayName, LogoUrl (nullable), Message (nullable, max length enforced)
- Validation that branding cannot overwrite core receipt compliance fields

---

## Epic 1 Complete ✓

At this point, the platform can:
- Onboard merchants and link to Square
- Ingest Square transactions in real-time, producing claimable receipts
- Let customers claim receipts at checkout via stub identifier
- Retrieve receipt history with baseline security (rate limiting, audit logging)
- Display merchant branding on receipts

**Pilot-ready for core value validation**. Epic 2 now hardens security and activates engagement modules.

---

### **Feature 7: Extensible Receipt Envelope — Warranty Module Activation**

**Epic**: Anonymous Access Hardening & Consent Lifecycle

**Why Here**: First concrete use of the envelope established in Epic 1. No dependencies beyond Epic 1 being complete. Positioned before Loyalty to prove the pattern with the simpler of the two modules first. Must come before hardened challenge flows and consent (which are cross-cutting concerns), since warranty is a vertical capability test.

**What It Depends On**:
- Extensible Receipt Envelope (Feature 2 — envelope column exists)
- Real-Time Square Transaction Ingestion (Feature 3 — receipts exist to attach warranty to)

**What Depends On It**:
- None directly, but proves envelope extensibility for all future modules

**What It Exposes/Unlocks**:
- Warranty module attach/read/update endpoints for a receipt
- Envelope shape: `envelope.warranty = { moduleVersion, productRef, coverageStart, coverageEnd, termsRef }`
- Validation that warranty writes don't corrupt core receipt or other modules

**Build Size**: **S** — Envelope module writer/reader helpers, warranty attach/read/update endpoints, validation (end >= start), frontend warranty config form

**Infra/Foundation Work Alongside**:
- Envelope module validation pattern (namespaced, versioned)
- Documentation of warranty module schema version 1

---

### **Feature 8: Extensible Receipt Envelope — Loyalty Module Activation**

**Epic**: Anonymous Access Hardening & Consent Lifecycle

**Why Here**: Second concrete envelope module, proves the pattern generalizes. No dependencies beyond Epic 1 and warranty proving the pattern. Can be built immediately after warranty or in parallel.

**What It Depends On**:
- Extensible Receipt Envelope (Feature 2)
- Real-Time Square Transaction Ingestion (Feature 3)
- (Soft: Warranty Module Activation for pattern reuse, but technically independent)

**What Depends On It**:
- None directly, completes envelope module validation

**What It Exposes/Unlocks**:
- Loyalty module attach/read/update endpoints for a receipt
- Envelope shape: `envelope.loyalty = { moduleVersion, programRef, participationStatus }`
- Participation status set (enrolled, pending, not-participating), extensible

**Build Size**: **S** — Loyalty module writer/reader helpers, loyalty attach/read/update endpoints, status validation, frontend loyalty config form

**Infra/Foundation Work Alongside**:
- Loyalty module schema version 1 documentation
- Validation that loyalty writes don't corrupt core receipt or warranty module

---

### **Feature 9: Consent & Contact Opt-In**

**Epic**: Anonymous Access Hardening & Consent Lifecycle

**Why Here**: Must exist before hardened challenge flows (which may deliver one-time codes to consented contact channels) and before any partner API (which extends consent to programmatic access grants in Epic 3). Independent of warranty/loyalty modules — can be built in parallel or after them.

**What It Depends On**:
- Merchant Onboarding (consent tied to merchants)
- (Soft: Anonymous History Baseline Access — consent data excluded from baseline history responses)

**What Depends On It**:
- Hardened Anonymous Access & Challenge Flows (may use consented contact channel for OTP delivery)
- Partner Integration APIs (Epic 3 — extends consent model to partner access grants)

**What It Exposes/Unlocks**:
- Consent grant/list/revoke endpoints
- Consent record: customer stub identifier, merchant, contact channel (email/phone), contact value, purpose, version, granted-at, revoked-at
- Enforcement check: any contact-channel use must resolve against active consent
- Data segregation: contact values never appear in baseline receipt/history responses

**Build Size**: **M** — Consent entity, grant/list/revoke endpoints, enforcement check abstraction, audit logging (grant/revoke events), frontend consent management screen

**Infra/Foundation Work Alongside**:
- EF Core migration for ConsentRecord entity
- Fields: Id, CustomerStubId (FK or string ref), MerchantId (FK), ContactChannelType (enum), ContactValue, Purpose, ConsentVersion, GrantedAtUtc, RevokedAtUtc (nullable)
- Unique constraint on (CustomerStubId, MerchantId, Purpose, ContactChannelType) for active grants
- Consent enforcement helper function for future contact-consuming features

---

### **Feature 10: Hardened Anonymous Access & Challenge Flows**

**Epic**: Anonymous Access Hardening & Consent Lifecycle

**Why Here**: Extends Feature 5 (Anonymous History Baseline Access). Must come after consent feature (may use consented contact channel for OTP delivery). This is the highest-priority security hardening — closes Risk 2 from the PRD.

**What It Depends On**:
- Anonymous History Baseline Access (Feature 5 — extends with challenge-gated full-history tier)
- Consent & Contact Opt-In (Feature 9 — may deliver OTP to consented contact channel)

**What Depends On It**:
- None directly, completes Epic 2 security hardening

**What It Exposes/Unlocks**:
- Tiered history response: baseline (no challenge) vs. full (challenge-gated)
- Challenge issuance/verification endpoints
- Possession-based proof mechanisms (match last transaction attribute OR verify one-time code)
- Challenge attempt audit log (issued/succeeded/failed/expired)
- Independent rate limiting for challenge issuance/verification

**Build Size**: **M** — Challenge issuance/verification logic, one-time code generation (salted hash storage), pluggable notification interface (log/in-memory for MVP), tiered history response logic, challenge attempt audit log, rate limiter for challenges

**Infra/Foundation Work Alongside**:
- EF Core migration for ChallengeAttempt entity
- Fields: Id, CustomerStubId, ChallengeType, IssuedAtUtc, ExpiresAtUtc, ChallengeSecretHash (never plaintext), Outcome (enum), VerifiedAtUtc (nullable)
- INotificationSender interface with log-based implementation (seam for real SMS/email provider later)
- Challenge rate limiter (separate budget from Feature 5's history rate limiter)
- Configuration for challenge expiry window (default: few minutes, configurable)

---

## Epic 2 Complete ✓

At this point, the platform has:
- Hardened anonymous history access (possession-based challenge, not just identifier)
- First-class consent lifecycle (grant/revoke/audit contact sharing)
- Two activated envelope modules (warranty, loyalty) proving extensibility without core schema rework

**Security hardened, engagement modules proven, consent foundation laid for Epic 3 partner integrations.**

---

### **Feature 11: Cryptographic Receipt Verification**

**Epic**: Trust, Compliance & Partner Ecosystem

**Why Here**: No dependencies beyond Epic 1 receipts existing. Uses reserved verification fields established in Phase 1 schema, so no schema redesign. Can be built in parallel with Features 12/13/14 or before them. Positioned first in Epic 3 because it's the foundational trust primitive — partner APIs and compliance profiles are higher-value when receipts are verifiable.

**What It Depends On**:
- Real-Time Square Transaction Ingestion (Feature 3 — receipts exist to sign)
- Extensible Receipt Envelope (Feature 2 — verification signs core+envelope)

**What Depends On It**:
- None directly, but Partner Integration APIs and Expanded Compliance Profiles are more valuable with verified receipts

**What It Exposes/Unlocks**:
- Signing service: compute hash/signature over canonical receipt fields at issuance
- Verification endpoint: recompute and compare, return `valid` / `invalid` / `unverifiable`
- Phased per-merchant/per-region enablement
- Verification fields populated: signature/hash value, algorithm, canonicalization version

**Build Size**: **M** — Canonical serialization routine (deterministic, versioned), signature computation (hash or HMAC/asymmetric), verification logic, phased-enablement config, signature storage, verification endpoint

**Infra/Foundation Work Alongside**:
- EF Core migration for ReceiptSignature entity or signature fields on Receipt entity
- Fields: ReceiptId (FK), AlgorithmIdentifier, SignatureValue, CanonicalizationVersion, SignedAtUtc
- Canonical serialization version 1 specification (stable field order, normalized types)
- Signing key/material management (service-held key for MVP)

---

### **Feature 12: Expanded Compliance Profiles**

**Epic**: Trust, Compliance & Partner Ecosystem

**Why Here**: Depends only on receipts existing (Feature 3). Independent of verification, partner APIs, and additional POS providers. Can be built in parallel with Feature 11 or after it. Positioned before Additional POS Provider Support because new POS providers will need compliance evaluation at ingestion.

**What It Depends On**:
- Real-Time Square Transaction Ingestion (Feature 3 — receipts exist to evaluate)
- Merchant Onboarding (Feature 1 — compliance profiles attached to merchants)

**What Depends On It**:
- Additional POS Provider Support (Feature 14 — adapter-normalized receipts are evaluated against active profile)

**What It Exposes/Unlocks**:
- Compliance profile definition, versioning, activation per merchant/region
- Compliance evaluation at receipt issuance: `compliant` / `missing-fields` / `not-evaluated`
- Evaluation does not block claimability (additive metadata only)
- On-demand re-evaluation of existing receipts against newer profile versions

**Build Size**: **M** — Compliance profile entity, profile CRUD endpoints, evaluation logic (required-field checks, format validation), evaluation recording (profile id+version per receipt), integration into ingestion pipeline, frontend profile config screen

**Infra/Foundation Work Alongside**:
- EF Core migrations for ComplianceProfile, ComplianceEvaluation entities
- ComplianceProfile: Id, RegionLabel, Version, RuleDefinition (JSONB: required fields, format rules), ActiveFlag
- ComplianceEvaluation: Id, ReceiptId (FK), ProfileId (FK), ProfileVersion, Status (enum), EvaluatedAtUtc
- Evaluation logic callable from ingestion pipeline (asynchronous if needed to preserve latency budget)

---

### **Feature 13: Partner Integration APIs**

**Epic**: Trust, Compliance & Partner Ecosystem

**Why Here**: Extends Epic 2's consent model (Feature 9) to programmatic partner access. Must come after consent. Independent of verification and compliance profiles (but more valuable with them). Can be built in parallel with Features 11/12 or after them. Positioned before Additional POS Provider Support because partner APIs are a higher-value unlock.

**What It Depends On**:
- Consent & Contact Opt-In (Feature 9 — extends consent model to partner access grants)
- Anonymous History Baseline Access (Feature 5 — partner APIs consume history data)
- Extensible Receipt Envelope (Feature 2 — partner APIs scope envelope module access)

**What Depends On It**:
- None directly

**What It Exposes/Unlocks**:
- Partner client registration with scoped credentials (data type + merchant coverage)
- Scoped, time-bound bearer tokens (JWT-style, signed in-process)
- Customer-authorized access grant approval, viewing, revocation
- Partner API Gateway: enforces token scope + active grant on every call
- Partner API audit log (partner, scope, subject, outcome, timestamp)

**Build Size**: **L** — Partner client registry, token issuance/validation, access grant entity, grant approval/list/revoke endpoints, Partner API Gateway middleware (scope+grant enforcement), audit logging, data minimization (only in-scope fields returned), frontend partner credential/grant management screen

**Infra/Foundation Work Alongside**:
- EF Core migrations for PartnerClient, PartnerAccessGrant, PartnerAuditLog entities
- PartnerClient: Id, Name, AllowedScopes (JSONB or enum), Status (active/revoked), CreatedAtUtc
- PartnerAccessGrant: Id, CustomerStubId, PartnerClientId (FK), MerchantScope, DataTypeScope, GrantedAtUtc, ExpiresAtUtc, RevokedAtUtc (nullable)
- PartnerAuditLog: Id, PartnerClientId, GrantId, Endpoint, Subject, Outcome, Timestamp
- Token issuance/validation logic (signed, short-lived tokens with embedded client id + scope)
- Partner API Gateway middleware (validates token signature/expiry + checks active grant before every partner-facing endpoint)

---

### **Feature 14: Additional POS Provider Support**

**Epic**: Trust, Compliance & Partner Ecosystem

**Why Here**: Depends on ingestion (Feature 3) being established, and should come after compliance profiles (Feature 12) so new providers' receipts are evaluated at ingestion. Can be built after or in parallel with Feature 11/13. Positioned last in Epic 3 because it's an expansion feature (new merchant segment) rather than a foundational trust/compliance/integration primitive.

**What It Depends On**:
- Real-Time Square Transaction Ingestion (Feature 3 — refactors into adapter abstraction)
- Merchant Onboarding (Feature 1 — merchant POS provider configuration)
- Expanded Compliance Profiles (Feature 12 — adapter-normalized receipts evaluated against profiles)

**What Depends On It**:
- None directly

**What It Exposes/Unlocks**:
- `IPosAdapter` interface: provider-specific payload in, core receipt/envelope shape out
- Square ingestion refactored as SquareAdapter (reference implementation)
- At least one additional adapter (e.g., for Clover, Toast, or generic REST shape)
- Adapter routing by merchant POS provider configuration
- Normalized payloads flow through same validation/idempotency/claimability/compliance pipeline

**Build Size**: **M** — IPosAdapter interface definition, SquareAdapter refactor (existing ingestion logic, zero behavior change), at least one additional adapter implementation, adapter registry/dispatch logic, merchant POS provider config field, adapter validation tests

**Infra/Foundation Work Alongside**:
- Update Merchant entity with PosProviderType field (if not already present)
- Adapter registry (maps PosProviderType → IPosAdapter implementation)
- Additional adapter payload validation specific to that provider's shape
- Documentation of IPosAdapter contract (input: provider payload, output: normalized core receipt/envelope ingestion DTO)

---

## Epic 3 Complete ✓

At this point, the platform has:
- Cryptographic verification of receipt authenticity (phased rollout)
- Scoped, consent-aware, audited partner integration APIs
- Configurable, versioned compliance profiles (no schema change per region)
- Multi-POS provider support via adapter abstraction

**Trust established, partner ecosystem enabled, compliance scalable, addressable market expanded.**

---

## Full Platform Implementation Complete

All 14 features across 3 epics have been sequenced and implemented in dependency order:

**Epic 1 (Features 1–6)**: Core platform — onboarding, ingestion, claim, history, branding, extensible envelope foundation
**Epic 2 (Features 7–10)**: Security & engagement — warranty/loyalty modules, consent lifecycle, hardened anonymous access
**Epic 3 (Features 11–14)**: Trust & ecosystem — cryptographic verification, compliance profiles, partner APIs, multi-POS support

**The platform is now production-ready for:**
- Paperless checkout with instant receipt claim
- Privacy-preserving, consent-managed customer engagement
- Trusted, auditable receipt sharing with partners
- Compliant, multi-region, multi-POS merchant onboarding

---

## Dependency Graph Summary

### Features with No Hard Dependencies (Build #1 Candidates)
- Feature 1: Merchant Onboarding & Square Connect
- Feature 2: Extensible Receipt Envelope (Foundation)

### Dependency Tiers

**Tier 1** (foundation):
- Feature 1: Merchant Onboarding
- Feature 2: Extensible Receipt Envelope

**Tier 2** (core value delivery):
- Feature 3: Real-Time Square Transaction Ingestion (depends on 1, 2)

**Tier 3** (customer-facing core):
- Feature 4: Checkout Claim (depends on 3)
- Feature 5: Anonymous History Baseline Access (depends on 3, 4)
- Feature 6: Receipt Branding Customization (depends on 1)

**Tier 4** (engagement modules, prove envelope):
- Feature 7: Warranty Module Activation (depends on 2, 3)
- Feature 8: Loyalty Module Activation (depends on 2, 3)
- Feature 9: Consent & Contact Opt-In (depends on 1, soft: 5)

**Tier 5** (security hardening):
- Feature 10: Hardened Anonymous Access & Challenge Flows (depends on 5, 9)

**Tier 6** (trust & ecosystem, all parallel-capable):
- Feature 11: Cryptographic Receipt Verification (depends on 2, 3)
- Feature 12: Expanded Compliance Profiles (depends on 1, 3)
- Feature 13: Partner Integration APIs (depends on 2, 5, 9)

**Tier 7** (multi-POS expansion):
- Feature 14: Additional POS Provider Support (depends on 1, 3, 12)

### Circular Dependencies
**None detected.** All features have clear, acyclic dependency chains.

### Gaps/Inconsistencies
**None detected.** Every referenced dependency (merchant records, receipts, consent, envelope, etc.) has a corresponding feature that establishes it earlier in the sequence.

---

## Notes on Soft Dependencies

- **Receipt Branding (Feature 6)** is technically independent of ingestion/claim/history, but its value is only realized when customers retrieve branded receipts, so it's positioned after Feature 5 in the sequence even though it could be built earlier.
- **Loyalty Module (Feature 8)** could be built before or in parallel with Warranty Module (Feature 7) — they are independent. Warranty is sequenced first as the simpler of the two to validate the pattern.
- **Consent (Feature 9)** is a soft dependency for Challenge Flows (Feature 10) because OTP delivery may use a consented contact channel, but a log-based notification implementation allows Feature 10 to be tested without real contact delivery.
- **Cryptographic Verification (Feature 11)** and **Compliance Profiles (Feature 12)** and **Partner Integration APIs (Feature 13)** are all independent and can be built in parallel, but the sequence orders them by: (1) trust foundation first, (2) compliance second (needed for POS expansion), (3) partner APIs third (extends Epic 2's consent model).

---

## T-Shirt Size Summary

| Feature | Size | Epic |
|---------|------|------|
| 1. Merchant Onboarding & Square Connect | S | 1 |
| 2. Extensible Receipt Envelope (Foundation) | S | 1 |
| 3. Real-Time Square Transaction Ingestion | M | 1 |
| 4. Checkout Claim | S | 1 |
| 5. Anonymous History Baseline Access | S | 1 |
| 6. Receipt Branding Customization | S | 1 |
| 7. Warranty Module Activation | S | 2 |
| 8. Loyalty Module Activation | S | 2 |
| 9. Consent & Contact Opt-In | M | 2 |
| 10. Hardened Anonymous Access & Challenge Flows | M | 2 |
| 11. Cryptographic Receipt Verification | M | 3 |
| 12. Expanded Compliance Profiles | M | 3 |
| 13. Partner Integration APIs | L | 3 |
| 14. Additional POS Provider Support | M | 3 |

**Total Effort**: 8 Small + 5 Medium + 1 Large = ~14–18 weeks at 1–2 weeks per Small, 2–3 weeks per Medium, 3–4 weeks per Large

---

## Final Implementation Notes

1. **Follow the sequence strictly** — each feature's acceptance criteria should be met and validated before moving to the next.
2. **Epic boundaries are documentation groupings, not build gates** — Epic 2 features depend on Epic 1 being complete, but within an epic, follow the numbered sequence, not the epic label.
3. **Foundational infra first** — before Feature 1, ensure the baseline API/DB/frontend scaffold, structured logging, and rate-limiting abstraction are in place.
4. **Test in production-like conditions** — ingest-to-claimable latency (Feature 3), rate-limiting effectiveness (Features 5, 10), and challenge pass rates (Feature 10) must be measured under realistic load before declaring the platform pilot-ready.
5. **Envelope validation happens early** — Features 7/8 prove the envelope pattern works; any issues discovered there may require revisiting Feature 2's validation logic before continuing to Epic 3.

**Build, test, validate, iterate — one feature at a time, in this order.**
