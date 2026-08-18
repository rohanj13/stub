# Feature PRD: Checkout Claim (Customer-to-Receipt Assignment)

## 1. Feature Name

Checkout Claim — Assigning a Customer Stub Identifier to a Claimable Receipt

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "Checkout Claim Service" in the System Architecture Diagram

## 3. Goal

### Problem

A receipt becomes `Claimable` after Square ingestion, but nothing yet links it to the customer standing at the register. Without a fast, reliable claim action, the customer leaves without any way to retrieve the receipt later, defeating the purpose of the platform. Today this is modeled as `POST /api/receipts/{receiptId}/assign-customer`, but it lacks state-machine guardrails (e.g., claiming an already-claimed receipt, or claiming a receipt still `Pending`) and deterministic retry guidance for the checkout device.

### Solution

Harden the claim endpoint to enforce the `Claimable → Claimed` transition explicitly, reject claim attempts against receipts that are not yet claimable (with a response the checkout UI can use to retry), and prevent re-assignment of an already-claimed receipt to a different customer without an explicit override path.

### Impact

- Directly enables the core "tap NFC / scan QR at checkout" journey central to the platform's value proposition.
- Reduces failed-claim confusion at the register by giving the checkout device a deterministic signal to retry vs. stop.
- Prevents accidental reassignment of a receipt to the wrong customer identifier.

## 4. User Personas

- **Customer (No Signup)**: Taps/scans their stub identifier at checkout to claim the receipt.
- **Store Associate**: Operates the checkout device that initiates or displays the claim outcome.

## 5. User Stories

- As a **Customer**, I want to tap my stub pass/QR at checkout right after paying, so that the receipt is linked to my identity without creating an account.
- As a **Store Associate**, I want the checkout device to tell me clearly if the receipt isn't claimable yet, so that I can prompt the customer to retry in a moment rather than assume failure.
- As a **Customer**, I want an already-claimed receipt to not be silently reassigned to someone else's tap, so that my purchase history stays accurate and private.

## 6. Requirements

### Functional Requirements

- The claim endpoint must accept a receipt id and a customer stub identifier and require the identifier to be non-empty (`400 Bad Request` otherwise).
- The claim endpoint must return `404 Not Found` for an unknown receipt id.
- The claim endpoint must return a distinct, deterministic response (e.g., `409 Conflict` with a `retryable: true` flag) when the receipt exists but is still in `Pending` state, so the checkout device can retry.
- The claim endpoint must succeed and transition the receipt to `Claimed` when the receipt is `Claimable` and unclaimed.
- The claim endpoint must reject (not silently overwrite) a claim attempt on a receipt already `Claimed` by a different customer identifier, returning a clear conflict response.
- A repeat claim request with the same customer identifier on an already-claimed receipt must be treated as an idempotent success (no error).
- The system must record the claim timestamp (`ClaimedAtUtc`) on successful assignment.

### Non-Functional Requirements

- **Latency**: The claim action must complete within the checkout interaction window (sub-second under normal load).
- **Consistency**: State transition (`Claimable → Claimed`) must be atomic to avoid race conditions from near-simultaneous claim attempts.
- **Determinism**: Every failure mode (not found, not yet claimable, already claimed by someone else) must map to a distinct, documented response so the checkout UI can react appropriately.

## 7. Acceptance Criteria

**Story: Successful claim**

- [ ] Given a `Claimable` receipt, When a customer identifier is submitted to the claim endpoint, Then the receipt transitions to `Claimed` with `ClaimedAtUtc` set and the response confirms success.

**Story: Claim attempted before receipt is claimable**

- [ ] Given a receipt still in `Pending` state, When a claim is attempted, Then the response indicates the receipt is not yet ready and is retryable, and no assignment occurs.

**Story: Claim attempted on unknown receipt**

- [ ] Given a receipt id that does not exist, When a claim is attempted, Then the response is `404 Not Found`.

**Story: Re-claim conflict**

- [ ] Given a receipt already `Claimed` by customer A, When customer B attempts to claim it, Then the request is rejected with a conflict response and the original assignment is unchanged.
- [ ] Given a receipt already `Claimed` by customer A, When customer A's identifier is submitted again, Then the response is a success (idempotent) with no state change beyond the existing claim.

## 8. Out of Scope

- NFC/QR wallet-pass issuance and the physical tap/scan hardware/UX integration (this feature only covers the backend assignment API).
- Anonymous history retrieval after claim (covered by a separate feature).
- Customer identity verification beyond possession of the stub identifier at claim time.
