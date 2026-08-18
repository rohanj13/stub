# Feature PRD: Anonymous History Baseline Access

## 1. Feature Name

Anonymous History Baseline Access — Possession-Aware Receipt History Retrieval

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "Anonymous History Access Service" in the System Architecture Diagram

## 3. Goal

### Problem

Customers must be able to view their receipt history using only their stub identifier — no account, password, or signup. Today `GET /api/customers/{customerId}/receipts` returns history keyed purely on that identifier string, which means anyone who obtains or guesses a customer's stub id can view their full spending history. The PRD explicitly calls out this exact risk (Risk 2) as unacceptable even at MVP scale.

### Solution

Add baseline possession/risk controls around history retrieval: rate limiting per identifier, abuse-detection logging, and audit trails for every retrieval attempt, so that even without full account authentication, casual or automated abuse of a leaked/guessed identifier is detectable and throttled. This is the MVP baseline; hardened step-up challenge flows are explicitly a Phase 2 feature.

### Impact

- Directly mitigates the platform's highest-severity MVP privacy risk.
- Produces the audit trail needed to detect and respond to abuse during pilot.
- Establishes the extension point that Phase 2's hardened challenge flow will build on without an API break.

## 4. User Personas

- **Customer (No Signup)**: Wants to view their own past receipts safely.
- **Security/Privacy stakeholder**: Needs confidence that anonymous access can't be trivially abused during pilot.

## 5. User Stories

- As a **Customer**, I want to retrieve my receipt history using my stub identifier, so that I can view past purchases without creating an account.
- As a **Security stakeholder**, I want repeated or high-volume retrieval attempts against a single identifier to be rate-limited, so that scraping or brute-force enumeration is slowed and detectable.
- As a **Security stakeholder**, I want every history retrieval attempt logged (identifier, timestamp, outcome, caller context), so that abuse can be investigated after the fact.

## 6. Requirements

### Functional Requirements

- The system must return receipt history for a given customer stub identifier, ordered by most recent first.
- The system must return `400 Bad Request` for an empty/whitespace identifier.
- The system must apply a rate limit per stub identifier (and, where available, per caller IP) to the history endpoint, returning `429 Too Many Requests` once the limit is exceeded within a rolling window.
- The system must log every retrieval attempt with identifier, timestamp, outcome (success/rate-limited/error), and result count, without logging full receipt contents.
- The system must support configurable data minimization for anonymous views — at minimum, exclude any future consent-only fields (email/phone) from this endpoint's response by default.
- Rate-limit and audit-log configuration (window size, threshold) must be externally configurable without a code change.

### Non-Functional Requirements

- **Security & Privacy**: No endpoint may expose another customer's history through this identifier lookup; only exact-match retrieval is permitted (no partial/fuzzy identifier matching).
- **Performance**: Baseline rate-limit checks must add negligible latency (in-process counter acceptable for MVP; must be swappable for a distributed store like Redis later).
- **Auditability**: Audit log entries must be retained long enough to support incident investigation (retention policy to be defined operationally, not hardcoded to a trivial short window).
- **Extensibility**: The endpoint contract must not preclude adding a step-up challenge (e.g., a secondary proof) in Phase 2 without a breaking change for existing callers.

## 7. Acceptance Criteria

**Story: Retrieve own history**
- [ ] Given a valid stub identifier with existing receipts, When history is requested, Then the receipts are returned ordered by most recent first.
- [ ] Given a valid stub identifier with no receipts, When history is requested, Then an empty list is returned (not an error).

**Story: Reject invalid identifier**
- [ ] Given an empty or whitespace identifier, When history is requested, Then the response is `400 Bad Request`.

**Story: Rate limiting**
- [ ] Given more than the configured threshold of requests for the same identifier within the configured window, When an additional request is made, Then the response is `429 Too Many Requests`.
- [ ] Given a rate-limited request, When it occurs, Then it is logged as "rate-limited" in the audit trail.

**Story: Audit logging**
- [ ] Given any retrieval attempt (successful, rate-limited, or invalid), When it completes, Then an audit log entry is recorded with identifier, timestamp, and outcome, and without exposing full receipt contents in the log.

## 8. Out of Scope

- Step-up/challenge-based verification (e.g., last-4-digits, OTP, device binding) — Phase 2 feature.
- Consent-based contact field exposure/management — separate feature.
- Distributed rate-limiting infrastructure (Redis) — MVP may use an in-process store; this feature only requires the abstraction to support swapping later.
- Automated anomaly-detection alerting on top of the audit log (this feature only guarantees the log exists).
