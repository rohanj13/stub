# Feature PRD: Additional POS Provider Support

## 1. Feature Name

Additional POS Provider Support — POS Adapter Abstraction for Multi-Provider Ingestion

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Trust, Compliance & Partner Ecosystem
- Architecture Spec: [arch.md](../arch.md) — see "POS Adapter Abstraction" in the System Architecture Diagram

## 3. Goal

### Problem

Receipt ingestion today is hard-wired to Square's webhook payload shape. A merchant using any other POS provider cannot onboard without bespoke ingestion engineering, and every additional provider risks duplicating (and diverging from) the validation, idempotency, and claimability logic already built for Square.

### Solution

Introduce a POS Adapter abstraction (`IPosAdapter`) that normalizes a provider-specific transaction payload into the platform's existing core receipt/envelope ingestion contract. Refactor the existing Square integration to implement this interface as the reference adapter, and add at least one additional adapter to prove the abstraction holds for a differently shaped payload. Adapter-normalized events flow through the same validation, idempotency, and claimable-state-machine pipeline already in place — no changes to claim, history, envelope, or compliance logic are required to onboard a new provider.

### Impact

- Converts POS-provider expansion from a rearchitecture risk into an additive engineering task (implement one adapter).
- Preserves all existing Phase 1/Phase 2/Phase 3 ingestion guarantees (validation, idempotency, claimability, compliance evaluation) for every provider automatically.
- Expands the platform's addressable market to merchants on non-Square POS systems.

## 4. User Personas

- **Merchant Admin**: Wants to connect their POS provider (not just Square) and have transactions flow into claimable receipts the same way.
- **Backend Engineering**: Wants to add a new POS provider without touching shared ingestion, claim, history, envelope, or compliance code.
- **Store Associate**: Needs the checkout tap/scan experience to behave identically regardless of which POS provider the merchant uses.

## 5. User Stories

- As **Backend Engineering**, I want a well-defined `IPosAdapter` interface, so that adding a new POS provider means implementing one adapter rather than modifying shared ingestion logic.
- As a **Merchant Admin**, I want to connect a non-Square POS provider, so that I can use Stub without switching my point-of-sale system.
- As a **Merchant Admin**, I want transactions from my POS provider to become claimable receipts the same way Square transactions do, so that the checkout tap/scan experience is unaffected by my provider choice.
- As **Backend Engineering**, I want invalid or malformed payloads from any adapter to be rejected using the same semantics as the existing Square webhook path, so that error handling stays consistent across providers.

## 6. Requirements

### Functional Requirements

- The system must define an `IPosAdapter` interface (or equivalent abstraction) whose contract is a provider-specific transaction payload in, and the platform's existing core receipt/envelope ingestion shape out.
- The system must refactor the existing Square webhook ingestion to implement this interface as the reference adapter, with no change in externally observable behavior for existing Square merchants.
- The system must support at least one additional adapter implementation for a differently shaped provider payload, proving the abstraction generalizes beyond Square's specific format.
- The system must route an incoming transaction event to the correct adapter based on the merchant's configured POS provider.
- The system must apply the same validation, idempotency, and claimable-state-machine logic to adapter-normalized payloads regardless of source provider.
- The system must reject or quarantine a payload that fails core validation after adapter normalization, using the same rejection semantics (status codes, error contracts) as the existing Square webhook path.
- The system must allow a merchant to configure which POS provider/adapter their incoming transactions should be routed through.
- The system must support onboarding a new POS provider (adding a new adapter) without modifying claim, history, envelope, or compliance evaluation logic.

### Non-Functional Requirements

- **Extensibility**: Adding a new POS provider must require implementing the adapter interface only; it must not require changes to shared ingestion pipeline, claim, history, envelope, or compliance code.
- **Backward Compatibility**: Refactoring Square ingestion into the adapter abstraction must not change any existing Square merchant's observable behavior, response contracts, or latency characteristics.
- **Reliability**: Adapter normalization failures must be handled deterministically (rejected/quarantined) and must not crash or destabilize ingestion for other merchants/providers.
- **Performance**: Adapter normalization must not materially increase ingest-to-claimable latency established in Phase 1 for any provider, including Square.
- **Testability**: Each adapter implementation must be independently testable against representative provider payloads without requiring a live connection to that provider.

## 7. Acceptance Criteria

**Story: Square ingestion unaffected by refactor**
- [ ] Given the existing Square webhook ingestion is refactored to implement `IPosAdapter`, When a valid Square transaction event is received, Then the resulting receipt, claimability state, and response contract are identical to pre-refactor behavior.

**Story: New provider adapter normalizes correctly**
- [ ] Given a merchant is configured to use the additional POS provider's adapter, When a valid transaction event from that provider is received, Then it is normalized into the core receipt/envelope shape and the receipt becomes `claimable` through the existing state machine.

**Story: Invalid payload from a new adapter is rejected consistently**
- [ ] Given a malformed or invalid payload is received for a merchant using the additional provider's adapter, When ingestion is attempted, Then the payload is rejected using the same error semantics as an equivalent invalid Square payload.

**Story: Provider routing by merchant configuration**
- [ ] Given a merchant is configured for a specific POS provider, When a transaction event arrives, Then it is routed to and processed by that provider's adapter and no other.

**Story: Downstream pipeline unchanged for new provider receipts**
- [ ] Given a receipt originated from the additional provider's adapter, When claim, history retrieval, envelope module access, or compliance evaluation is performed on it, Then behavior is identical to a receipt originated from Square.

## 8. Out of Scope

- Full feature parity with Square for every additional POS provider (initial adapters may cover core transaction ingestion only, not every Square-specific capability).
- A self-service provider-adapter marketplace or third-party adapter contribution process.
- Provider-specific UI customization in the frontend beyond selecting/configuring which adapter a merchant uses.
- Migrating existing Square merchants to a different provider (this feature adds new-provider support; it does not migrate existing merchant data between providers).
