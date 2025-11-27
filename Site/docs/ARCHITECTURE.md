## Green Quarter Architecture

### Overview
Green Quarter is split into three deployable units:

- **Backend API (`backend/GreenQuarter.Api`)** — ASP.NET Core 8 REST API providing authentication, role-based authorization, CRUD endpoints, filtering, and export services.
- **Frontend (`frontend/`)** — Vue 3 + TypeScript SPA served via Vite (dev) or static assets (prod) that consumes the API and exposes dashboards tailored to each role.
- **Infrastructure (`infrastructure/`)** — Docker Compose stack orchestrating SQL Server 2022, the API, the SPA, plus database migration/seed helpers.

### Domain Model

| Entity         | Key Fields                                                               | Relationships                                               |
| -------------- | ------------------------------------------------------------------------ | ----------------------------------------------------------- |
| Apartment      | `Id`, `Number`, `Entrance`, `Floor`, `SquareMeters`, timestamps           | `OwnerApartments` (many-to-many with `Owners`)              |
| StorageRoom    | `Id`, `Label`, `Level`, `Area`, status                                   | `OwnerStorageRooms` (many-to-many with `Owners`)            |
| ParkingSpace   | `Id`, `SlotNumber`, `Section`, `Size`, `IsCovered`                       | `OwnerParkingSpaces` (many-to-many with `Owners`)           |
| Owner          | `Id`, `FullName`, `Phone`, `Email`, `UserId`, status                     | Links to properties, optional preferred contact             |

Support tables add auditing (`CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `IsDeleted`) to every entity. Linking tables store share percentage and ownership dates.

### Backend Layers

- `Domain`: pure entities, value objects, enums.
- `Application`: CQRS-style services, DTOs, validators; uses MediatR + FluentValidation to orchestrate commands/queries.
- `Infrastructure`: EF Core DbContext, Identity setup, repositories, exports (CSV/XLSX), JWT issuance, configuration binding.
- `Api`: controllers, request/response shaping, authentication/authorization middleware, OpenAPI docs.

### Authentication & Authorization

- ASP.NET Core Identity with SQL Server store.
- JWT bearer tokens signed with symmetric key; refresh tokens persisted per user.
- Roles: `Admin`, `Owner`. Policy-based checks (e.g., `Policies.RequireAdmin`) control controller/action access. Owners limited to linked assets via claims.

### Frontend Structure

- **Routing**: Vue Router with guarded routes. Layout split by role.
- **State**: Pinia stores manage auth state, per-entity datasets, filtering state, and export metadata.
- **UI**: Component library (e.g., Naive UI) for tables, forms, dialogs. Composition utilities wrap API access with typed responses.
- **Real-time filtering**: debounced inputs update store filters, triggering refetch with query params. Websocket hook reserved for future push updates.

### Data Export

Export service produces CSV and XLSX streams. Controllers expose `/export` endpoints requiring appropriate roles. Frontend simply downloads via `blob`.

### Observability

- Structured logging via Serilog (JSON) to console + rolling files.
- Health checks for API and SQL Server.
- Optional Application Insights/Seq sink configured via environment variables.

### CI/CD Summary

- GitHub Actions workflow builds & tests backend (`dotnet test`), frontend (`npm run build`), then builds Docker images and pushes to registry.
- Deployment job triggers TimeWeb Cloud to pull latest images and run `docker compose up -d --build`.

### Next Steps

1. Scaffold .NET solution & Vue project.
2. Implement EF Core entities/migrations.
3. Wire up controllers + frontend tables/export flows.
4. Configure CI/CD secrets and deploy via TimeWeb.

