# Loan Management System

Full-stack loan management app. .NET 8.0 backend with JWT authentication, Angular 19 frontend, SQL Server, Docker.

---

## Quick Start

```bash
# 1. Start backend + database
docker compose up

# 2. In another terminal, start frontend
cd frontend
npm install
npm start
```

Backend → http://localhost:5000/swagger  
Frontend → http://localhost:4200  
Login → `admin` / `admin123`

---

## API Endpoints

All `/loans` endpoints require JWT Bearer token.

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/auth/login` | Login | No |
| GET | `/auth/me` | Current user info | Yes |
| POST | `/loans` | Create a loan | Yes |
| GET | `/loans` | List all loans | Yes |
| GET | `/loans/{id}` | Loan details | Yes |
| POST | `/loans/{id}/payment` | Make a payment | Yes |

### Request / Response examples

```
POST /loans
{
  "amount": 15000.00,
  "applicantName": "Jane Smith"
}
```

```
POST /loans/1/payment
{
  "amount": 5000.00
}
```

```
GET /loans → 200
[
  {
    "id": 1,
    "amount": 25000.00,
    "currentBalance": 18750.00,
    "applicantName": "John Doe",
    "status": "active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null
  }
]
```

Loan statuses: `active` | `paid`. A loan becomes `paid` when `currentBalance` reaches 0.

---

## Manual Setup

### Backend

```bash
cd backend/src/Fundo.Applications.WebApi
dotnet restore
dotnet run
```

Needs SQL Server on localhost:1433. The connection string is in `appsettings.json`.

### Frontend

```bash
cd frontend
npm install
npm start
```

Uses `http://localhost:5000` as API base (configured in `src/environments/`).

### Tests

```bash
# Backend (18 tests: 10 unit + 8 integration)
cd backend/src
dotnet test

# Frontend
cd frontend
npm test
```

The integration tests use an in-memory database and do not require a real SQL Server.

---

## Architecture

```
take-home-test/
├── backend/src/
│   ├── Fundo.Applications.WebApi/
│   │   ├── Controllers/        # AuthController, LoanManagementController
│   │   ├── Services/           # IAuthService, AuthService, ILoanService, LoanService
│   │   ├── Data/               # LoanDbContext, DbInitializer
│   │   ├── Models/             # Loan, User
│   │   ├── DTOs/               # CreateLoanDto, LoanDto, LoginDto, PaymentDto
│   │   ├── Constants/          # LoanConstants (status strings, error messages)
│   │   ├── Program.cs          # Host builder + global error handler
│   │   ├── Startup.cs          # DI, JWT, CORS, Swagger config
│   │   └── appsettings.json
│   └── Fundo.Services.Tests/
│       ├── Unit/               # 10 unit tests (mocked LoanService)
│       └── Integration/        # 8 integration tests (WebApplicationFactory + InMemory DB)
├── frontend/src/
│   ├── app/
│   │   ├── guards/auth.guard.ts          # Functional guard (CanActivateFn)
│   │   ├── interceptors/auth.interceptor  # Functional interceptor (HttpInterceptorFn)
│   │   ├── pages/login/                   # Login form with validation
│   │   ├── pages/dashboard/               # Loan table + stats grid
│   │   ├── services/                      # AuthService, LoanService
│   │   ├── app.config.ts                  # HttpClient + interceptors
│   │   └── app.routes.ts                  # Routes with authGuard
│   ├── styles/                            # Design system (5 partials)
│   └── environments/                      # API base URL per environment
├── .github/workflows/ci.yml               # GitHub Actions (dotnet build + test)
├── docker-compose.yml                     # SQL Server 2022 + API
└── README.md
```

### Design Decisions

- **Service layer** separates business logic from controllers, enabling unit tests with Moq
- **DTOs** decouple API contracts from entity models
- **AsNoTracking()** on read queries reduces EF Core overhead
- **BCrypt (work factor 12)** for password hashing, not plaintext
- **Multi-stage Dockerfile** keeps the final image minimal (runtime only)
- **Healthcheck in docker-compose** ensures the API waits for SQL Server before starting
- **Functional guards and interceptors** (Angular 15+) avoid class boilerplate
- **Standalone components** (Angular 14+) without NgModules
- **Design tokens** (`_tokens.scss`) keep colors, spacing, and typography consistent

### Authentication Flow

1. User logs in at `/login` with username + password
2. Backend validates credentials (BCrypt), returns JWT (8h expiry)
3. Frontend stores token in `localStorage`
4. `authInterceptor` attaches `Authorization: Bearer <token>` to every HTTP request
5. `authGuard` prevents access to `/dashboard` without a valid token
6. Backend `[Authorize]` attribute secures all `/loans` endpoints
7. Logout clears the token and redirects to `/login`

### What's Included (Beyond Requirements)

- JWT authentication and authorization
- GitHub Actions CI pipeline (build + test on push/PR)
- Structured logging with ILogger
- Dark theme UI with glassmorphism, animations, responsive grid
- Loading, error, and empty states in the frontend
- Design system with tokens, typography, component styles

### What's Not Included (Would Add With More Time)

- FluentValidation for input validation
- Serilog for structured log output
- Pagination, filtering, and sorting on GET /loans
- Frontend forms for creating loans and making payments
- Angular unit tests for components and services
- Refresh token mechanism
- API versioning

---

## Seed Data

On first startup, the database is seeded with:

| Applicant | Amount | Balance | Status |
|-----------|--------|---------|--------|
| John Doe | $25,000 | $18,750 | active |
| Jane Smith | $15,000 | $0 | paid |
| Robert Johnson | $50,000 | $32,500 | active |
| Emily Williams | $10,000 | $0 | paid |
| Michael Brown | $75,000 | $72,000 | active |

Default user: `admin` / `admin123`

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 8.0, C# |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2022 |
| Auth | JWT (System.IdentityModel.Tokens.Jwt) |
| Testing | xUnit, Moq, WebApplicationFactory |
| CI | GitHub Actions |
| Frontend | Angular 19, TypeScript |
| UI | Angular Material, SCSS |
| Container | Docker, Docker Compose |