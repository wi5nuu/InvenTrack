# MASTER PROMPT — InvenTrack Enterprise
## For: Claude Code | Local Development | Senior .NET Portfolio Project

---

## ROLE & MISSION

You are a **Senior .NET Software Architect** with 10+ years of enterprise development experience. Your task is to build a complete, production-quality **Enterprise Inventory & Monitoring System** called **"InvenTrack Enterprise"** from scratch on the local machine.

This is a **portfolio project** designed to demonstrate mastery of:
- ETL pipelines, .NET ecosystem, Middleware, SDLC practices, Microsoft SQL Server
- REST API design, Docker containerization, CI/CD, OWASP security
- SSRS-style reporting, SMTP integration, microservices architecture concepts

**Every line of code must be production-grade, clean, well-commented, and follow enterprise best practices.**

---

## TECH STACK (MANDATORY — DO NOT DEVIATE)

| Layer | Technology |
|---|---|
| Backend Framework | ASP.NET Core 8 (LTS) |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Database | Microsoft SQL Server 2022 (via Docker) |
| Authentication | JWT Bearer Token + Refresh Token |
| API Documentation | Swagger / OpenAPI 3.0 (Swashbuckle) |
| Containerization | Docker + docker-compose |
| CI/CD | GitHub Actions |
| Email | MailKit (SMTP) |
| Reporting / Export | QuestPDF (PDF) + ClosedXML (Excel) |
| ETL / Data Import | CsvHelper + EPPlus (CSV & Excel import) |
| Testing | xUnit + FluentAssertions + Moq |
| Logging | Serilog (console + file sink) |
| Frontend | Razor Pages + Bootstrap 5 + Chart.js |
| Caching | IMemoryCache (in-memory, extensible to Redis) |
| Background Jobs | IHostedService + BackgroundService |

---

## PROJECT STRUCTURE

Create the solution with the following structure. Follow Clean Architecture / N-Tier pattern:

```
InvenTrack/
├── src/
│   ├── InvenTrack.Web/                    # ASP.NET Core MVC + Razor Pages (UI Layer)
│   │   ├── Controllers/
│   │   ├── Views/
│   │   ├── wwwroot/
│   │   ├── Middleware/
│   │   ├── Filters/
│   │   └── Program.cs
│   │
│   ├── InvenTrack.API/                    # REST API Project (separate from Web UI)
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   └── Program.cs
│   │
│   ├── InvenTrack.Application/            # Business Logic Layer
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   ├── DTOs/
│   │   └── Validators/
│   │
│   ├── InvenTrack.Domain/                 # Domain Entities & Core Business Rules
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Exceptions/
│   │
│   ├── InvenTrack.Infrastructure/         # Data Access, External Services
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Migrations/
│   │   │   └── Seed/
│   │   ├── Repositories/
│   │   ├── Services/                      # ETL, Email, Export, etc.
│   │   └── BackgroundJobs/
│   │
│   └── InvenTrack.Shared/                 # Shared models, constants, helpers
│       ├── Constants/
│       ├── Helpers/
│       └── Wrappers/
│
├── tests/
│   ├── InvenTrack.UnitTests/
│   └── InvenTrack.IntegrationTests/
│
├── docker/
│   ├── Dockerfile
│   ├── docker-compose.yml
│   └── docker-compose.override.yml
│
├── .github/
│   └── workflows/
│       └── ci-cd.yml
│
├── docs/
│   ├── architecture-diagram.md
│   ├── api-reference.md
│   └── setup-guide.md
│
├── scripts/
│   ├── seed-data.sql
│   └── stored-procedures.sql
│
├── .gitignore
├── README.md
└── InvenTrack.sln
```

---

## DATABASE SCHEMA

Create the following tables in SQL Server. Use EF Core Code-First Migrations AND also provide raw SQL scripts in `scripts/`.

### Tables:

