# 🎟️ EventFlow

**EventFlow** is a  booking platform built with ASP.NET Core 8. It features a RESTful API, an MVC web interface, background workers, role-based access control, localization, health monitoring, and a comprehensive unit test suite — all structured around Clean Architecture principles.

---

## 📋 Table of Contents

-  Overview
-  Architecture
-  Tech Stack
-  Features
-  Project Structure
-  Getting Started
-  API Endpoints
-  Roles & Permissions
   Background Workers
-  Health Checks
-  Localization
-  Testing
-  Database Seeding

---

## 🌐 Overview

EventFlow allows users to browse events, book or purchase tickets, manage their bookings, and create their own events. Administrators and moderators can approve pending events, manage users, and configure global system settings.

The system is split across two runnable frontends:

| App | Purpose |
|-----|---------|
| `EventFlow.API` | RESTful JSON API with JWT authentication |
| `EventFlow.MVC` | Cookie-authenticated Razor MVC web application |
| `EventFlow.Workers` | Background service host for scheduled tasks |
| `HealthChecker` | Standalone health check UI dashboard |

---

## 🏛️ Architecture

EventFlow is built following **Clean Architecture** with a strict separation of concerns across four layers:

```
┌──────────────────────────────────────────────────────┐
│                    Presentation                       │
│          EventFlow.API   |   EventFlow.MVC            │
└─────────────────────┬────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────────────┐
│                   Application                         │
│     Services, Interfaces, DTOs, Validators            │
└─────────────────────┬────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────────────┐
│                     Domain                            │
│          Entities, Domain Constants                   │
└─────────────────────┬────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────────────┐
│                 Infrastructure                        │
│    Repositories, UnitOfWork, EF DbContext, Seed       │
└──────────────────────────────────────────────────────┘
```

**Key principles applied:**
- Dependency Inversion — all services depend on interfaces, not concrete implementations
- Repository Pattern + Unit of Work — data access is fully abstracted
- Service Layer — all business logic lives in the Application layer, not in controllers
- CQRS-lite — separate request/response models for every use case

---

## 🛠️ Tech Stack

### Backend
| Technology | Usage |
|-----------|-------|
| **ASP.NET Core 8** | Web framework (API + MVC) |
| **Entity Framework Core 8** | ORM / database access |
| **SQL Server** | Relational database |
| **ASP.NET Core Identity** | User management, password hashing, roles |
| **JWT Bearer Authentication** | Stateless auth for the API |
| **Cookie Authentication** | Session-based auth for MVC |
| **FluentValidation** | Request model validation |
| **Mapster** | Object mapping (DTO ↔ Entity) |
| **Serilog** | Structured logging (Console + File sinks) |
| **NCrontab** | Cron expression parsing for background workers |
| **Swashbuckle / Swagger** | API documentation |
| **Asp.Versioning** | API versioning |
| **X.PagedList** | Server-side pagination |

### Testing
| Technology | Usage |
|-----------|-------|
| **xUnit** | Test framework |
| **Moq** | Mocking framework |
| **coverlet** | Code coverage collection |

### Health Monitoring
| Technology | Usage |
|-----------|-------|
| **AspNetCore.HealthChecks.SqlServer** | SQL Server health probe |
| **AspNetCore.HealthChecks.UI** | Visual health dashboard |

---

## ✨ Features

### User Features
-  **Register & Login** — full authentication with email + password
-  **Browse Events** — paginated list of active upcoming events
-  **Book Tickets** — reserve tickets that expire after a configurable window
-  **Purchase Tickets** — confirm a booking to make it permanent
-  **Cancel Bookings** — cancel unpurchased reservations and restore tickets
-  **View Cart & Orders** — see pending bookings and purchase history
-  **Create Events** — any authenticated user can submit events for approval
-  **Edit Events** — event owners can edit within a configurable time window

### Admin Features
-  **Manage Users** — view all users, promote to Moderator, revoke Moderator role
-  **Approve Events** — activate pending events submitted by users
-  **Global Settings** — configure system-wide values (ticket limits, expiry hours, edit windows)

### Moderator Features
-  **Approve Events** — same event approval capability as Admin

### System Features
-  **Expired Booking Cleanup** — cron worker automatically removes expired bookings and restores tickets
-  **Event Auto-Deactivation** — cron worker deactivates events whose end time has passed
-  **Localization** — error and validation messages in **English** and **Georgian (ka-GE)**
-  **Health Checks** — liveness and readiness endpoints + dashboard UI
-  **API Versioning** — versioned API with Swagger per-version documentation
-  **Structured Logging** — request logging, security warnings, audit logs via Serilog

---

## 📁 Project Structure

