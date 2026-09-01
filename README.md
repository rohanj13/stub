# Stub - Digital Receipt Platform

Stub is a digital receipt platform that receives transactions from POS providers, stores the raw transaction, transforms it into a canonical receipt format, and assigns receipts to anonymous customer identities.

## Architecture

This project uses a **modular monolith** architecture with the following structure:

```
Stub.sln
src/
  Stub.Api/              - ASP.NET Core Web API with controllers and endpoints
  Stub.Application/      - Application layer (business logic)
  Stub.Domain/           - Domain entities and enums
  Stub.Infrastructure/   - Data access, EF Core, and external integrations
tests/
  Stub.Api.Tests/
  Stub.Application.Tests/
  Stub.Domain.Tests/
```

### Dependencies

- **Api** → Application → Domain
- **Infrastructure** → Application + Domain

## Tech Stack

- **ASP.NET Core Web API** (.NET 10.0 LTS)
- **PostgreSQL** (via Docker Compose)
- **Entity Framework Core 10.x**
- **Npgsql** (PostgreSQL provider)
- **Swagger/OpenAPI** (API documentation)
- **Docker Compose** (local development)

## API Endpoints

### Merchants
- `POST /api/merchants` - Create a new merchant
- `GET /api/merchants/{id}` - Get merchant details
- `GET /api/merchants` - List all merchants

### Terminals
- `POST /api/terminals` - Create a new terminal
- `GET /api/terminals/{id}` - Get terminal details

### Receipt Identities
- `POST /api/receipt-identities` - Create a new receipt identity
- `GET /api/receipt-identities/{id}` - Get receipt identity details
- `GET /api/receipt-identities/{publicIdentifier}/receipts` - Get all receipts for an identity

### Receipts
- `GET /api/receipts` - List all receipts
- `GET /api/receipts/{id}` - Get receipt details with items, payments, and assignments

### Receipt Assignments
- `POST /api/receipt-assignments` - Assign a receipt to an identity

### Receipt Assignment Sessions
- `POST /api/receipt-assignment-sessions` - Create a new assignment session

### Webhooks
- `POST /api/webhooks/square` - Receive Square webhook events

### Health
- `GET /api/health` - Health check endpoint

## Getting Started

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for PostgreSQL)

### Running Locally

1. **Start PostgreSQL**:
   ```bash
   docker compose up db -d
   ```

2. **Run the API**:
   ```bash
   cd src/Stub.Api
   dotnet run
   ```

3. **Access Swagger UI**:
   Open your browser to the URL shown in the console (Swagger UI is at the root `/`)

### Run with Docker Compose

```bash
docker compose up --build
```

Services:
- API + Swagger: `http://localhost:8080`
- PostgreSQL: `localhost:5432`

### Database Migrations

To create and apply migrations:

```bash
# Add a migration
dotnet ef migrations add InitialCreate --project src/Stub.Infrastructure --startup-project src/Stub.Api

# Apply migrations
dotnet ef database update --project src/Stub.Infrastructure --startup-project src/Stub.Api
```

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Domain Model

### Core Entities

- **Merchant** - Business using the platform
- **Terminal** - Physical/virtual POS terminal
- **HardwareDevice** - Devices attached to terminals (NFC, QR, etc.)
- **ProviderConnection** - Connection to POS provider (Square, Shopify, Lightspeed)
- **ReceiptIdentity** - Anonymous customer identity (e.g., `rid_01ABC123XYZ`)
- **ReceiptCredential** - Credential types (QR, NFC, AppleWallet, GoogleWallet)
- **CustomerProfile** - Optional user profile
- **RawTransaction** - Original POS payload (stored as jsonb)
- **Receipt** - Canonical digital receipt
- **ReceiptItem** - Line items on a receipt
- **ReceiptPayment** - Payment details
- **ReceiptExtension** - Future metadata (warranties, loyalty, etc.)
- **ReceiptAssignment** - Links receipt to identity
- **ReceiptAssignmentSession** - Temporary session for customer interaction

### Database Features
- **jsonb columns**: `RawTransaction.Payload`, `ReceiptExtension.Data`
- **Decimal precision**: Monetary fields use `decimal(18,2)`
- **Indexes**: PublicIdentifier, CredentialValue, ExternalTransactionId
- **Timestamps**: Automatic CreatedAt/UpdatedAt tracking

## Provider Integrations

Provider structure for POS integrations:

```
Stub.Infrastructure/Providers/
  Square/
    SquareWebhookHandler.cs
    SquareReceiptMapper.cs
  Shopify/
    ShopifyWebhookHandler.cs
  Lightspeed/
    LightspeedWebhookHandler.cs
```

### Intended Flow

1. Square Webhook → RawTransaction → SquareReceiptMapper → Receipt → ReceiptAssignment

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=stub;Username=postgres"
  }
}
```

## What's Implemented

✅ Modular monolith architecture  
✅ All domain entities with relationships  
✅ Entity Framework Core with PostgreSQL  
✅ Entity configurations with jsonb support  
✅ Basic API controllers  
✅ Swagger documentation  
✅ Docker Compose for PostgreSQL  
✅ Project structure and dependencies  

## What's Not Implemented Yet

- EF Core migrations (create with `dotnet ef migrations add`)
- Square integration (handlers are placeholders)
- Authentication and authorization
- Background job processing
- Email notifications
- NFC hardware functionality

## License

Copyright © 2026. All rights reserved.