```sql
-- 1. Users (Authentication)
Users: Id, Username, Email, PasswordHash, PasswordSalt, Role (Admin/Manager/Staff),
       RefreshToken, RefreshTokenExpiry, IsActive, CreatedAt, UpdatedAt

-- 2. Categories
Categories: Id, Name, Description, IsActive, CreatedAt, UpdatedAt

-- 3. Suppliers
Suppliers: Id, Name, ContactPerson, Email, Phone, Address, IsActive, CreatedAt, UpdatedAt

-- 4. Products (core inventory table)
Products: Id, SKU (unique), Name, Description, CategoryId (FK), SupplierId (FK),
          UnitPrice, CostPrice, StockQuantity, MinStockThreshold,
          Unit (pcs/kg/liter/etc), ImageUrl, IsActive, CreatedAt, UpdatedAt

-- 5. StockMovements (audit trail for all inventory changes)
StockMovements: Id, ProductId (FK), MovementType (IN/OUT/ADJUSTMENT/IMPORT),
                Quantity, QuantityBefore, QuantityAfter, Reference, Notes,
                UserId (FK), CreatedAt

-- 6. ImportJobs (ETL tracking)
ImportJobs: Id, FileName, FileType (CSV/Excel), TotalRows, SuccessRows, FailedRows,
            Status (Pending/Processing/Completed/Failed), ErrorLog, UserId (FK),
            StartedAt, CompletedAt, CreatedAt

-- 7. AlertLogs (email alert history)
AlertLogs: Id, ProductId (FK), AlertType (LowStock/OutOfStock),
           EmailSent, SentTo, SentAt, CreatedAt

-- 8. AuditLogs (system-wide audit)
AuditLogs: Id, UserId, Action, EntityName, EntityId, OldValues, NewValues,
           IpAddress, UserAgent, CreatedAt
```

### Stored Procedures (create in `scripts/stored-procedures.sql`):

1. `sp_GetInventorySummary` — total products, total stock value, low stock count, out of stock count
2. `sp_GetLowStockProducts` — products below MinStockThreshold with supplier info
3. `sp_GetStockMovementReport` — movement history with filters (date range, product, type)
4. `sp_GetTopMovingProducts` — top 10 most moved products in a date range
5. `sp_GetInventoryValuation` — full inventory valuation report (cost × qty per category)
6. `sp_UpdateStockQuantity` — atomic stock update with movement logging (transaction-safe)

---

## FEATURE IMPLEMENTATION — COMPLETE DETAILS

### FEATURE 1: Authentication & Authorization

**Implementation:**
- JWT Bearer authentication with 15-minute access token lifetime
- Refresh token stored in HttpOnly cookie (not localStorage) — OWASP compliant
- Role-based authorization: `[Authorize(Roles = "Admin")]`, `[Authorize(Roles = "Admin,Manager")]`
- Password hashing with BCrypt (min 12 rounds)
- Account lockout after 5 failed attempts (stored in cache)
- Endpoints: `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `POST /api/auth/register`

**Seed default users:**
```
Admin:   admin@inventrack.local / Admin@123456
Manager: manager@inventrack.local / Manager@123456
Staff:   staff@inventrack.local / Staff@123456
```

---

### FEATURE 2: Custom Middleware Pipeline

Create ALL of the following middleware in `InvenTrack.Web/Middleware/` and `InvenTrack.API/Middleware/`:

**a) RequestLoggingMiddleware.cs**
- Log every request: timestamp, method, path, query string, IP address, user agent
- Log every response: status code, elapsed time in ms
- Use Serilog structured logging
- Skip logging for health check endpoints

**b) GlobalExceptionMiddleware.cs**
- Catch ALL unhandled exceptions
- Return consistent JSON error response: `{ success: false, message: "...", errorCode: "...", traceId: "..." }`
- Log full exception details with Serilog (including stack trace)
- Never expose internal error details in production (check environment)
- Handle specific exception types: `NotFoundException`, `ValidationException`, `UnauthorizedException`, `ForbiddenException`

**c) RateLimitingMiddleware.cs**
- IP-based rate limiting using IMemoryCache
- Limits: 100 requests/minute for API, 200 requests/minute for Web
- Return HTTP 429 with `Retry-After` header when exceeded
- Whitelist: localhost, internal IPs

**d) AuditMiddleware.cs**
- Intercept POST/PUT/DELETE requests
- Log to AuditLogs table: user, action, entity, old/new values, IP, timestamp
- Use reflection to capture request/response body differences

**e) SecurityHeadersMiddleware.cs**
- Add all OWASP-recommended security headers:
  ```
  X-Content-Type-Options: nosniff
  X-Frame-Options: DENY
  X-XSS-Protection: 1; mode=block
  Strict-Transport-Security: max-age=31536000
  Content-Security-Policy: default-src 'self'
  Referrer-Policy: strict-origin-when-cross-origin
  Permissions-Policy: geolocation=(), microphone=(), camera=()
  ```

---

### FEATURE 3: REST API (InvenTrack.API project)

All APIs must follow REST conventions. All responses must use the wrapper:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null,
  "timestamp": "2025-01-01T00:00:00Z",
  "traceId": "abc123"
}
```