```
EventFlow/
├── EventFlow.API/               # REST API (JWT auth, Swagger, versioning)
│   ├── Controllers/             # AccountController, EventsController,
│   │                            # BookingController, GlobalSettingsController
│   ├── Middlewares/             # GlobalExceptionMiddleware, CultureMiddleware
│   ├── infrastructures/
│   │   ├── JWT/                 # JWTHelper, JWTConfiguration
│   │   └── Extensions/          # Authentication extension methods
│   └── VersionSwagger/          # Swagger versioning support
│
├── EventFlow.MVC/               # Razor MVC application (Cookie auth)
│   ├── Controllers/             # AccountController, HomeController,
│   │                            # EventController, AdminController
│   ├── Models/                  # ViewModels
│   └── Views/                   # Razor views (Bootstrap 5)
│
├── EventFlow.Application/       # Business logic layer
│   ├── Bookings/                # BookingService + interfaces + DTOs + validators
│   ├── Events/                  # EventService + interfaces + DTOs + validators
│   ├── Users/                   # UserService + interfaces + DTOs + validators
│   ├── GlobalSettings/          # GlobalSettingsService + cache + DTOs
│   ├── Exceptions/              # NotFoundException, BadRequestException, ForbiddenException
│   └── Localization/            # .resx files (en, ka-GE)
│
├── EventFlow.Domain/            # Domain entities and constants
│   ├── Bookings/                # Booking entity
│   ├── Events/                  # Event entity
│   ├── Users/                   # User entity (extends IdentityUser<int>)
│   ├── GlobalSettings/          # GlobalSettings entity
│   └── Constants/               # GlobalSettingsKeys
│
├── EventFlow.Infrastructure/    # EF repositories and unit of work
│   ├── Bookings/                # BookingRepository
│   ├── Events/                  # EventRepository
│   ├── Users/                   # UserRepository
│   └── GlobalSettings/          # GlobalSettingsRepository
│
├── EventFlow.Persistence/       # DbContext, EF config, migrations, seed
│   ├── Context/                 # ApplicationDbContext
│   ├── Configuration/           # Fluent API entity configs
│   ├── Migrations/              # EF Core migrations
│   └── Seed/                    # DatabaseSeeder (roles, users, events, settings)
│
├── EventFlow.Workers/           # Background worker host
│   └── BackgroundWorkers/       # CronTabExpiredBookingWorker,
│                                # CronTabExpiredEventWorker
│
├── HealthChecker/               # Standalone health check UI dashboard
│
└── Eventflow.Application.Tests/ # Unit tests (xUnit + Moq)
    ├── BookingsTest/
    ├── EventsTest/
    ├── GlobalSettings/
    └── UserTests/
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (local or Docker)

### 1. Clone the repository

```bash
git clone https://github.com/your-username/EventFlow.git
cd EventFlow
```

### 2. Configure the connection string

Update `appsettings.json` in both `EventFlow.API` and `EventFlow.MVC`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EventFlow;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Apply migrations & seed the database

Migrations run automatically on startup via `DatabaseSeeder.InitializeAsync`. The seeder creates:
- Roles: `Admin`, `Moderator`, `User`
- Default users (see [Database Seeding](#-database-seeding))
- Global settings
- Sample events

### 4. Run the API

```bash
cd EventFlow.API
dotnet run
```

Swagger UI will be available at: `https://localhost:7052/swagger`

### 5. Run the MVC app

```bash
cd EventFlow.MVC
dotnet run
```

Browse to: `http://localhost:5190`

### 6. Run the background workers (optional)

```bash
cd EventFlow.Workers
dotnet run
```

### 7. Run the health check dashboard (optional)

```bash
cd HealthChecker
dotnet run
```

Browse to: `https://localhost:7095/healthchecks-ui`

---

## 📡 API Endpoints

All API routes are versioned under `/api/v1/`.

### Account

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/account/register` | ❌ | Register a new user |
| `POST` | `/api/v1/account/login` | ❌ | Login and receive JWT token |
| `GET` | `/api/v1/account/users` | Admin | Get all users |
| `GET` | `/api/v1/account/users/{id}` | Admin | Get user by ID |
| `PUT` | `/api/v1/account/users/{id}/moderator` | Admin | Assign Moderator role |

### Events

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/v1/events` | ❌ | Get paginated active events |
| `GET` | `/api/v1/events/{id}` | ✅ | Get event by ID |
| `POST` | `/api/v1/events` | ✅ | Create a new event |
| `PUT` | `/api/v1/events/{id}` | ✅ (owner) | Update own event |
| `PUT` | `/api/v1/events/{id}/activate` | Admin | Activate a pending event |
| `DELETE` | `/api/v1/events/{id}` | Admin / Moderator | Delete an event |

### Bookings

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/booking` | ✅ | Create a booking (reserve tickets) |
| `PUT` | `/api/v1/booking/{id}/buy` | ✅ (owner) | Purchase a booking |
| `DELETE` | `/api/v1/booking/{id}/cancel` | ✅ (owner) | Cancel a booking |

### Global Settings

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/v1/globalsettings` | Admin | Create a setting |
| `PUT` | `/api/v1/globalsettings/{id}` | Admin | Update a setting |

