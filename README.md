# stub
Digital Receipt Infrastructure

## Boilerplate stack
- **Backend**: ASP.NET Core (.NET 10) minimal API
- **Frontend**: React + Vite app in `src/Stub.Frontend` (runs separately from backend)
- **Storage**: PostgreSQL for merchants and receipts

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
1) Start PostgreSQL:
```bash
docker run --rm -p 5432:5432 \
  -e POSTGRES_DB=stub \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_HOST_AUTH_METHOD=trust \
  postgres:17-alpine
```

2) Start API:
```bash
dotnet run --project /home/runner/work/stub/stub/src/Stub.Api/Stub.Api.csproj
```

3) In a second terminal start frontend:

```bash
cd /home/runner/work/stub/stub/src/Stub.Frontend
npm install
npm run dev
```

Then open `http://localhost:5173` to use the web UI.

If the API runs on a different URL, set `VITE_API_BASE_URL` before starting the frontend.

## Run with Docker Compose
```bash
docker compose -f /home/runner/work/stub/stub/docker-compose.yml up --build
```

Services:
- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
