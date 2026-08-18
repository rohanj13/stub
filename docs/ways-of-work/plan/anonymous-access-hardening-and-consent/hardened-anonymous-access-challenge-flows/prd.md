# Feature PRD: Hardened Anonymous Access & Challenge Flows

## 1. Feature Name

Hardened Anonymous Access & Challenge Flows — Possession-Based Step-Up Verification for Full History

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Anonymous Access Hardening & Consent Lifecycle
- Architecture Spec: [arch.md](../arch.md) — see "Challenge / Step-Up Verification Service" in the System Architecture Diagram (Service Layer)

## 3. Goal

### Problem

Phase 1's anonymous history access is protected only by rate limiting and audit logging around a stub identifier. That identifier is a single static value — anyone who obtains it (via a leaked screenshot, shoulder-surfing a QR code, or log exposure) can retrieve a customer's full purchase history immediately, and rate limiting only slows this, it doesn't prevent it. This is the platform's highest-severity remaining privacy risk called out in the PRD (Risk 2) and explicitly deferred to Phase 2.

### Solution

Introduce a tiered response model for history requests: a minimal baseline view is returned immediately (no challenge), and a full-history view requires the caller to pass a possession-based challenge first — proving they have more than just the identifier (e.g., matching a recent transaction's total, or verifying a short-lived one-time code delivered through a channel tied to a prior consented interaction). Every challenge issuance and verification attempt is logged and independently rate-limited from the Phase 1 history endpoint's own limiter.

### Impact

- Closes the platform's highest-priority privacy gap by requiring genuine possession evidence, not just a static identifier, for full history disclosure.
- Provides a measurable, auditable signal (challenge pass/fail rates) that security/privacy stakeholders can use to evaluate pilot readiness.
- Establishes an extension point for future challenge types (e.g., device binding) without breaking existing callers, per the Phase 1 extensibility requirement.

## 4. User Personas

- **Customer (No Signup)**: Wants to view their full purchase history but expects that a leaked identifier alone shouldn't be enough for someone else to see it.
- **Security/Privacy Stakeholder**: Needs proof that full history access requires more than identifier possession, with an audit trail to investigate abuse.
- **Platform Operator** (internal): Monitors challenge issuance/success/failure rates to detect abuse patterns or friction issues during pilot.

## 5. User Stories

- As a **Customer**, I want to see a minimal summary of my history without any extra step, so that I have some immediate confirmation my identifier works.
- As a **Customer**, I want to prove I made a specific purchase (e.g., confirm the last transaction total) to unlock my full history, so that my data stays protected even if my identifier leaks.
- As a **Security Stakeholder**, I want every challenge attempt (issued, succeeded, failed, expired) logged with outcome and timestamp, so that abuse patterns are investigable.
- As a **Platform Operator**, I want challenge issuance and verification independently rate-limited from the baseline history endpoint, so that challenge abuse (e.g., brute-forcing a one-time code) can't hide behind the Phase 1 limiter's budget.
- As a **Customer**, I want an expired or failed challenge to give a clear, non-revealing error, so that I understand I need to retry without learning anything that would help an attacker guess the proof.

## 6. Requirements

### Functional Requirements

- The history endpoint must return a minimal baseline view (e.g., count of receipts and most recent receipt date, no line-item detail) when no verified challenge is present for the request.
- The system must expose a challenge-issuance action for a given stub identifier that returns a challenge reference and, for time-boxed code challenges, triggers delivery via the pluggable notification interface.
- The system must support at least one possession-proof mechanism: verifying a submitted value (e.g., last transaction total or currency) against actual receipt data for that identifier, OR verifying a delivered one-time code.
- The system must expose a challenge-verification action that accepts the challenge reference and the customer's submitted proof, and returns either a verified session/token usable for the full-history call or a deterministic failure.
- One-time codes, when used, must expire within a short, configurable window (default target: a few minutes) and must be single-use — a verified or expired code must not be re-verifiable.
- The system must never persist a challenge secret (one-time code) in plaintext; only a salted hash may be stored for verification.
- The system must independently rate-limit challenge issuance and verification attempts per stub identifier, separate from the Phase 1 history rate limiter.
- The system must log every challenge attempt (issued, succeeded, failed, expired) with stub identifier, timestamp, challenge type, and outcome, without logging the raw proof value or one-time code.
- The full-history endpoint must reject requests lacking a valid, unexpired, unconsumed verified-challenge reference with a deterministic error (not silently falling back to baseline).

### Non-Functional Requirements

- **Security**: Challenge secrets must be stored only as salted hashes; verification must use constant-time comparison to avoid timing side-channels; failure responses must not reveal which part of a multi-field proof was incorrect.
- **Privacy**: Challenge audit logs must exclude the raw proof/one-time code value; only outcome metadata is retained.
- **Performance**: Baseline (unchallenged) history requests must retain the Phase 1 latency profile; challenge issuance/verification may add latency only to the full-history path, not the baseline path.
- **Reliability**: Expired or consumed challenges must be reliably rejected even under concurrent verification attempts (no double-use race condition).
- **Extensibility**: The challenge contract (issue → reference → verify) must support adding new challenge types later without changing the endpoint shape for existing callers.
- **Configurability**: Challenge expiry window and rate-limit thresholds must be externally configurable without a code change, consistent with the Phase 1 rate-limit configurability requirement.

## 7. Acceptance Criteria

**Story: Baseline view without challenge**
- [ ] Given a valid stub identifier, When history is requested without a verified challenge, Then a minimal baseline view (no line-item detail) is returned successfully.

**Story: Challenge issuance**
- [ ] Given a valid stub identifier, When a challenge is issued, Then a challenge reference is returned and, for code-based challenges, a one-time code is delivered via the notification interface and only its salted hash is persisted.
- [ ] Given a challenge issuance, When it is logged, Then the audit entry records identifier, timestamp, challenge type, and "issued" outcome without the raw code.

**Story: Successful verification unlocks full history**
- [ ] Given a valid, unexpired challenge and a correct proof, When verification is submitted, Then a verified reference/token is returned and the full-history endpoint returns complete receipt detail when that reference/token is presented.

**Story: Failed or expired verification is rejected**
- [ ] Given an incorrect proof, When verification is submitted, Then the response is a deterministic failure that does not reveal which field was wrong, and the attempt is logged as "failed."
- [ ] Given an expired or already-consumed challenge, When verification is attempted, Then the response is a deterministic failure and the attempt is logged as "expired" or rejected as already-used.

**Story: Independent rate limiting**
- [ ] Given repeated challenge issuance or verification attempts for the same identifier beyond the configured threshold, When an additional attempt is made, Then the response is `429 Too Many Requests`, independent of the Phase 1 history endpoint's own rate limit.

**Story: Full history requires a verified challenge**
- [ ] Given no verified challenge reference/token is presented, When full history is requested, Then the request is rejected with a deterministic error rather than returning full data or silently degrading to baseline without indication.

## 8. Out of Scope

- Device-binding, biometric, or multi-factor challenge types beyond the initial possession-proof/one-time-code mechanism.
- Building a production-grade SMS/email delivery integration (the pluggable notification interface with a log/in-memory implementation is sufficient for this feature).
- Consent management for contact channels used in code delivery — covered by the "Consent & Contact Opt-In" feature; this feature only requires that a delivery channel exists for testing.
- Full customer account/authentication system — the challenge flow remains anonymous-first, not an account login.
- Cryptographic receipt verification (signatures/hashes) — remains a Phase 3 concern.
- Alerting/dashboarding on top of challenge audit logs (this feature only guarantees the log entries exist).