---

## 🔐 Roles & Permissions

| Action | User | Moderator | Admin |
|--------|------|-----------|-------|
| Browse events | ✅ | ✅ | ✅ |
| Create event | ✅ | ✅ | ✅ |
| Edit own event (within time window) | ✅ | ✅ | ✅ |
| Book / buy tickets | ✅ | ✅ | ✅ |
| Cancel own booking | ✅ | ✅ | ✅ |
| Approve pending events | ❌ | ✅ | ✅ |
| Delete any event | ❌ | ✅ | ✅ |
| Manage users / assign roles | ❌ | ❌ | ✅ |
| Configure global settings | ❌ | ❌ | ✅ |

---

## ⚙️ Global Settings

The following settings are configurable at runtime by Admins:

| Key | Default | Description |
|-----|---------|-------------|
| `BookingExpirationHours` | `1` | Hours before an unpurchased booking expires |
| `EventEditAllowedDays` | `3` | Days after creation during which an event can be edited |
| `MaxTicketPerUser` | `5` | Maximum tickets one user can book per event |

Settings are cached in-memory for 1 hour and invalidated immediately on update.

---

## ⏱️ Background Workers

Two cron-based background workers run in `EventFlow.Workers` using **NCrontab**:

### `CronTabExpiredBookingWorker`
- **Schedule:** every 5 seconds (configurable)
- **Action:** finds all bookings where `ExpirationTime <= DateTime.Now` and `IsPurchased == false`, restores the ticket count to the event, and deletes the bookings

### `CronTabExpiredEventWorker`
- **Schedule:** every 5 seconds (configurable)
- **Action:** finds all active events where `EndTime < DateTime.Now` and sets `IsActive = false`

Both workers use `IServiceScopeFactory` to correctly resolve scoped services (EF DbContext) inside the singleton worker host.

---

## 🏥 Health Checks

### API Health Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /quickhealth` | Liveness probe — always returns healthy if the app is running |
| `GET /health` | Full readiness probe — includes SQL Server connectivity check |

### Health Dashboard

The `HealthChecker` project hosts an **AspNetCore.HealthChecks.UI** dashboard that polls the API's `/health` endpoint every 5 seconds and provides a visual status page.

Configure the target API URL in `HealthChecker/appsettings.json`:

```json
"HealthChecksUI": {
  "HealthChecks": [
    {
      "Name": "EventFlow API",
      "Uri": "https://localhost:7052/health"
    }
  ]
}
```

---

## 🌍 Localization

Error messages and validation messages are fully localized using `.resx` resource files.

| Culture | File |
|---------|------|
| English (default) | `ErrorMessages.resx`, `ValidationMessages.resx` |
| Georgian | `ErrorMessages.ka-GE.resx`, `ValidationMessages.ka-GE.resx` |

The active culture is resolved per-request from the `Accept-Language` HTTP header via `CultureMiddleware`. Supported values: `en-US`, `ka-GE`.

---

## 🧪 Testing

The `Eventflow.Application.Tests` project contains unit tests for all three core services using **xUnit** and **Moq**.

### Run all tests

```bash
cd Eventflow.Application.Tests
dotnet test
```

### Test coverage

| Service | Tests |
|---------|-------|
| `BookingService` | CancelAsync, CreateAsync, BuyAsync, CleanupExpiredBookingsAsync, GetUserBookingsAsync |
| `EventService` | GetPagedVisibleAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync, DeactivateEndedEventsAsync, ActivateEvent, GetPendingEvents, GetEventsByUserIdAsync |
| `GlobalSettingsService` | GetByKeyAsync (cache hit, cache miss, key not found), UpdateAsync, CreateAsync |
| `UserService` | AssignModeratorRoleAsync, RemoveModeratorRoleAsync, GetAllUsersAsync, GetByIdAsync, AuthenticationAsync, RegisterAsync |

All tests use **fixture-based setup** to share mock configuration and keep individual test methods focused.

---

## 🌱 Database Seeding

On first run, `DatabaseSeeder` automatically seeds:

### Roles
- `Admin`, `Moderator`, `User`

### Default Users

| Email | Password | Role |
|-------|----------|------|
| `admin@eventflow.ge` | `AdminPass123!` | Admin |
| `david.moderator@gmail.com` | `ModPass123!` | Moderator |
| `nino.diasamidze@gmail.com` | `UserPass123!` | User |
| `sandro.tech@gmail.com` | `UserPass123!` | User |

### Global Settings
- `BookingExpirationHours`: 1
- `EventEditAllowedDays`: 3
- `MaxTicketPerUser`: 5

### Sample Events
- Jazz Evening at Mushtaidi Park *(Active)*
- .NET Architecture Masterclass *(Active)*
- Bakuriani Winter Festival 2026 *(Pending approval)*

---
