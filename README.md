# Loan Management System

Full-stack loan management app. .NET 8.0 backend with JWT authentication, Angular 19 frontend, SQL Server, Docker.

---

## Quick Start

```bash
# 1. Start backend + database
docker compose up --build

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

All `/loans` endpoints require JWT (delivered via httpOnly cookie after login).

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/auth/login` | Login | No |
| GET | `/auth/me` | Current user info | Yes |
| POST | `/auth/logout` | Logout | No |
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

On startup, the database is automatically created and seeded. If the DB is not yet available, the API retries up to 10 times with a 5-second delay (configured in Program.cs).

### Frontend

```bash
cd frontend
npm install
npm start
```

Uses `http://localhost:5000` as API base (configured in `src/environments/`).

### Tests

```bash
# Backend (20 tests: 11 unit + 9 integration)
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
├── .editorconfig                     # Editor formatting rules (2-space indent, 4 for C#)
├── .env.example                      # Environment variable template
├── .gitignore
├── docker-compose.yml                # SQL Server 2022 + API containers
├── .github/workflows/ci.yml          # GitHub Actions (dotnet build + test)
├── .vscode/
│   ├── launch.json                   # VS Code debug configuration
│   └── tasks.json                    # VS Code build tasks
│
├── backend/src/
│   ├── src.sln                       # .NET solution file
│   ├── .dockerignore
│   ├── Fundo.Applications.WebApi/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs     # POST login, GET me, POST logout
│   │   │   └── LoanManagementController.cs  # CRUD loans + payments
│   │   ├── Services/
│   │   │   ├── IAuthService.cs       # Auth interface
│   │   │   ├── AuthService.cs        # BCrypt verify + JWT generation (8h expiry)
│   │   │   ├── ILoanService.cs       # Loan interface
│   │   │   └── LoanService.cs        # Business logic + EF Core queries
│   │   ├── Data/
│   │   │   ├── LoanDbContext.cs      # EF Core context + Fluent API config
│   │   │   └── DbInitializer.cs      # Seed data + admin user (BCrypt work factor 12)
│   │   ├── Models/
│   │   │   ├── Loan.cs               # Loan entity (RowVersion for concurrency)
│   │   │   └── User.cs               # User entity (unique username/email)
│   │   ├── DTOs/
│   │   │   ├── CreateLoanDto.cs      # Validation: amount 100–1,000,000, name 2–200 chars
│   │   │   ├── LoanDto.cs            # Read-only loan representation
│   │   │   ├── LoginDto.cs           # Username 3–50, password 6–100
│   │   │   └── PaymentDto.cs         # Amount 0.01–1,000,000
│   │   ├── Constants/
│   │   │   └── LoanConstants.cs      # Status strings (active/paid) + error messages
│   │   ├── Program.cs                # Host builder + DB retry logic (10 retries, 5s delay)
│   │   ├── Startup.cs                # DI, JWT, CORS, Swagger, rate limiting (legacy pattern)
│   │   ├── appsettings.json          # Connection string + JWT config
│   │   ├── Dockerfile                # Multi-stage build (SDK → runtime)
│   │   └── Fundo.Applications.WebApi.csproj  # net8.0, EF Core 8, BCrypt, Swashbuckle, JWT
│   └── Fundo.Services.Tests/
│       ├── Unit/
│       │   └── LoansControllerTests.cs    # 11 unit tests (Moq + xUnit)
│       ├── Integration/
│       │   ├── ConcurrencyTests.cs        # 2 integration tests (RowVersion verification)
│       │   └── Fundo.Applications.WebApi/
│       │       └── Controllers/
│       │           └── LoanManagementControllerTests.cs  # 7 integration tests (HTTP + InMemory DB)
│       └── Fundo.Services.Tests.csproj    # xUnit, Moq, FluentAssertions, coverlet
│
└── frontend/
    ├── angular.json                  # Angular CLI config (SCSS, budgets)
    ├── package.json                  # Angular 19, Material 19, TypeScript 5.7
    ├── tsconfig.json                 # Strict mode, ES2022, bundler resolution
    ├── tsconfig.app.json
    ├── tsconfig.spec.json
    ├── .gitignore
    ├── public/
    │   └── favicon.ico
    └── src/
        ├── index.html                # Google Fonts (Cormorant Garamond + Inter + Material Icons)
        ├── main.ts                   # bootstrapApplication (standalone)
        ├── styles.scss               # Imports all 5 partials
        ├── app/
        │   ├── app.component.ts      # Root shell (standalone, RouterOutlet only)
        │   ├── app.component.html    # <router-outlet>
        │   ├── app.component.scss    # Empty (styles in pages + global partials)
        │   ├── app.config.ts         # provideRouter + provideHttpClient + authInterceptor
        │   ├── app.routes.ts         # / → /dashboard, /login (loginGuard), /dashboard (authGuard)
        │   ├── constants/
        │   │   └── loan.constants.ts # Status enums (active, paid, approved, pending, etc.)
        │   ├── guards/
        │   │   ├── auth.guard.ts     # Functional CanActivateFn: authGuard + loginGuard
        │   │   └── auth.guard.spec.ts
        │   ├── interceptors/
        │   │   ├── auth.interceptor.ts      # Functional HttpInterceptorFn: withCredentials + 401 redirect
        │   │   └── auth.interceptor.spec.ts
        │   ├── pages/
        │   │   ├── login/
        │   │   │   ├── login.component.ts      # Form with loading, error handling, password toggle
        │   │   │   ├── login.component.html    # Glassmorphism card, SVG icons, demo credentials hint
        │   │   │   └── login.component.scss    # Ambient glow, glass-panel-premium, responsive
        │   │   └── dashboard/
        │   │       ├── dashboard.component.ts       # Stats grid, table, payment dialog, logout
        │   │       ├── dashboard.component.html     # Mat-table, status badges, avatar initials
        │   │       ├── dashboard.component.scss     # 409 lines: sticky header, stat cards, animations
        │   │       └── dashboard.component.spec.ts  # 12 unit tests (mocked services)
        │   └── services/
        │       ├── auth.service.ts       # Login/logout, BehaviorSubject<boolean>, waitForAuthCheck()
        │       ├── auth.service.spec.ts
        │       └── loan.service.ts       # getLoans, makePayment, createLoan (all withCredentials)
        ├── environments/
        │   ├── environment.ts             # apiBase: http://localhost:5000
        │   └── environment.development.ts # apiBase: http://localhost:5000
        └── styles/
            ├── _tokens.scss          # Design tokens: colors, spacing (4px base), radii, shadows, fonts
            ├── _reset.scss           # CSS reset (box-sizing, margin, body bg/font)
            ├── _typography.scss      # Heading font (Cormorant Garamond), body (Inter), tabular-nums
            ├── _components.scss      # Buttons (.btn--primary, .btn--ghost), forms, alerts, surfaces
            └── _material.scss        # Mat-table dark theme overrides, dialog reset
```

