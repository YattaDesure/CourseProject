# Green Quarter Property Management

Monorepo for the Green Quarter subsystem. The legacy PHP prototype remains under `php/` for reference, while the new stack introduces:

- `backend/` – ASP.NET Core 8 Web API (JWT auth, EF Core, export services)
- `frontend/` – Vue 3 + TypeScript (Composition API, Pinia, role-aware routing)
- `infrastructure/` – SQL Server artifacts, Docker Compose, deployment helpers
- `docs/` – architecture notes, deployment runbooks, and user manuals

## Quick start

1. **Clone & install prerequisites**
   - .NET 8 SDK, Node.js 20+, Docker Desktop (with Compose V2)
2. **Environment variables**
   - Copy `.env.example` to `.env` once created; populate SQL SA password, JWT secrets, admin bootstrap credentials.
3. **Run the stack**
   - `docker compose up --build` (applies migrations, seeds sample data, starts API + SPA)
4. **Access**
   - API: `http://localhost:5000`
   - SPA: `http://localhost:5173` (proxied through Nginx container in production)

See `docs/DEPLOYMENT.md` for production roll-out, `docs/USER_GUIDE.md` for role behaviors, and `docs/DATABASE.md` for schema details.