For paginated responses, add:
```json
{
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 150,
    "totalPages": 15
  }
}
```

**API Endpoints to implement:**

```
AUTH
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout

PRODUCTS
GET    /api/v1/products                    (paginated, filterable, searchable)
GET    /api/v1/products/{id}
POST   /api/v1/products                    [Admin, Manager]
PUT    /api/v1/products/{id}               [Admin, Manager]
DELETE /api/v1/products/{id}               [Admin only]
GET    /api/v1/products/low-stock          (below threshold)
PATCH  /api/v1/products/{id}/stock         (adjust stock, logs movement)

CATEGORIES
GET    /api/v1/categories
GET    /api/v1/categories/{id}
POST   /api/v1/categories                  [Admin]
PUT    /api/v1/categories/{id}             [Admin]
DELETE /api/v1/categories/{id}             [Admin]

SUPPLIERS
GET    /api/v1/suppliers
GET    /api/v1/suppliers/{id}
POST   /api/v1/suppliers                   [Admin, Manager]
PUT    /api/v1/suppliers/{id}              [Admin, Manager]
DELETE /api/v1/suppliers/{id}              [Admin]

STOCK MOVEMENTS
GET    /api/v1/stock-movements             (paginated, filter by product/type/date)
POST   /api/v1/stock-movements             (manual adjustment)

ETL / IMPORT
POST   /api/v1/import/upload               (multipart/form-data, CSV or Excel)
GET    /api/v1/import/jobs                 (list all import jobs)
GET    /api/v1/import/jobs/{id}            (job status + error log)
GET    /api/v1/import/template/csv         (download CSV template)
GET    /api/v1/import/template/excel       (download Excel template)

REPORTS
GET    /api/v1/reports/inventory-summary   (uses sp_GetInventorySummary)
GET    /api/v1/reports/low-stock           (uses sp_GetLowStockProducts)
GET    /api/v1/reports/movements           (uses sp_GetStockMovementReport)
GET    /api/v1/reports/top-products        (uses sp_GetTopMovingProducts)
GET    /api/v1/reports/valuation           (uses sp_GetInventoryValuation)
GET    /api/v1/reports/export/pdf          (full inventory PDF report)
GET    /api/v1/reports/export/excel        (full inventory Excel report)

DASHBOARD
GET    /api/v1/dashboard/summary           (summary cards data)
GET    /api/v1/dashboard/chart/movements   (7-day movement chart data)
GET    /api/v1/dashboard/chart/categories  (stock distribution by category)

HEALTH
GET    /health                             (health check endpoint)
```

Implement **API Versioning** using route prefix `/api/v1/`.
Add **Swagger UI** at `/swagger` with JWT authentication support in the UI.

---

### FEATURE 4: ETL Pipeline (Data Import)

This is the "Must Have ETL" skill demonstration. Implement a full ETL pipeline:

**CSV Import Flow:**
1. User uploads CSV file via web UI or API
2. System validates file size (max 10MB) and MIME type
3. Create `ImportJob` record with status = Pending
4. Hand off to `ImportBackgroundService` (IHostedService) via `Channel<ImportJob>`
5. Background service processes rows:
   - **Extract**: Read CSV using CsvHelper with custom mapping
   - **Transform**: Validate each row (SKU format, positive prices, valid category)
   - **Load**: Bulk upsert products (insert new, update existing by SKU)
   - Log each row result (success/fail with reason)
6. Update `ImportJob` with final status, success/fail counts, error log
7. Send email notification on completion (SMTP)

**CSV Template columns:**
```
SKU, ProductName, Category, Supplier, UnitPrice, CostPrice, InitialStock, MinStockThreshold, Unit, Description
```

**Excel Import:** Same flow but use EPPlus to read .xlsx files.

**Validation rules per row:**
- SKU: required, alphanumeric + dash, max 50 chars
- ProductName: required, max 200 chars
- UnitPrice: required, > 0, max 999999999
- CostPrice: required, > 0, must be <= UnitPrice
- InitialStock: required, >= 0, integer
- MinStockThreshold: required, >= 0, integer, < InitialStock
- Category: auto-create if not exists
- Supplier: auto-create if not exists