### Technical Decisions

- Input Validation: Added guards in the service layer to prevent negative payment amounts, ensuring financial integrity.
- Concurrency Control: Implemented optimistic concurrency using `RowVersion` on the Loan entity. This prevents two users from updating the same loan balance simultaneously, which is critical for financial transactions.
- Audit Trail: Intentionally excluded `Delete` functionality. In financial applications, maintaining a complete history of all records is mandatory for audit purposes; therefore, loans are never deleted, only updated.
- Security: Used BCrypt for password hashing and httpOnly cookies for session management to protect against XSS (Cross-Site Scripting) attacks, which is a more secure approach than storing tokens in LocalStorage.
- Rate Limiting: Sliding window rate limiter (5 requests per minute per IP) on the login endpoint to prevent brute force attacks.
- DB Retry Logic: Program.cs retries database connection up to 10 times with a 5-second delay to handle container startup ordering (SQL Server may not be ready when the API starts).

### Design Decisions

- Service layer separates business logic from controllers, enabling unit tests with Moq
- DTOs decouple API contracts from entity models
- AsNoTracking() on read queries reduces EF Core overhead
- BCrypt (work factor 12) for password hashing, not plaintext
- Multi-stage Dockerfile keeps the final image minimal (runtime only)
- Healthcheck in docker-compose ensures the API waits for SQL Server before starting
- Functional guards and interceptors (Angular 15+) avoid class boilerplate
- Standalone components (Angular 14+) without NgModules
- Design tokens (_tokens.scss) keep colors, spacing, and typography consistent
- Optimistic Concurrency Control: Implemented `RowVersion` on the `Loan` entity and handled `DbUpdateConcurrencyException` in the service layer to prevent race conditions during concurrent payment processing.
- Data Validation: Utilized Entity Framework Core DataAnnotations for model validation directly at the entity level, keeping the API lightweight.
- Legacy Startup.cs pattern kept for test compatibility; should migrate to minimal hosting pattern for production.
- BehaviorSubject in AuthService for reactive auth status; components subscribe to `isLoggedIn$`.
- MatDialog for payment form to avoid blocking the main dashboard view.

