# InvenTrack Enterprise - Architecture Diagram

## High-Level Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        Browser[Web Browser]
        Postman[API Clients / Postman]
    end

    subgraph "Presentation Layer"
        Web[InvenTrack.Web<br/>ASP.NET Core MVC<br/>Razor Pages + Bootstrap 5]
        Swagger[Swagger UI<br/>API Documentation]
    end

    subgraph "API Layer"
        API[InvenTrack.API<br/>REST API Controllers]
        Middleware[Custom Middleware Pipeline<br/>Auth · Logging · Rate Limit · Audit · Security Headers]
    end

    subgraph "Application Layer"
        Services[Application Services<br/>Product · Category · Supplier · Stock<br/>Auth · Import · Report · Dashboard]
        Validators[FluentValidation<br/>DTO Validation]
        Mapping[AutoMapper<br/>Entity ↔ DTO]
    end

    subgraph "Domain Layer"
        Entities[Domain Entities<br/>Product · Category · Supplier · User<br/>StockMovement · ImportJob · AuditLog]
        Enums[Enumerations<br/>UserRole · MovementType · ImportStatus]
        Exceptions[Custom Exceptions<br/>NotFound · Validation · Unauthorized]
    end

    subgraph "Infrastructure Layer"
        Db[(SQL Server<br/>Entity Framework Core)]
        Repos[Repositories + UnitOfWork]
        BgJobs[Background Services<br/>Import Processor · Daily Summary]
        Email[Email Service<br/>MailKit SMTP]
        Export[Export Services<br/>QuestPDF · ClosedXML]
        ETL[ETL Pipeline<br/>CsvHelper · EPPlus]
        Cache[IMemoryCache<br/>Redis-ready]
    end

    subgraph "Cross-Cutting"
        Logging[Serilog<br/>Console + File]
        Auth[JWT Bearer Auth<br/>BCrypt Password Hashing]
        Security[OWASP Security<br/>Headers · Rate Limiting · Audit]
    end

    Browser --> Web
    Browser --> API
    Postman --> API

    Web --> Services
    API --> Middleware
    Middleware --> Services

    Services --> Mapping
    Services --> Validators
    Services --> Entities
    Services --> Repos

    Repos --> Db
    BgJobs --> Repos
    Email --> SMTP[(SMTP Server)]
    Export --> PDF[PDF Files]
    Export --> XLSX[Excel Files]
    ETL --> CSV[CSV Files]
    ETL --> XLSX
    BgJobs --> Email

    Logging -.-> Services
    Auth -.-> API
    Security -.-> Middleware
    Cache -.-> Services
```

## Data Flow

### Request Flow (API)

```
HTTP Request → SecurityHeaders → RequestLogging → GlobalException → RateLimiting → JWT Auth → Controller → Service → Repository → Database
```

### Import Flow

```
Upload CSV/Excel → Validate File → Create ImportJob → Channel → BackgroundService → Parse Rows → Validate → Upsert Products → Update Job Status → Email Notification
```

## Database Schema

```
Users (1) ──< StockMovements (M)
Users (1) ──< ImportJobs (M)
Users (1) ──< AuditLogs (M)

Categories (1) ──< Products (M)
Suppliers (1) ──< Products (M)

Products (1) ──< StockMovements (M)
Products (1) ──< AlertLogs (M)
```
