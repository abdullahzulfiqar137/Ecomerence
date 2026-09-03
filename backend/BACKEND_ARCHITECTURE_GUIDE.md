# 🏛️ Complete Backend Architecture & Implementation Guide

This guide explains the complete architecture, folder structure, design patterns, and step-by-step implementation process for building this enterprise-grade **Global E-Commerce Backend** using **.NET 10** and **Clean Architecture**.

---

## 📑 Table of Contents
1. [Architectural Overview](#1-architectural-overview)
2. [Folder & Directory Structure](#2-folder--directory-structure)
3. [Layer-by-Layer Breakdown](#3-layer-by-layer-breakdown)
   - [Domain Layer (`ECommerce.Domain`)](#31-domain-layer-ecommercedomain)
   - [Application Layer (`ECommerce.Application`)](#32-application-layer-ecommerceapplication)
   - [Infrastructure Layer (`ECommerce.Infrastructure`)](#33-infrastructure-layer-ecommerceinfrastructure)
   - [Presentation API Layer (`ECommerce.API`)](#34-presentation-api-layer-ecommerceapi)
   - [Testing Layer (`ECommerce.UnitTests` & `IntegrationTests`)](#35-testing-layer)
4. [Step-by-Step Implementation Workflow](#4-step-by-step-implementation-workflow)
5. [Enterprise Patterns Implemented](#5-enterprise-patterns-implemented)
6. [How to Run, Test, and Maintain](#6-how-to-run-test-and-maintain)

---

## 1. Architectural Overview

The backend is built following **Clean Architecture (Onion Architecture)** and **Domain-Driven Design (DDD)** principles.

```
                  ┌────────────────────────┐
                  │     ECommerce.API      │  Presentation / Controllers / Middleware
                  └───────────┬────────────┘
                              │
                              ▼
                  ┌────────────────────────┐
                  │ ECommerce.Application  │  Use Cases / DTOs / Interfaces / Validators
                  └───────────┬────────────┘
                              │
                              ▼
                  ┌────────────────────────┐
                  │    ECommerce.Domain    │  Entities / Enums / Value Objects (Core)
                  └────────────────────────┘
                              ▲
                              │
                  ┌───────────┴────────────┐
                  │ECommerce.Infrastructure│  EF Core / DB / Services / Background Workers
                  └────────────────────────┘
```

### Dependency Rules:
1. **Domain**: Has zero external dependencies. Pure C# classes representing business models and rules.
2. **Application**: Depends only on **Domain**. Defines business contracts (`Interfaces`), DTOs, and request validations.
3. **Infrastructure**: Depends on **Application** and **Domain**. Implements database persistence, external integrations, caching, and background workers.
4. **API**: The entry point. Registers DI containers, configures middleware pipeline, and exposes versioned RESTful controllers.

---

## 2. Folder & Directory Structure

```
backend/
├── CleanDb.sql                                 # Database cleanup script (resets data, retains schema)
├── ECommerce.sln                               # Visual Studio Solution file
├── ECommerce_Postman_Collection.json           # Comprehensive Postman test collection
├── run-api.bat                                 # Quick 1-click startup batch script
├── .env.example                                # Environment variable documentation
│
├── src/
│   ├── ECommerce.Domain/                       # [LAYER 1: Core Domain Entities & Rules]
│   │   ├── Common/                             # BaseEntity, AuditableEntity, ISoftDeletable
│   │   ├── Constants/                          # AppRoles, AppPermissions, CacheKeys
│   │   ├── Entities/                           # 58 Rich Business Entities
│   │   ├── Enums/                              # OrderStatus, PaymentStatus, AddressType, etc.
│   │   ├── Exceptions/                         # DomainException, NotFoundException, etc.
│   │   └── ValueObjects/                       # Money, Dimension, Weight
│   │
│   ├── ECommerce.Application/                  # [LAYER 2: Contracts, DTOs & Validation]
│   │   ├── Common/                             # PaginationParams, PagedResult
│   │   ├── DTOs/                               # Request/Response Data Transfer Objects
│   │   │   ├── Admin/                          # Analytics & user management DTOs
│   │   │   ├── Auth/                           # Login, Register, RefreshToken DTOs
│   │   │   ├── Cart/                           # AddToCart, CartItem DTOs
│   │   │   ├── Catalog/                        # Product, Category, Brand DTOs
│   │   │   ├── Checkout/                       # QuoteRequest, ProcessCheckout DTOs
│   │   │   ├── Coupons/                        # Coupon validation DTOs
│   │   │   ├── International/                  # Currency & Tax DTOs
│   │   │   ├── Inventory/                      # StockAdjustment, Warehouse DTOs
│   │   │   ├── Media/                          # FileUploadResultDto
│   │   │   ├── Notifications/                  # SendEmailRequest DTOs
│   │   │   ├── Orders/                         # Order creation & status update DTOs
│   │   │   ├── Payments/                       # Intent & Refund DTOs
│   │   │   ├── Reviews/                        # Review creation & rating DTOs
│   │   │   ├── Search/                         # Faceted search DTOs
│   │   │   ├── Shipping/                       # ShippingRate DTOs
│   │   │   └── Wishlist/                       # Wishlist item DTOs
│   │   ├── Interfaces/
│   │   │   ├── Repositories/                   # IGenericRepository, IUserRepository
│   │   │   └── Services/                       # 20+ Service interfaces (IAuthService, etc.)
│   │   ├── Models/                             # ApiResponse<T>, ProblemDetails
│   │   ├── Specifications/                     # Specification Pattern classes
│   │   └── Validators/                         # FluentValidation validator rules
│   │
│   ├── ECommerce.Infrastructure/               # [LAYER 3: EF Core, Services & Workers]
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs         # Main EF Core DbContext with 40+ DbSets
│   │   │   ├── Configurations/                 # 30+ Fluent API Entity Type Configurations
│   │   │   ├── Migrations/                     # EF Core code-first migrations
│   │   │   ├── Seed/                           # DatabaseSeeder (Roles, Admin, Demo Catalog)
│   │   │   └── Specification/                  # SpecificationEvaluator
│   │   ├── Repositories/                       # GenericRepository, UserRepository
│   │   ├── Services/                           # Concrete business implementations:
│   │   │   ├── Admin/                          # AdminUserService
│   │   │   ├── Analytics/                      # AnalyticsService
│   │   │   ├── Authentication/                 # AuthService, JwtTokenGenerator, PasswordHasher
│   │   │   ├── Caching/                        # CacheService (Distributed Memory / Redis)
│   │   │   ├── Cart/                           # CartService, WishlistService
│   │   │   ├── Catalog/                        # CatalogService, CategoryService, BrandService
│   │   │   ├── Checkout/                       # CheckoutService (Price calculation & quotes)
│   │   │   ├── Coupons/                        # CouponService
│   │   │   ├── Idempotency/                    # IdempotencyService (Duplicate prevention)
│   │   │   ├── International/                  # CurrencyService, TaxService
│   │   │   ├── Inventory/                      # InventoryService, WarehouseService
│   │   │   ├── Jobs/                           # BackgroundJobService & AutomatedMaintenanceWorker
│   │   │   ├── Notifications/                  # NotificationService, EmailServices
│   │   │   ├── Orders/                         # OrderService & OrderStateMachine
│   │   │   ├── Payments/                       # PaymentService & Multi-Gateway Adapters
│   │   │   ├── Reviews/                        # ReviewService
│   │   │   ├── Search/                         # SearchService (Multi-term faceted search)
│   │   │   ├── Shipping/                       # ShippingService
│   │   │   └── Storage/                        # LocalFileStorageService (Media uploads)
│   │   └── DependencyInjection.cs              # DI extension method AddInfrastructureServices()
│   │
│   └── ECommerce.API/                          # [LAYER 4: Presentation & Pipeline]
│       ├── Controllers/
│       │   ├── BaseApiController.cs            # Base controller with standard responses & user claims
│       │   └── v1/                             # 26 API Version 1 Controllers
│       ├── Middlewares/
│       │   ├── CorrelationIdMiddleware.cs      # Distributed tracing header injection
│       │   ├── SecurityHeadersMiddleware.cs    # CSP, HSTS, X-Content-Type security headers
│       │   ├── GlobalExceptionHandlerMiddleware.cs # Unhandled exception RFC 7807 responses
│       │   └── RequireIdempotencyAttribute.cs  # Action filter for safe retryability
│       ├── Program.cs                          # Host builder, Swagger, Rate Limiting, Pipeline
│       ├── appsettings.json                    # Connection strings, JWT settings, Email config
│       └── appsettings.Development.json        # Dev overrides
│
└── tests/
    ├── ECommerce.UnitTests/                    # 64 Unit Tests covering all core services
    └── ECommerce.IntegrationTests/             # WebApplicationFactory integration tests
```

---

## 3. Layer-by-Layer Breakdown

### 3.1. Domain Layer (`ECommerce.Domain`)
**Purpose**: Models real-world e-commerce entities and state machines.
* **Entities**:
  * `User`, `Role`, `Permission`, `RefreshToken`, `AuditLog`
  * `Product`, `ProductVariant`, `ProductAttribute`, `ProductAttributeValue`, `ProductImage`
  * `Category`, `Brand`, `ProductReview`, `ProductQuestion`, `ProductAnswer`
  * `Cart`, `CartItem`, `Wishlist`, `WishlistItem`
  * `Order`, `OrderItem`, `OrderAddress`, `OrderStatusHistory`, `OrderNote`
  * `InventoryItem`, `Warehouse`, `StockMovement`, `StockAdjustment`, `InventoryReservation`, `LowStockAlert`
  * `Payment`, `Shipment`, `ShippingMethod`, `ShippingRate`, `ShippingZone`, `TrackingEvent`
  * `Coupon`, `DiscountRule`, `TaxRule`, `Currency`, `Country`, `Notification`, `IdempotentRequest`
* **Enums**: `OrderStatus`, `PaymentStatus`, `DiscountType`, `AddressType`, `NotificationStatus`, etc.
* **Common**: `BaseEntity` (Guid Id) and `AuditableEntity` (CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy, IsDeleted).

### 3.2. Application Layer (`ECommerce.Application`)
**Purpose**: Use case logic, data transfer contracts, and validations.
* **DTOs**: Segregated by feature domain so requests and responses never leak EF Core domain models directly to the consumer.
* **Interfaces**: Decouples API from implementation:
  * `ICatalogService`, `IOrderService`, `IInventoryService`, `IPaymentService`, `ICheckoutService`, `IIdempotencyService`, `IFileStorageService`, etc.
* **Validators**: Uses **FluentValidation** for strong input sanitization (e.g., password complexity, email format, positive order totals).
* **Models**: `ApiResponse<T>` unified response wrapper:
  ```json
  {
    "success": true,
    "message": "Operation successful",
    "data": { ... },
    "errors": []
  }
  ```

### 3.3. Infrastructure Layer (`ECommerce.Infrastructure`)
**Purpose**: External integrations, database engine, and heavy computations.
* **EF Core Configuration**: Every entity has a separate `IEntityTypeConfiguration<T>` in `Data/Configurations/` defining exact column types, lengths, foreign keys, and indexes.
* **Order State Machine (`IOrderStateMachine`)**: Validates allowable state transitions (e.g. `Pending` -> `Confirmed` -> `Shipped` -> `Delivered`).
* **Safe Idempotency (`IdempotencyService`)**: Checks SHA-256 payload hashes against cached execution records to guarantee duplicate requests return the same initial response without double-charging or duplicate order creation.
* **Media Storage (`LocalFileStorageService`)**: Manages file uploads to `wwwroot/uploads/` with GUID sanitization, MIME validation, and static file delivery.
* **Automated Background Worker (`AutomatedMaintenanceBackgroundWorker`)**: Registered as an `IHostedService`:
  * Every 2 minutes: Releases expired cart inventory holds and updates low-stock alerts.
  * Every 6 hours: Deactivates expired coupons and purges abandoned guest carts.

### 3.4. Presentation Layer (`ECommerce.API`)
**Purpose**: HTTP routing, request filtering, serialization, and documentation.
* **API Versioning**: URL-segment routing (`/api/v1/...`) with Swagger documentation grouping.
* **Rate Limiting (`Microsoft.AspNetCore.RateLimiting`)**:
  * `AuthPolicy`: 10 requests / min per IP (prevents brute-force credential stuffing).
  * `CheckoutPolicy`: 20 requests / min per IP (prevents card cracking).
  * `GlobalPolicy`: 120 requests / min per IP.
* **Middlewares**:
  * `CorrelationIdMiddleware`: Adds `X-Correlation-ID` header to trace requests across logs.
  * `SecurityHeadersMiddleware`: Injects X-Frame-Options, X-XSS-Protection, CSP, and HSTS headers.
  * `GlobalExceptionHandlerMiddleware`: Translates uncaught exceptions into RFC 7807 Problem Details.
  * `RequireIdempotencyAttribute`: Action filter enforcing safe retries.

---

## 4. Step-by-Step Implementation Workflow

To build this structure from scratch in a new repository, follow this systematic process:

### Step 1: Initialize the Solution & Projects
```powershell
mkdir backend
cd backend
dotnet new sln -n ECommerce

# Create class libraries and Web API
dotnet new classlib -o src/ECommerce.Domain -f net10.0
dotnet new classlib -o src/ECommerce.Application -f net10.0
dotnet new classlib -o src/ECommerce.Infrastructure -f net10.0
dotnet new webapi -o src/ECommerce.API -f net10.0

# Create test projects
dotnet new xunit -o tests/ECommerce.UnitTests -f net10.0
dotnet new xunit -o tests/ECommerce.IntegrationTests -f net10.0

# Add all projects to solution
dotnet sln add (Get-ChildItem -Recurse *.csproj)
```

### Step 2: Establish Project References
```powershell
# Application references Domain
dotnet add src/ECommerce.Application reference src/ECommerce.Domain

# Infrastructure references Application and Domain
dotnet add src/ECommerce.Infrastructure reference src/ECommerce.Application
dotnet add src/ECommerce.Infrastructure reference src/ECommerce.Domain

# API references Infrastructure and Application
dotnet add src/ECommerce.API reference src/ECommerce.Infrastructure
dotnet add src/ECommerce.API reference src/ECommerce.Application

# Tests reference all layers
dotnet add tests/ECommerce.UnitTests reference src/ECommerce.Application src/ECommerce.Infrastructure src/ECommerce.Domain
dotnet add tests/ECommerce.IntegrationTests reference src/ECommerce.API
```

### Step 3: Install Core NuGet Packages
* **Application**: `FluentValidation.DependencyInjectionExtensions`
* **Infrastructure**: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.InMemory`, `Microsoft.Extensions.Caching.Memory`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `BCrypt.Net-Next`
* **API**: `Asp.Versioning.Mvc.ApiExplorer`, `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`
* **Tests**: `FluentAssertions`, `Moq`, `Microsoft.AspNetCore.Mvc.Testing`

### Step 4: Implement Domain Entities & Common Base Classes
Create `BaseEntity`, `AuditableEntity`, and domain entities in `ECommerce.Domain/Entities/`.

### Step 5: Define Contracts in Application Layer
Create DTOs, FluentValidation validators, and service interfaces in `ECommerce.Application/Interfaces/Services/`.

### Step 6: Implement Infrastructure Persistence & Services
1. Create `ApplicationDbContext` and configure entity mappings via Fluent API.
2. Implement services (`AuthService`, `CatalogService`, `OrderService`, `IdempotencyService`, `LocalFileStorageService`).
3. Create `DatabaseSeeder.cs` for initial roles, users, currencies, categories, and products.
4. Create `AutomatedMaintenanceBackgroundWorker.cs` for scheduled background tasks.
5. Write `AddInfrastructureServices()` extension method in `DependencyInjection.cs`.

### Step 7: Build Controllers & Pipeline in API
1. Implement `BaseApiController` and v1 controllers.
2. Configure `Program.cs` with Serilog, JWT authentication, Swagger, Rate Limiting, Static Files, and Health Checks.
3. Apply `[RequireIdempotency]` to checkout and payment actions.

---

## 5. Enterprise Patterns Implemented

| Pattern | Location | Purpose |
|---|---|---|
| **Clean Architecture** | Entire Solution | Strict separation of business rules from database and frameworks |
| **Specification Pattern** | `Application/Specifications` | Reusable, composable query logic across repositories |
| **State Machine Pattern** | `Infrastructure/Services/Orders` | Deterministic, audited order lifecycle state transitions |
| **Idempotency Engine** | `Infrastructure/Services/Idempotency` | Guarantees exactly-once order placement and payment execution |
| **Repository & Unit of Work** | `Infrastructure/Repositories` | Encapsulates EF Core queries and transaction boundaries |
| **Adapter / Factory Pattern** | `Infrastructure/Services/Payments` | Pluggable multi-payment gateway routing (Stripe, PayPal, COD) |
| **Hosted Service / Worker** | `Infrastructure/Services/Jobs` | Automated background reconciliation of inventory and cart holds |
| **Result Wrapper Pattern** | `Application/Models/ApiResponse.cs` | Consistent standard JSON envelope across all endpoints |

---

## 6. How to Run, Test, and Maintain

### 🚀 Running Locally
```powershell
cd backend
dotnet run --project src/ECommerce.API
```
* **Swagger Documentation**: Open `https://localhost:7196` or `http://localhost:5196` in your browser.
* **Health Check**: `GET http://localhost:5196/health`

### 🧪 Running Automated Tests
```powershell
cd backend
dotnet test
```
All **64 unit tests** and integration tests will execute against in-memory database configurations.

### 🧹 Database Reset Script
If you need to wipe out business test data while keeping roles, admin users, and schema intact:
Execute `backend/CleanDb.sql` in SQL Server Management Studio (SSMS) or Azure Data Studio.