### Authentication Flow

1. User logs in at `/login` with username + password
2. Backend validates credentials (BCrypt), returns JWT (8h expiry) in httpOnly cookie
3. Frontend uses httpOnly cookies for token storage (XSS protection)
4. `authInterceptor` uses `withCredentials: true` to include cookies in HTTP requests
5. `authGuard` prevents access to `/dashboard` without a valid token
6. `loginGuard` redirects already-authenticated users away from `/login` to `/dashboard`
7. Backend `[Authorize]` attribute secures all `/loans` endpoints
8. On 401 responses, the interceptor auto-logs out and redirects to `/login`
9. Logout clears the cookie via `POST /auth/logout` and redirects to `/login`

### What's Included (Beyond Requirements)

- JWT authentication and authorization with httpOnly cookies
- GitHub Actions CI pipeline (build + test on push/PR to main/master)
- Structured logging with ILogger
- Dark theme UI with glassmorphism, animations, responsive grid
- Loading, error, and empty states in the frontend
- Design system with 5 SCSS partials (tokens, reset, typography, components, material)
- Full Loan Lifecycle: End-to-end implementation of loan creation and payment processing, including business rules (preventing payments on already paid loans).
- Rate limiting on login endpoint (sliding window, 5 req/min per IP)
- DB retry logic for container startup ordering
- Password visibility toggle on login form
- Status badge system with color-coded categories (success, warning, danger, neutral)
- Avatar initials for loan applicants
- Live indicator in dashboard header
- Payment dialog with balance display and input validation

### Security Considerations

Note: This is a take-home test/challenge implementation. For production deployment, the following security measures should be implemented:

Hardcoded Values (Development Only):
- Connection string, JWT key, and admin credentials are currently hardcoded in appsettings.json and DbInitializer.cs
- In production, these should be loaded from environment variables or a secret management system (Azure Key Vault, AWS Secrets Manager, etc.)

Security Features Implemented:
- httpOnly cookies for JWT token storage (prevents XSS attacks)
- Rate limiting on login endpoint (prevents brute force attacks)
- BCrypt with work factor 12 for password hashing
- HSTS enabled for HTTPS enforcement
- CORS configured for localhost:4200
- Concurrency control with RowVersion to prevent race conditions
- Optimistic concurrency handling in loan payments
- Input validation via DataAnnotations on all DTOs

Production Recommendations:
- Use environment variables for all sensitive configuration
- Implement secret rotation policies
- Add API rate limiting globally
- Implement refresh token mechanism
- Add input sanitization and output encoding
- Enable security headers (CSP, X-Frame-Options, etc.)
- Implement audit logging for sensitive operations
- Migrate from legacy Startup.cs to minimal hosting pattern

### What's Not Included (Would Add With More Time)

- Serilog for structured log output
- Pagination, filtering, and sorting on GET /loans
- API versioning
- Refresh token mechanism
- Angular E2E tests (Playwright / Cypress)
- Dark/light theme toggle
- Internationalization (i18n)

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
| Auth | JWT (httpOnly cookies, 8h expiry) |
| Hashing | BCrypt (work factor 12) |
| API Docs | Swagger / Swashbuckle |
| Testing | xUnit, Moq, FluentAssertions, WebApplicationFactory |
| CI | GitHub Actions |
| Frontend | Angular 19, TypeScript 5.7 |
| UI | Angular Material 19, SCSS |
| Fonts | Cormorant Garamond (display) + Inter (UI) |
| Container | Docker, Docker Compose |