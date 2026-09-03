# 🛒 Global E-Commerce Platform

A production-ready, enterprise-grade, scalable E-Commerce platform built with **.NET 10** and **Clean Architecture**.

---

## 📁 Repository Structure

```
├── backend/                             # Complete ASP.NET Core .NET 10 Backend API
│   ├── src/                             # Clean Architecture source projects
│   │   ├── ECommerce.Domain/            # Entities, Enums, Value Objects (Zero dependencies)
│   │   ├── ECommerce.Application/       # DTOs, Interfaces, Validators, Models
│   │   ├── ECommerce.Infrastructure/    # EF Core, Services, Storage, Background Workers
│   │   └── ECommerce.API/               # REST Controllers (v1), Middlewares, Program.cs
│   ├── tests/                           # Unit and Integration test suites (64/64 passing)
│   │   ├── ECommerce.UnitTests/
│   │   └── ECommerce.IntegrationTests/
│   ├── CleanDb.sql                      # Database maintenance & wipe script
│   ├── ECommerce_Postman_Collection.json # Complete API testing Postman collection
│   ├── run-api.bat                      # 1-click launch script
│   └── BACKEND_ARCHITECTURE_GUIDE.md    # Complete architecture & implementation guide
```

---

## 📚 Complete Architecture Guide

For a comprehensive explanation of:
* How every layer was designed and organized
* The complete directory structure and file breakdown
* Enterprise patterns (Idempotency, Rate Limiting, State Machine, Outbox)
* How to reproduce this architecture step-by-step

👉 **Read the [Backend Architecture Guide](file:///d:/Ecomerence/backend/BACKEND_ARCHITECTURE_GUIDE.md)**

---

## ⚡ Quick Start

```powershell
# 1. Navigate to backend
cd backend

# 2. Run all unit tests
dotnet test

# 3. Launch the API
dotnet run --project src/ECommerce.API
```

* **Swagger UI**: `http://localhost:5196`
* **Health Check**: `http://localhost:5196/health`
