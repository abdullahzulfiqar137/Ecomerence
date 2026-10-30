# Global E-Commerce Platform

A full-stack e-commerce application with an ASP.NET Core **.NET 10** backend organized around Clean Architecture.

## Architecture

The backend is organized into the following layers:

- **Domain** — core entities, enums, and value objects
- **Application** — use cases, DTOs, interfaces, validators, and application models
- **Infrastructure** — Entity Framework Core, persistence, integrations, and background services
- **API** — REST endpoints, middleware, and application startup

## Repository structure

```text
.
├── backend/
│   ├── src/
│   │   ├── ECommerce.Domain/
│   │   ├── ECommerce.Application/
│   │   ├── ECommerce.Infrastructure/
│   │   └── ECommerce.API/
│   ├── tests/
│   │   ├── ECommerce.UnitTests/
│   │   └── ECommerce.IntegrationTests/
│   ├── CleanDb.sql
│   ├── ECommerce_Postman_Collection.json
│   └── BACKEND_ARCHITECTURE_GUIDE.md
├── frontend/
└── run-backend.bat
```

## Technology

- C# and .NET 10
- ASP.NET Core Web API
- Clean Architecture
- Entity Framework Core
- SQL Server
- Unit and integration test projects

## Prerequisites

- .NET 10 SDK
- SQL Server
- Node.js and npm for the frontend (if working on the frontend)

## Quick start — backend

From the repository root:

```powershell
cd backend
dotnet restore
dotnet test
dotnet run --project src/ECommerce.API
```

The repository's current README documents Swagger at http://localhost:5196 and a health endpoint at http://localhost:5196/health. Confirm the actual URLs from the API launch output and configuration.

## Documentation

- [Backend architecture guide](backend/BACKEND_ARCHITECTURE_GUIDE.md)
- [Postman collection](backend/ECommerce_Postman_Collection.json)

## Database safety

Review `backend/CleanDb.sql` carefully before running it: database cleanup scripts can permanently delete data. Use a disposable development database and take a backup before any destructive database operation.

Never commit production connection strings, passwords, API keys, access tokens, or customer data.

## Testing

Run the backend test suites with:

```powershell
cd backend
dotnet test
```

A previous version of this README reported 64 passing tests. Treat that as a historical report, not a current guarantee; run the command above to verify the current checkout.

## License

No license terms are stated here. Unless a license file is present and applicable, all rights remain with the copyright holder.
