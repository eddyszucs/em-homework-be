# Clinic Backend

ASP.NET Core 8 Web API for a clinic management system. Manages patients, doctors, waitlists, diagnoses, and audit logging with JWT authentication, SignalR real-time notifications, and PostgreSQL persistence.

## Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with WSL2 backend
- .NET 8 SDK (for local development without Docker)
- `curl` (included in the backend container for health checks)

### Start Everything with One Command

```bash
cd backend
docker --context desktop-linux compose up -d
```

Wait ~10 seconds for the database to initialise, then verify:

```bash
curl http://localhost:5000/health
```

Expected: `{"status":"Healthy"}`

The Swagger UI is available at [http://localhost:5000/swagger](http://localhost:5000/swagger).

### Stop

```bash
docker --context desktop-linux compose down      # keep data volume
docker --context desktop-linux compose down -v  # wipe data volume
```

### Running Tests

```bash
cd backend

# All tests
dotnet test ClinicBackend.Tests/ClinicBackend.Tests.csproj

# Unit tests only
dotnet test ClinicBackend.Tests/ClinicBackend.Tests.csproj --filter "Category=Unit"

# Integration tests only
dotnet test ClinicBackend.Tests/ClinicBackend.Tests.csproj --filter "Category=Integration"
```

## What We Built

### Architecture

```
backend/
├── ClinicBackend/                 # Main API project
│   ├── Controllers/               # Auth, Patients, Waitlist, Diagnosis, AuditLog
│   ├── Services/                  # Business logic (Auth, Patient, Waitlist, Audit, Notification)
│   ├── Models/                    # Entities, DTOs, Enums, State Machine
│   ├── Data/                      # ClinicDbContext, DbSeeder
│   ├── Validators/                # FluentValidation rules (TAJ number, patient name)
│   ├── Middleware/                # AuditLoggingMiddleware
│   ├── Hubs/                      # SignalR ClinicNotificationHub
│   ├── Migrations/               # EF Core PostgreSQL migrations
│   └── Program.cs                 # App bootstrap, DI, middleware pipeline
├── ClinicBackend.Tests/           # xUnit test project
│   ├── Integration/              # WebApplicationFactory-based integration tests
│   └── Unit/                      # Service + validator unit tests
└── docker-compose.yml             # PostgreSQL + backend containers
```

### API Endpoints

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| `POST` | `/api/auth/login` | Public | Login → JWT token |
| `POST` | `/api/auth/refresh` | Public | Refresh JWT |
| `GET` | `/api/patients` | Assistant | List all patients |
| `GET` | `/api/patients/{id}` | Assistant, Doctor | Get patient by ID |
| `POST` | `/api/patients` | Assistant | Create patient (Recorded) |
| `PUT` | `/api/patients/{id}` | Assistant | Update patient |
| `DELETE` | `/api/patients/{id}` | Assistant | Soft-delete patient |
| `GET` | `/api/waitlist` | Doctor | Get doctor's waitlist |
| `POST` | `/api/waitlist/{patientId}/assign` | Assistant | Assign patient to doctor → Waiting |
| `POST` | `/api/waitlist/{patientId}/call` | Doctor | Call patient → InProgress |
| `POST` | `/api/waitlist/{patientId}/release` | Doctor | Release patient → Done |
| `GET` | `/api/diagnoses?patientId=` | Assistant, Doctor | List diagnoses for patient |
| `POST` | `/api/diagnoses` | Doctor | Add diagnosis |
| `GET` | `/api/audit?patientId=&userId=&from=&to=` | Assistant | Query audit logs |
| `GET` | `/hubs/clinic` | * (ws) | SignalR hub for real-time notifications |

### Patient State Machine

```
Recorded → Waiting → InProgress → Done
```

- Only an **Assistant** can assign a patient to a doctor (→ `Waiting`)
- Only the **assigned Doctor** can call their patient (→ `InProgress`) or release them (→ `Done`)
- Soft-delete is implemented via `IsDeleted` query filter — deleted patients are excluded from all queries but remain in the database

### Authentication

JWT Bearer tokens with a 60-minute expiry. Include as `Authorization: Bearer <token>` header.

Seed users (created on first startup via `DbSeeder`):

| Username | Password | Role |
|----------|----------|------|
| `assistent1` | `Asst123!` | Assistant |
| `assistent2` | `Asst123!` | Assistant |
| `dr_kovacs` | `Doc123!` | Doctor (Belgyógyász) |
| `dr_nagy` | `Doc123!` | Doctor (Bőrgyógyász) |
| `dr_szabo` | `Doc123!` | Doctor (Szemész) |

### Real-time Notifications

SignalR hub at `/hubs/clinic`. Clients receive events when patient status changes, diagnoses are added, or patients are assigned to doctors.

### Audit Logging

Every create/update/delete on patients and diagnoses is recorded with user ID, timestamp, and change details. Queried via `/api/audit`.

### Tests — 68 Total

- **54 unit tests** — service logic, validators, state machine transitions
- **14 integration tests** — full HTTP request/response through the pipeline with an in-memory database

## Tech Stack

- **ASP.NET Core 8** — Web API framework
- **Entity Framework Core 8** with **Npgsql** (PostgreSQL)
- **JWT Bearer** authentication
- **FluentValidation** for input validation
- **Serilog** with PostgreSQL + Console sinks
- **SignalR** for real-time notifications
- **BCrypt** for password hashing
- **Swashbuckle** (Swagger/OpenAPI)
- **xUnit** + **Moq** + **WebApplicationFactory** for testing

## If We Had More Time

- **Repository pattern** — abstract `DbContext` behind `IRepository<T>` for cleaner unit testability and potential polyglot persistence
- **Refresh token rotation** — store refresh tokens in the DB with family tracking to detect token reuse attacks
- **API versioning** — `/api/v1/...` URL versioning to safely evolve the contract without breaking clients
- **Paginated list endpoints** — replace `List<T>` returns with cursor-based pagination to handle large patient sets efficiently
- **Polished SignalR** — typed strongly-typed hub events, graceful client reconnect, and a demo UI to visualise notifications
- **Admin role and dashboard** — An admin panel for managing users (creating assistants/doctors) and viewing audit logs.
- **Integration test database per test** — `CustomWebApplicationFactory` currently shares a database per test class fixture; parallelised test runs could benefit from a fresh in-memory DB per test method