# Feature PRD: Cryptographic Receipt Verification

## 1. Feature Name

Cryptographic Receipt Verification — Phased Signing and Verification of Core Receipt Data

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Trust, Compliance & Partner Ecosystem
- Architecture Spec: [arch.md](../arch.md) — see "Receipt Signing & Verification Service" in the System Architecture Diagram

## 3. Goal

### Problem

Receipts issued today have no cryptographic proof of authenticity. If a receipt is disputed, screenshotted, or altered, there is no deterministic way to confirm the data shown matches what Stub originally recorded. The Phase 1 schema reserved fields for this purpose specifically so this capability could be added later without a redesign, but that capability does not yet exist.

### Solution

Add a Receipt Signing & Verification Service that computes a deterministic hash/signature over a canonical serialization of a receipt's core fields, stores the signature reference in the already-reserved verification fields, and exposes a verification action that recomputes and compares the signature against current data, returning `valid`, `invalid`, or `unverifiable`. Activation is phased per merchant/region so rollout does not require touching previously issued receipts.

### Impact

- Gives merchants and customers a deterministic way to confirm receipt authenticity in disputes or audits.
- Delivers on the Phase 1 architectural commitment to support verification without schema redesign.
- Establishes the foundation compliance and partner-sharing features can build on for higher-trust data exchange.

## 4. User Personas

- **Merchant Admin**: Wants to be able to prove a receipt from their store is authentic and unaltered.
- **Customer (No Signup / Opt-In)**: Wants confidence that a receipt shown as theirs is genuine.
- **Compliance/Legal**: Needs an auditable mechanism to support authenticity claims during regulatory review or dispute resolution.

## 5. User Stories

- As a **Merchant Admin**, I want new receipts from my store to be automatically signed, so that I can later prove they haven't been tampered with.
- As a **Customer**, I want to check whether a receipt I'm viewing is verified, so that I can trust the data is genuine.
- As a **Compliance/Legal** stakeholder, I want verification to be enabled per merchant/region on a rolling basis, so that we can pilot the capability without forcing every merchant to adopt it at once.
- As a **Merchant Admin**, I want receipts issued before verification was enabled to be clearly distinguishable from verified ones, so that I don't mistakenly treat them as tamper-proof.

## 6. Requirements

### Functional Requirements

- The system must define a canonical serialization of a receipt's core fields (stable field order, normalized types, explicit version identifier) used as the signing/verification input.
- The system must compute a cryptographic hash and/or digital signature over the canonical serialization at or immediately after receipt issuance, for merchants/regions where verification is enabled.
- The system must store the resulting signature/hash value, algorithm identifier, and canonicalization version in the verification fields already reserved in the core receipt schema.
- The system must expose a verification action that recomputes the canonical serialization and signature for a given receipt and compares it against the stored value.
- The verification action must return exactly one of: `valid` (signature matches current data), `invalid` (signature present but does not match), or `unverifiable` (no signature stored, e.g., receipt predates verification enablement).
- The system must support enabling verification independently per merchant or per region, without requiring changes to receipts issued before enablement.
- The system must not modify the core receipt or envelope schema to implement this feature; it must only populate previously reserved fields.
- The system must record which canonicalization/algorithm version was used per signature, so future algorithm changes don't invalidate the ability to verify older receipts signed under a prior version.

### Non-Functional Requirements

- **Performance**: Signature computation at issuance must not materially increase ingest-to-claimable latency established in Phase 1; if computation cannot complete synchronously within budget, the receipt must still become claimable and verification must complete asynchronously with a deterministic pending/unverifiable interim state.
- **Security**: Signing keys/material must not be exposed through any API response; only the resulting signature/hash reference is externally visible.
- **Determinism**: The same receipt data and canonicalization version must always produce the same verification result; verification must not depend on external state beyond the receipt's own stored fields.
- **Backward Compatibility**: Enabling verification for a merchant/region must not change existing Phase 1/Phase 2 endpoint behavior or response shape for merchants not opted in.

## 7. Acceptance Criteria

**Story: Receipt is signed at issuance for an enabled merchant**

- [ ] Given a merchant with verification enabled, When a new receipt is ingested, Then a signature/hash is computed and stored in the reserved verification fields alongside the algorithm and canonicalization version.

**Story: Verification succeeds for an unaltered receipt**

- [ ] Given a signed receipt whose core fields have not changed since signing, When verification is requested, Then the result is `valid`.

**Story: Verification fails for an altered receipt**

- [ ] Given a signed receipt whose stored signature no longer matches its current core field values, When verification is requested, Then the result is `invalid`.

**Story: Verification on a pre-rollout receipt**

- [ ] Given a receipt issued before verification was enabled for its merchant, When verification is requested, Then the result is `unverifiable` and no error is raised.

**Story: Phased enablement does not affect other merchants**

- [ ] Given verification is enabled for Merchant A but not Merchant B, When a new receipt is issued for Merchant B, Then no signature is computed and existing Merchant B endpoint behavior is unchanged.

## 8. Out of Scope

- Legal certification or jurisdiction-specific government digital signature standards.
- Backfilling signatures for all historical Phase 1/Phase 2 receipts as part of this feature (may be a separate, explicit effort).
- Customer- or merchant-facing UI/UX polish for displaying verification badges (this feature covers the signing/verification mechanism and API, not final presentation design).
- Key rotation/management tooling beyond recording the algorithm/canonicalization version used per signature.
