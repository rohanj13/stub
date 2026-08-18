# Feature PRD: Extensible Receipt Envelope (Foundation)

## 1. Feature Name

Extensible Receipt Envelope — Core/Extension Schema Foundation

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "envelope column (JSONB)" technical enabler

## 3. Goal

### Problem

Every future platform capability — loyalty metadata, ad placements, warranty registration, downstream integration references — needs somewhere to live on the receipt. If these are bolted onto the core receipt schema as they arrive, each addition risks breaking existing consumers and mixing optional, policy-gated data with compliance-critical core fields (Risk 4 in the PRD: extensibility debt).

### Solution

Introduce a versioned, optional "envelope" container alongside the core receipt object now, before any concrete module (loyalty/ads/warranty/integrations) is built. The envelope is additive-only: its presence, absence, or content must never affect core receipt validity, required compliance fields, or existing API consumers.

### Impact

- Prevents expensive schema rework when loyalty/ads/warranty/integration features are built in later phases.
- Gives engineering a clear, enforced boundary between "must always be correct" (core) and "optional, policy-gated" (envelope) data.

## 4. User Personas

- **Backend Engineering**: Implements and enforces the core/envelope boundary.
- **Third-Party Integrator** (future-facing): Will eventually read/write scoped envelope modules.

## 5. User Stories

- As a **Backend Engineer**, I want a defined envelope container on the receipt entity, so that future modules have a clear, versioned place to live without touching the core schema.
- As a **Backend Engineer**, I want the envelope to be schema-versioned, so that older and newer envelope shapes can coexist during rollout of new modules.
- As a **Platform Operator**, I want confidence that removing or corrupting envelope data can never invalidate a receipt's core compliance fields, so that the platform stays audit-safe.

## 6. Requirements

### Functional Requirements

- The receipt entity must include an `Envelope` field stored as a structured, versioned JSON document (e.g., `{ "version": 1, "modules": { ... } }`), separate from core columns.
- The system must accept receipt creation/updates with no envelope present (envelope is fully optional) and treat that as valid.
- The system must not allow envelope content to overwrite or shadow any core receipt field (id, merchant reference, transaction reference, totals, currency, timestamps, customer linkage, line items).
- The system must expose the envelope as part of the receipt retrieval response, structured so unknown/absent modules simply don't appear (no null-shaped placeholders required).
- The system must validate the envelope's top-level shape (must be a JSON object with a `version` field) if present, rejecting malformed envelope content with a deterministic error.
- The system must document the reserved module keys for future features (`loyalty`, `ads`, `warranty`, `integrations`) even though no module logic is implemented yet in this feature.

### Non-Functional Requirements

- **Data Integrity**: Envelope storage must be isolated (e.g., a distinct JSONB column) so a query or migration touching the envelope cannot accidentally alter core fields.
- **Extensibility**: Adding a new module key to the envelope must require no migration to existing receipts and no change to core API contracts.
- **Compatibility**: Existing API consumers that ignore the envelope field entirely must continue to function unchanged.

## 7. Acceptance Criteria

**Story: Envelope is optional**

- [ ] Given a receipt created without envelope content, When it is retrieved, Then the response is valid and simply omits/empties the envelope field.

**Story: Envelope does not affect core validity**

- [ ] Given a receipt with arbitrary (valid-shaped) envelope content, When core fields are read, Then they are unaffected by envelope content.
- [ ] Given malformed envelope content (not a JSON object, or missing `version`), When a receipt is created/updated with it, Then the request is rejected with a deterministic error and the core receipt is not persisted in a partial state.

**Story: Envelope is versioned and extensible**

- [ ] Given an envelope with `version: 1`, When a new module key is introduced in a later feature, Then no migration is required for existing receipts lacking that key.

## 8. Out of Scope

- Implementation of any concrete module (loyalty, ads, warranty, integrations) — this feature only establishes the container and validation rules.
- Envelope module-level authorization/policy enforcement (e.g., which caller can write `ads` data) — later phase concern.
- UI for viewing/editing envelope content.
