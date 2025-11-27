# Backend Scaffold

Create the ASP.NET Core solution inside this folder:

```bash
cd backend
dotnet new sln -n GreenQuarter
dotnet new webapi -n GreenQuarter.Api
dotnet new classlib -n GreenQuarter.Domain
dotnet new classlib -n GreenQuarter.Application
dotnet new classlib -n GreenQuarter.Infrastructure
dotnet sln add **/*.csproj
```

Recommended packages:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `FluentValidation.AspNetCore`
- `Serilog.AspNetCore`

Project structure:

```
backend/
 ├── GreenQuarter.Api
 ├── GreenQuarter.Application
 ├── GreenQuarter.Domain
 └── GreenQuarter.Infrastructure
```

Wire up EF Core DbContext in `GreenQuarter.Infrastructure`, register it plus repositories in `Api` `Program.cs`, and expose controllers for apartments, storage rooms, parking spaces, owners, authentication, and exports.

