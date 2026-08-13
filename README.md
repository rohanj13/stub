# stub
Digital Receipt Infrastructure

## Boilerplate stack
- **Backend**: ASP.NET Core (.NET 10) minimal API
- **Frontend**: Static web UI served from the backend (`wwwroot/index.html`)
- **Storage for MVP boilerplate**: In-memory service for merchants and receipts

## MVP workflow covered
1. Merchant portal onboarding (create merchant + connect Square account/webhook registration flag)
2. Square transaction webhook ingestion (creates a digital receipt object)
3. Customer stub-card scan simulation (assign customer id to receipt without signup)
4. Receipt history lookup by customer id

## API surface
- `POST /api/merchants`
- `GET /api/merchants`
- `POST /api/merchants/{merchantId}/connect/square`
- `POST /api/webhooks/square/transactions`
- `GET /api/receipts/{receiptId}`
- `POST /api/receipts/{receiptId}/assign-customer`
- `GET /api/customers/{customerId}/receipts`

## Run locally
```bash
dotnet run --project /home/runner/work/stub/stub/src/Stub.Api/Stub.Api.csproj
```

Then open `http://localhost:5077` (or the URL printed by `dotnet run`) to use the web UI.
