# Feature PRD: Consent & Contact Opt-In

## 1. Feature Name

Consent & Contact Opt-In — Grant, View, and Revoke Lifecycle for Customer Contact Sharing

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Anonymous Access Hardening & Consent Lifecycle
- Architecture Spec: [arch.md](../arch.md) — see "Consent Management Service" in the System Architecture Diagram (Service Layer)

## 3. Goal

### Problem

The platform currently has no supported way for a customer to voluntarily share an email or phone number with a merchant, and no mechanism to track why that data was shared, when, or whether the customer later withdrew permission. Without this, merchants cannot legally or safely use contact information for support, loyalty, or marketing purposes, and the platform cannot demonstrate compliance-readiness for consent handling — a requirement the PRD calls out explicitly (sections 8.6 and 12.3).

### Solution

Introduce a first-class consent record: a customer can grant a merchant permission to use a specific contact channel (email or phone) for a specific, named purpose (e.g., "receipt delivery," "loyalty updates," "marketing"). Every grant is versioned and timestamped. Customers can list their active grants for a merchant and revoke any of them, with revocation taking effect immediately and permanently (a new grant, not an un-revoke, is required to restore access). Any future code path that wants to use a contact channel must check for an active, matching consent record first and fail deterministically if none exists.

### Impact

- Gives merchants a lawful, auditable basis to use customer contact information, unlocking support/loyalty/marketing use cases the PRD identifies as future value drivers.
- Gives customers clear, revocable control over their own contact data, directly satisfying the privacy/compliance requirements in PRD sections 8.6 and 12.3.
- Produces the audit trail ("what was granted, when, by what version, and when revoked") needed for compliance/legal review before wider rollout.

## 4. User Personas

- **Customer (Opt-In)**: Wants to choose whether to share an email/phone with a specific merchant, for a specific reason, and wants to be able to revoke that later.
- **Merchant Admin**: Wants to know which customers have consented to be contacted, and for what purpose, before using a contact channel.
- **Compliance/Legal Stakeholder**: Needs to verify, for any customer/merchant pair, exactly what was consented to and whether it is still active.

## 5. User Stories

- As a **Customer**, I want to grant a merchant permission to use my email for a specific purpose (e.g., receipt delivery), so that I control exactly what my contact information is used for.
- As a **Customer**, I want to view all my active consent grants for a merchant, so that I know what I've agreed to.
- As a **Customer**, I want to revoke a consent grant at any time, so that the merchant immediately loses permission to use that contact channel for that purpose.
- As a **Merchant Admin**, I want the system to prevent me from using a customer's contact channel for a purpose they haven't consented to (or have revoked), so that I don't inadvertently violate the customer's stated preference.
- As a **Compliance Stakeholder**, I want every grant and revocation permanently recorded with purpose, version, and timestamp, so that I can answer audit questions about consent history.

## 6. Requirements

### Functional Requirements

- The system must expose an endpoint to grant consent, accepting: customer stub identifier reference, merchant id, contact channel (email or phone), contact value, purpose, and consent version.
- The system must expose an endpoint to list a customer's active (non-revoked) consent grants, optionally filtered by merchant.
- The system must expose an endpoint to revoke a specific consent grant by its identifier, recording a revoked-at timestamp.
- Each consent record must persist: customer stub identifier reference, merchant id, contact channel type, contact value, purpose, consent version, granted-at timestamp, and revoked-at timestamp (nullable, null while active).
- The system must enforce a uniqueness rule such that only one active (non-revoked) grant may exist for a given `(customer, merchant, purpose, channel)` combination; granting again while one is active must either update the existing record's version/timestamp or require revocation first (single deterministic behavior, chosen and documented consistently).
- The system must provide a shared enforcement check (usable by any future contact-consuming code path) that returns whether an active consent exists for a given `(customer, merchant, purpose, channel)` combination, and must reject use with a deterministic error when no active consent exists.
- Contact values (email/phone) must be stored separately from the core anonymous receipt/customer data model, and must never be included in the Phase 1 baseline receipt or history responses.
- Revocation must be immediate: any enforcement check performed after a revoked-at timestamp is set must report no active consent for that record.
- The system must reject grant requests with a missing/empty contact value, an unrecognized channel type, or an empty purpose, returning a deterministic validation error.

### Non-Functional Requirements

- **Privacy & Data Segregation**: Contact data must be stored in a dedicated consent/contact table, never merged into the core receipt or anonymous customer identifier data structures.
- **Auditability**: Grant and revocation events must be permanently retained (revocation must not delete the record — it must mark it revoked) so the full consent history remains reconstructable.
- **Consistency**: Consent state must be strongly consistent — a revocation must be immediately visible to the enforcement check with no caching-induced staleness window.
- **Security**: Endpoints that grant/revoke/list consent must validate that the caller is acting on their own stub identifier's consent records (no cross-customer consent tampering).
- **Compliance-Readiness**: The consent version field must allow future changes to a purpose's terms (e.g., an updated marketing disclosure) to be tracked distinctly from earlier grants under a prior version.
- **Extensibility**: The consent model must support adding new purposes or channel types later without a breaking schema change.

## 7. Acceptance Criteria

**Story: Grant consent**

- [ ] Given a customer supplies a valid contact channel, value, purpose, and version, When consent is granted, Then a consent record is created with a granted-at timestamp and no revoked-at value.
- [ ] Given a grant request with a missing contact value or empty purpose, When submitted, Then the response is a deterministic validation error and no record is created.

**Story: List active consent grants**

- [ ] Given a customer has one active and one revoked grant for a merchant, When active grants are listed, Then only the active grant is returned.

**Story: Revoke consent**

- [ ] Given an active consent grant, When the customer revokes it, Then the record's revoked-at timestamp is set and it no longer appears in the active-grants list.
- [ ] Given a revoked grant, When an enforcement check is performed for that channel/purpose, Then the check reports no active consent.

**Story: Enforcement blocks unconsented use**

- [ ] Given no active consent record exists for a given customer/merchant/purpose/channel, When a contact-channel-consuming action checks for consent, Then the check reports no active consent and the action is rejected deterministically.
- [ ] Given an active consent record exists, When the enforcement check is performed for the matching purpose/channel, Then the check reports active consent and the action is permitted to proceed.

**Story: Data segregation**

- [ ] Given a customer has granted consent with a contact value, When the Phase 1 baseline receipt or history response is generated, Then the contact value does not appear in that response.

## 8. Out of Scope

- Actual marketing/notification send mechanics that would consume a granted consent (this feature only guarantees consent is capturable, auditable, and enforceable — not that campaign sends are built).
- Full customer account/profile management (consent is tracked against the anonymous stub identifier, not a full account).
- Contact value verification (e.g., confirming an email address is deliverable or a phone number is valid beyond basic format checks) beyond what's needed to accept a grant.
- Multi-jurisdiction consent legal-basis modeling (e.g., GDPR-specific lawful-basis categorization) — this feature provides the mechanical grant/revoke/audit primitives; jurisdiction-specific legal categorization is a future compliance-pack concern per PRD Phase 3.
- UI polish for the consent management screen beyond a functional grant/list/revoke interface.
