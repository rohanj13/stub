# Feature PRD: Expanded Compliance Profiles

## 1. Feature Name

Expanded Compliance Profiles — Configurable, Versioned Regional Compliance Evaluation

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Trust, Compliance & Partner Ecosystem
- Architecture Spec: [arch.md](../arch.md) — see "Compliance Profile Service" in the System Architecture Diagram

## 3. Goal

### Problem

The receipt schema today implicitly assumes a single baseline compliance shape. Merchants operating in additional regions have different required fields, formatting rules, and retention expectations, but there is no way to define, version, or evaluate against those rules without a bespoke schema change per region — which is exactly the rework the Phase 1 envelope-first design was meant to avoid.

### Solution

Introduce a Compliance Profile service that lets Compliance/Legal define, version, and activate regional compliance rule sets (required fields, formatting rules, retention expectations) independent of the core receipt schema. Each receipt is evaluated against its merchant's active compliance profile at issuance, recording a deterministic status without blocking claimability, so merchants can be informed of gaps without disrupting the customer-facing experience.

### Impact

- Unlocks merchant onboarding in additional regions without repeated core schema/engineering work per region.
- Gives Compliance/Legal a self-contained way to define and evolve rules over time.
- Surfaces compliance gaps early (at issuance) rather than discovering them during an audit or dispute.

## 4. User Personas

- **Compliance/Legal**: Defines, versions, and activates regional compliance profiles.
- **Merchant Admin**: Operates in a specific region/profile and needs visibility into whether their receipts meet the active requirements.
- **Product Owner**: Needs confidence that expanding to a new region is an additive configuration change, not a re-architecture.

## 5. User Stories

- As **Compliance/Legal**, I want to define a new regional compliance profile with required fields and formatting rules, so that receipts in that region can be evaluated against it.
- As **Compliance/Legal**, I want to version a compliance profile, so that changing the rules doesn't retroactively invalidate receipts evaluated under a prior version.
- As a **Merchant Admin**, I want my receipts evaluated against my active compliance profile at issuance, so that I know immediately if required fields are missing.
- As a **Merchant Admin**, I want a compliance evaluation failure to not block a customer from claiming or viewing their receipt, so that compliance gaps don't degrade the checkout experience.
- As a **Product Owner**, I want to attach a new compliance profile to a merchant without a schema change, so that expanding to a new region does not require an engineering release per region.

## 6. Requirements

### Functional Requirements

- The system must support defining a compliance profile consisting of a region/label, a version, and a structured rule definition describing required fields, formatting rules, and retention expectations.
- The system must support activating a compliance profile for a given merchant or region, and support changing the active profile going forward.
- The system must not require a core receipt/envelope schema change to define or activate a new compliance profile.
- The system must evaluate each newly issued receipt against its merchant's active compliance profile and record a deterministic status: `compliant`, `missing-fields`, or `not-evaluated` (e.g., no active profile configured).
- The system must record which profile id and version was used for a given evaluation, so historical evaluations remain interpretable after the active profile changes.
- Compliance evaluation must not block or delay a receipt's transition to `claimable`; evaluation results must be additive metadata, not a gate on the existing claim flow.
- The system must allow re-evaluating a previously issued receipt against a newer profile version on demand, without altering the receipt's original evaluation record.
- The system must expose an operator-facing way (via API/frontend) to view a merchant's active profile and the compliance status distribution of its receipts.

### Non-Functional Requirements

- **Extensibility**: Adding a new compliance profile must be achievable as a data/configuration change, not a code or schema change.
- **Auditability**: Every compliance evaluation must be recorded with enough detail (profile id, version, status, timestamp) to reconstruct why a receipt was flagged.
- **Performance**: Compliance evaluation at issuance must not materially increase ingest-to-claimable latency established in Phase 1.
- **Backward Compatibility**: Merchants without an active compliance profile must continue to behave exactly as before this feature (receipts recorded as `not-evaluated`, no behavior change to claim/history/envelope flows).

## 7. Acceptance Criteria

**Story: Define and activate a new compliance profile**

- [ ] Given Compliance/Legal defines a new profile with required fields for a region, When the profile is activated for a merchant, Then subsequently issued receipts for that merchant are evaluated against it.

**Story: Receipt evaluated as compliant**

- [ ] Given a merchant's active profile requires a specific set of fields, When a receipt is issued containing all required fields in the correct format, Then the evaluation status is `compliant`.

**Story: Receipt evaluated as missing fields**

- [ ] Given a merchant's active profile requires a field the receipt does not contain, When the receipt is issued, Then the evaluation status is `missing-fields`, and the receipt still becomes `claimable` normally.

**Story: No active profile**

- [ ] Given a merchant has no active compliance profile configured, When a receipt is issued, Then the evaluation status is `not-evaluated` and no other behavior changes.

**Story: Profile version change does not retroactively affect prior evaluations**

- [ ] Given a merchant's profile is updated to a new version, When a previously evaluated receipt's evaluation record is inspected, Then it still reflects the profile version active at the time it was evaluated.

**Story: On-demand re-evaluation**

- [ ] Given a receipt was evaluated under an older profile version, When an operator triggers re-evaluation against the current active profile, Then a new evaluation record is created reflecting the current profile version, without overwriting the original.

## 8. Out of Scope

- A non-technical rule-authoring UI for compliance staff (profiles may be defined via configuration/data in this feature).
- Automated legal risk scoring, advisory recommendations, or jurisdiction-specific legal certification.
- Blocking claimability or customer-facing access based on compliance status (compliance evaluation is informational metadata in this feature).
- Retroactive bulk re-evaluation of all historical receipts when a new profile is activated (on-demand re-evaluation of individual receipts is in scope; bulk backfill is not).
