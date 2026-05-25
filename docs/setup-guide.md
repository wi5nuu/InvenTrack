# InvenTrack Enterprise - Setup Guide

## Prerequisites

- Windows 10/11, macOS, or Linux
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recommended)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional, for DB inspection)

## Option 1: Docker Setup (Recommended)

```bash
# Clone the repository
git clone https://github.com/yourusername/InvenTrack.git
cd InvenTrack

# Start all services
docker compose -f docker/docker-compose.yml up -d

# Check status
docker compose -f docker/docker-compose.yml ps

# View logs
docker compose -f docker/docker-compose.yml logs -f api
```

Access:
- Web UI: http://localhost:8080
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Option 2: Manual Setup

### Step 1: Install SQL Server

Using Docker:
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=InvenTrack@2024!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Step 2: Run Database Migrations

```bash
cd src/InvenTrack.API
dotnet restore
dotnet run
```
The database will be auto-created and seeded on first run.

### Step 3: Run the API

```bash
cd src/InvenTrack.API
dotnet run
# API available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

### Step 4: Run the Web UI

```bash
# In a separate terminal
cd src/InvenTrack.Web
dotnet run
# Web UI available at http://localhost:8080
```

## Configuration

All configuration is in `appsettings.json`. For production, override via environment variables:

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=prod-server,1433;Database=InvenTrackDB;User Id=sa;Password=YourPassword!;TrustServerCertificate=True;"
$env:JwtSettings__SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
$env:EmailSettings__Username = "your-email@gmail.com"
$env:EmailSettings__Password = "your-app-password"
```

## Default Credentials

| Role | Email | Password |
|---|---|---|
| Admin | admin@inventrack.local | Admin@123456 |
| Manager | manager@inventrack.local | Manager@123456 |
| Staff | staff@inventrack.local | Staff@123456 |

## Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests with code coverage
dotnet test tests/InvenTrack.UnitTests --collect:"XPlat Code Coverage"

# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
open coverage/index.html
```

## Verifying the Setup

1. Open http://localhost:5000/health - should return `{"status":"healthy"}`
2. Open http://localhost:5000/swagger - should show Swagger UI
3. Login with admin credentials via API:
   ```bash
   curl -X POST http://localhost:5000/api/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"admin@inventrack.local","password":"Admin@123456"}'
   ```
4. Open http://localhost:8080 - should show login page

## Stopping Services

```bash
# Docker
docker compose -f docker/docker-compose.yml down

# Manual - press Ctrl+C in each terminal
```
