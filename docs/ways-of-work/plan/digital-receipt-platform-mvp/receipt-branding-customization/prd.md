# Feature PRD: Merchant Receipt Branding & Customization

## 1. Feature Name

Merchant Receipt Branding & Customization (Baseline)

## 2. Epic

- Parent Epic: [epic.md](../epic.md) — Digital Receipt Platform MVP
- Architecture Spec: [arch.md](../arch.md) — see "Merchant Management Service" in the System Architecture Diagram

## 3. Goal

### Problem

Merchants want their digital receipts to look like their brand (store name, logo, an optional message), not a generic Stub template. There is currently no merchant-level storage for these preferences, and no guardrail preventing branding customization from overwriting or hiding compliance-required receipt fields.

### Solution

Add a merchant-level branding preferences record (display name, logo reference, optional message) that the receipt rendering surface reads and applies, while enforcing that compliance-required fields (totals, tax lines, transaction id, timestamps) always render regardless of branding configuration.

### Impact

- Improves merchant satisfaction and pilot adoption by making receipts feel merchant-branded rather than generic.
- Establishes a config surface pattern that later phases (loyalty/ads/warranty display preferences) can extend.

## 4. User Personas

- **Merchant Admin**: Configures branding for their store's receipts.
- **Customer (No Signup)**: Views the branded receipt when looking up a purchase.

## 5. User Stories

- As a **Merchant Admin**, I want to set my store's display name, logo, and an optional message on receipts, so that customers recognize my brand.
- As a **Merchant Admin**, I want to know that my branding customization cannot hide required compliance fields, so that I don't accidentally create a non-compliant receipt.
- As a **Customer**, I want to see the merchant's branding when I view a receipt, so that I can easily identify where the purchase was made.

## 6. Requirements

### Functional Requirements

- The system must store, per merchant, a display name (defaulting to the merchant's registered `Name` if unset), an optional logo reference (URL or asset id), and an optional short message string with a defined max length.
- The system must expose an endpoint to get and update a merchant's branding preferences.
- The system must validate that updates to branding preferences never remove or overwrite the core compliance-required receipt fields already defined in the receipt entity (totals, currency, transaction id, timestamps, line items).
- The receipt retrieval response must include the associated merchant's branding preferences alongside the core receipt data for rendering.
- The system must reject a branding update with an oversized message string (`400 Bad Request`) rather than silently truncating.

### Non-Functional Requirements

- **Data Integrity**: Branding preferences must be stored separately from core receipt/compliance fields so a schema or data bug in branding can never corrupt compliance data.
- **Performance**: Fetching a receipt with branding must not introduce a noticeable additional round trip beyond a single joined query.
- **Extensibility**: The branding preferences model should be structured so additional customization fields (e.g., color theme) can be added later without breaking existing consumers.

## 7. Acceptance Criteria

**Story: Set branding preferences**
- [ ] Given a registered merchant, When branding preferences are submitted with a display name, logo reference, and message, Then they are persisted and returned on subsequent get requests.
- [ ] Given a message exceeding the max length, When an update is submitted, Then the response is `400 Bad Request` and no partial update is saved.

**Story: Compliance fields are protected**
- [ ] Given any branding configuration, When a receipt is rendered/retrieved, Then totals, currency, transaction id, timestamps, and line items are always present and unaffected by branding content.

**Story: Receipt includes branding on retrieval**
- [ ] Given a merchant with branding preferences set, When a receipt for that merchant is retrieved by id, Then the response includes the merchant's branding alongside the core receipt fields.
- [ ] Given a merchant with no branding preferences set, When a receipt is retrieved, Then the response falls back to the merchant's registered name with no logo/message.

## 8. Out of Scope

- Rich receipt theming (colors, layout templates) — only name/logo/message in this feature.
- Ads/promotional placement content — covered by the envelope/loyalty features in later phases.
- Logo file upload/storage pipeline — this feature assumes a logo reference (URL) is provided, not binary upload handling.
