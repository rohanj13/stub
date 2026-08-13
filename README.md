# stub
Digital Receipt Infrastructure

## Boilerplate stack
- **Backend**: ASP.NET Core (.NET 10) minimal API
- **Frontend**: React + Vite app in `src/Stub.Frontend` (runs separately from backend)
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

In a second terminal:

```bash
cd /home/runner/work/stub/stub/src/Stub.Frontend
npm install
npm run dev
```

Then open `http://localhost:5173` to use the web UI.

If the API runs on a different URL, set `VITE_API_BASE_URL` before starting the frontend.
