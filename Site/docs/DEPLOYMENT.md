# Deployment Guide

## 1. Prerequisites

- Docker 24+ with Compose V2
- .NET 8 SDK (for local builds / EF migrations)
- Node.js 20+ / pnpm 9+
- Access to a container registry (GHCR, Docker Hub, or TimeWeb Container Registry)
- TimeWeb Cloud account with SSH access and the ability to provision Docker-based hosting

## 2. Environments

| Variable | Description | Example |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | API environment (`Development`, `Production`) | `Production` |
| `JWT__ISSUER` | Token issuer | `https://greenquarter.local` |
| `JWT__AUDIENCE` | Token audience | `greenquarter-clients` |
| `JWT__KEY` | 256-bit secret | `base64-encoded` |
| `SQLSERVER__SA_PASSWORD` | Strong SA password | `S0m3$uperStrongPwd` |
| `DB__CONNECTION_STRING` | EF Core connection string | `Server=sqlserver,1433;Database=GreenQuarter;User Id=sa;Password=...;TrustServerCertificate=True;` |
| `ADMIN__EMAIL` | Seed admin login | `admin@greenquarter.test` |
| `ADMIN__PASSWORD` | Seed admin password | `ChangeMe!123` |

Store secrets in `.env` locally and in TimeWeb Secrets for production.

## 3. Local Docker workflow

1. Build assets:
   - `cd backend && dotnet publish -c Release`
   - `cd frontend && pnpm install && pnpm build`
2. From repo root: `docker compose up --build`
3. Compose overview (see `docker-compose.yml`):
   - `sqlserver`: SQL Server 2022 with mounted `./infrastructure/db/init.sql`
   - `api`: ASP.NET Core image (`backend/Dockerfile`) applying migrations on start
   - `frontend`: Nginx serving `/app/dist`
   - `adminer` (optional): DB inspection UI

## 4. Database migrations & seeding

### Add a migration
```
cd backend
dotnet ef migrations add AddParkingSpaces --project GreenQuarter.Infrastructure --startup-project GreenQuarter.Api
```

### Apply migrations locally
```
dotnet ef database update --project GreenQuarter.Infrastructure --startup-project GreenQuarter.Api
```

### Seed data

`GreenQuarter.Infrastructure/Seed/SeedData.cs` runs automatically on API startup and inserts:

- Default admin with `Admin` role
- Sample owners (`Owner` role) and their linked assets
- Example apartments, storage units, parking slots

## 5. CI/CD pipeline (GitHub Actions)

`/.github/workflows/ci.yml` (to be created) should:

1. Checkout repository
2. Restore & test backend (`dotnet restore`, `dotnet test`)
3. Install & build frontend (`pnpm install`, `pnpm build`)
4. Build & push Docker images:
   - `ghcr.io/<org>/greenquarter-api:sha`
   - `ghcr.io/<org>/greenquarter-frontend:sha`
5. Deploy (TimeWeb)
   - SSH into the target server or use TimeWeb API
   - Pull latest images, run `docker compose up -d --remove-orphans`

## 6. TimeWeb Cloud deployment

1. **Provision server**
   - Ubuntu 22.04 LTS with Docker Engine preinstalled.
2. **Configure secrets**
   - Create `/opt/greenquarter/.env` with production values.
3. **Copy compose file**
   - Upload `docker-compose.prod.yml` (extend base compose with TLS + volumes).
4. **Bootstrap**
   ```
   cd /opt/greenquarter
   docker compose pull
   docker compose up -d
   ```
5. **Reverse proxy / TLS**
   - Use TimeWeb load balancer or Traefik/Nginx sidecar.
   - Terminate TLS, forward to `frontend` (443→8443) and `api` (443→5000).
6. **Zero-downtime updates**
   - `docker compose pull && docker compose up -d --remove-orphans`

## 7. Monitoring & backups

- Enable SQL Server backups via Azure-compatible tools or nightly `sqlcmd` exports mounted to `backups/`.
- Add structured logging (Serilog + Seq/ELK) and expose health checks at `/health`.
- TimeWeb snapshots recommended before major releases.