---

### FEATURE 5: SMTP Email Service

**Config (appsettings.json):**
```json
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "Username": "",
  "Password": "",
  "FromAddress": "inventrack@company.com",
  "FromName": "InvenTrack System"
}
```

**Email triggers:**
1. **Low Stock Alert**: When product stock drops below `MinStockThreshold`
   - Subject: `[ALERT] Low Stock: {ProductName} ({SKU})`
   - Include: current qty, threshold, supplier contact, link to product
2. **Import Completed**: When ETL job finishes
   - Subject: `[InvenTrack] Import Job #{JobId} Completed`
   - Include: total/success/failed counts, download error log link
3. **Daily Inventory Summary**: Triggered by background job at 07:00 AM
   - HTML email with table: total products, low stock count, total value
4. **New User Registration**: Welcome email with credentials

**Implementation:**
- Use MailKit library (NOT SmtpClient — it's obsolete)
- Create `IEmailService` interface and `EmailService` implementation
- HTML email templates stored in `src/InvenTrack.Infrastructure/EmailTemplates/`
- Use Razor engine to render email templates (RazorLight or Scriban)
- Queue emails via `Channel<EmailMessage>` to avoid blocking request thread
- Retry logic: 3 attempts with exponential backoff on failure

---

### FEATURE 6: Reporting & Export

**PDF Report (using QuestPDF):**
Create a professional-looking PDF inventory report that includes:
- Company header with logo placeholder and report date
- Executive Summary section: total products, total stock value, low stock count
- Category breakdown table: category, product count, total stock value
- Low Stock Products table: SKU, name, current stock, threshold, supplier
- Full Inventory table: all products with stock levels (color-coded rows)
- Footer: page numbers, generated timestamp, generated by user

**Excel Export (using ClosedXML):**
- Multiple worksheets: Summary, By Category, Full Inventory, Low Stock, Movements
- Auto-fit columns, freeze header row
- Conditional formatting: red fill for stock < threshold, yellow for stock < threshold × 1.5
- Summary sheet with formula-based totals

**Web UI Reports page:**
- Date range picker for filtering
- Report type selector
- Preview in browser (Chart.js charts)
- Export to PDF / Export to Excel buttons

---

### FEATURE 7: Web UI (Razor Pages / MVC)

Build a clean, professional admin dashboard using Bootstrap 5. Pages:

**Layout:** Sidebar navigation + top navbar with user info + breadcrumbs

**Pages to build:**
1. `/login` — Login form with validation
2. `/dashboard` — Summary cards + 2 charts (bar + pie) using Chart.js
3. `/products` — Datatable (server-side pagination/search/sort), Add/Edit modal, Delete confirm
4. `/products/import` — File upload dropzone, import history table, job status polling
5. `/categories` — CRUD table
6. `/suppliers` — CRUD table
7. `/stock-movements` — Read-only log with filters
8. `/reports` — Report builder: select type + date range + export buttons
9. `/users` — User management (Admin only)
10. `/profile` — Change password, view audit log

**UI Requirements:**
- Responsive (mobile-friendly)
- Form validation client-side (jQuery Validate) AND server-side
- Loading spinners on all async operations
- Toast notifications for success/error (Toastr.js)
- Confirm dialogs for destructive actions
- All tables must support: search, sort by column, pagination

---

### FEATURE 8: Security (OWASP Top 10 Compliance)

Document and implement defenses for the following OWASP Top 10 items. Add `// SECURITY:` comments in code where each defense is applied:

1. **A01 - Broken Access Control**: Role-based authorization on all endpoints, server-side checks, no IDOR vulnerabilities
2. **A02 - Cryptographic Failures**: BCrypt for passwords, HTTPS enforced, no sensitive data in logs, JWT secrets in environment variables (not appsettings)
3. **A03 - Injection**: Parameterized queries ONLY (EF Core + Dapper with parameters for stored procs), never string concatenation in SQL
4. **A04 - Insecure Design**: Principle of least privilege, input validation on all DTOs using FluentValidation
5. **A05 - Security Misconfiguration**: Security headers middleware, no default credentials in production, custom error pages (no stack traces)
6. **A06 - Vulnerable Components**: Use only pinned NuGet package versions, add note about running `dotnet list package --vulnerable`
7. **A07 - Auth Failures**: Account lockout, rate limiting on auth endpoints, refresh token rotation, secure cookie flags
8. **A08 - Software Integrity**: File upload validation (MIME type + magic bytes check, not just extension)
9. **A09 - Logging Failures**: Comprehensive Serilog logging, audit trail for all data changes
10. **A10 - SSRF**: Whitelist-based URL validation if any external URL fetching

---

### FEATURE 9: Testing

**Unit Tests (InvenTrack.UnitTests):**

Test the following with mocked dependencies using Moq:
- `ProductService`: CreateProduct, UpdateStock, GetLowStockProducts
- `ImportService`: ValidateRow (all validation rules), ProcessCsvRow
- `AuthService`: Login (valid credentials, invalid credentials, locked account)
- `EmailService`: Queue behavior, retry logic
- Middleware tests: RateLimiting behavior, GlobalException response format

**Integration Tests (InvenTrack.IntegrationTests):**

Use `WebApplicationFactory<Program>` with an in-memory SQLite database (swap SQL Server for tests):
- Auth flow: login → get token → access protected endpoint → refresh token → logout
- Product CRUD via API: create → read → update → delete
- Import flow: upload CSV → check job status → verify products created
- Report export: request PDF → verify non-empty response

**Test coverage target: minimum 70%**

---

### FEATURE 10: Docker & CI/CD

**Dockerfile** (multi-stage build):
```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InvenTrack.Web/InvenTrack.Web.csproj", "src/InvenTrack.Web/"]
# ... copy all project files
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InvenTrack.Web.dll"]
```

**docker-compose.yml** (run entire stack with one command):
```yaml
services:
  web:
    build: .
    ports: ["8080:8080", "8081:8081"]
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=...
    depends_on: [sqlserver]

  api:
    build:
      context: .
      dockerfile: docker/Dockerfile.api
    ports: ["5000:5000"]
    depends_on: [sqlserver]

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "InvenTrack@2024!"
      ACCEPT_EULA: "Y"
    ports: ["1433:1433"]
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./scripts:/docker-entrypoint-initdb.d

volumes:
  sqlserver_data:
```

**GitHub Actions CI/CD** (`.github/workflows/ci-cd.yml`):
```yaml
# Trigger on push to main and pull requests
# Jobs:
# 1. build-and-test: restore → build → run unit tests → run integration tests → report coverage
# 2. security-scan: run dotnet list package --vulnerable
# 3. docker-build: build Docker image → tag with commit SHA and 'latest'
# 4. deploy (manual trigger or on tag): push image to registry
```

---

### FEATURE 11: Configuration & Environment

**appsettings.json** (development defaults, safe to commit):
```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=InvenTrackDB;User Id=sa;Password=InvenTrack@2024!;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "REPLACE_WITH_ENV_VAR_IN_PRODUCTION",
    "Issuer": "InvenTrack",
    "Audience": "InvenTrack.Client",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "REPLACE_WITH_ENV_VAR",
    "Password": "REPLACE_WITH_ENV_VAR",
    "FromAddress": "noreply@inventrack.local",
    "FromName": "InvenTrack Enterprise"
  },
  "ImportSettings": {
    "MaxFileSizeMb": 10,
    "AllowedExtensions": [".csv", ".xlsx"],
    "UploadPath": "uploads/imports",
    "BatchSize": 100
  },
  "RateLimit": {
    "ApiRequestsPerMinute": 100,
    "WebRequestsPerMinute": 200,
    "AuthAttemptsBeforeLockout": 5,
    "LockoutMinutes": 15
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/inventrack-.txt", "rollingInterval": "Day" } }
    ]
  },
  "AlertSettings": {
    "DailySummaryTime": "07:00",
    "LowStockAlertRecipients": ["admin@inventrack.local"]
  }
}
```

Sensitive values in production must come from **environment variables**, never hardcoded.

---

### FEATURE 12: Seed Data

When running for the first time, automatically seed:
- 3 default users (Admin, Manager, Staff)
- 5 categories: Electronics, Office Supplies, Raw Materials, Finished Goods, Spare Parts
- 3 suppliers with full details
- 20 sample products across all categories with realistic data
- 30 stock movement records (historical data for charts)

---

### FEATURE 13: Repository Pattern

Implement the full Repository + Unit of Work pattern:

```csharp
// Generic repository
IRepository<T> where T : BaseEntity {
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedResult<T>> GetPagedAsync(PaginationParams params);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
}

// Unit of Work
IUnitOfWork {
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ISupplierRepository Suppliers { get; }
    IStockMovementRepository StockMovements { get; }
    IImportJobRepository ImportJobs { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

---

## README.md REQUIREMENTS

The README.md must include:
1. Project title + badge (build status from GitHub Actions)
2. Short description (2-3 sentences)
3. Architecture diagram (ASCII art or Mermaid diagram)
4. Tech stack table
5. Prerequisites (Docker Desktop, .NET 8 SDK, Git)
6. Quick Start (run with docker-compose in 3 commands)
7. Manual Setup (step-by-step without Docker)
8. API documentation link (Swagger URL)
9. Default credentials table
10. Feature list with OWASP compliance note
11. Project structure tree
12. How to run tests with coverage report
13. Screenshots section (placeholder)
14. License

---

## CODING STANDARDS (ENFORCE THROUGHOUT)

1. **Naming**: PascalCase for classes/methods, camelCase for parameters/variables, `_camelCase` for private fields
2. **Comments**: XML documentation on all public methods and classes. Inline `// SECURITY:` comments for security-critical code. `// OWASP A0X:` tags for OWASP-specific implementations
3. **Error handling**: Never use empty catch blocks. Always log exceptions. Use custom exception types
4. **Async/Await**: All I/O operations must be async. Never use `.Result` or `.Wait()` — use `await` only
5. **Dependency Injection**: Register all services in extension methods (`AddApplicationServices()`, `AddInfrastructureServices()`) — not directly in `Program.cs`
6. **Validation**: Use FluentValidation for all DTOs. Never trust client input
7. **Constants**: No magic strings or numbers — use constants or enums
8. **SOLID principles**: Each class has single responsibility. Depend on interfaces, not implementations

---

## EXECUTION ORDER

Build in this exact order to avoid dependency issues:

1. Create solution and all projects, add project references
2. Install all NuGet packages (list all packages with exact versions)
3. Build Domain layer (Entities, Enums, Exceptions, BaseEntity)
4. Build Shared layer (Wrappers, Constants, Helpers)
5. Build Infrastructure layer (DbContext, Migrations, Repositories, UnitOfWork)
6. Build Application layer (DTOs, Interfaces, Services, Validators)
7. Build API project (Controllers, Middleware, Program.cs)
8. Build Web project (Controllers, Views, Middleware, Program.cs)
9. Add seed data
10. Write unit tests
11. Write integration tests
12. Create Docker files
13. Create GitHub Actions workflow
14. Write README.md

---

## DELIVERABLES CHECKLIST

At the end, confirm each item is completed:

- [ ] Solution builds with `dotnet build` — zero errors, zero warnings
- [ ] All migrations applied: `dotnet ef database update`
- [ ] `docker-compose up` starts all services successfully
- [ ] Swagger UI accessible at `http://localhost:5000/swagger`
- [ ] Web UI accessible at `http://localhost:8080`
- [ ] Login works with seeded credentials
- [ ] Product CRUD works end-to-end
- [ ] CSV import works (upload → background process → result)
- [ ] PDF export downloads successfully
- [ ] Excel export downloads successfully
- [ ] Email alert triggers on low stock (log confirmation if SMTP not configured)
- [ ] All unit tests pass: `dotnet test`
- [ ] Test coverage ≥ 70%
- [ ] All 5 stored procedures created and callable
- [ ] Security headers present in all responses
- [ ] Rate limiting returns HTTP 429 after limit exceeded
- [ ] JWT auth works (login → token → protected endpoint)
- [ ] Refresh token rotation works
- [ ] GitHub Actions workflow file is valid YAML
- [ ] README.md is complete

---

## FINAL NOTES FOR CLAUDE CODE

- **Generate ALL files completely** — no placeholder comments like `// TODO: implement this`
- **Every file must be immediately runnable** — no missing using statements, no unresolved references
- **Use the exact package versions that are stable with .NET 8** — do not use preview/beta packages
- **All connection strings, secrets, and passwords must use environment variable overrides** — show clearly where to set them
- **After generating all code, provide a `SETUP.md`** with exact commands to run the project from zero on a fresh Windows machine
- **Prioritize correctness over brevity** — this is a portfolio project, verbose and clear code is preferred over clever one-liners

Begin by creating the solution structure and installing packages. Proceed through the execution order step by step without skipping anything.
