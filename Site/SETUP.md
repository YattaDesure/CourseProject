# Green Quarter Property Management System - Setup Guide

## Overview

Complete property management system with:
- **Backend**: ASP.NET Core 8.0 Web API with JWT authentication
- **Frontend**: Vue.js 3 with TypeScript support, Pinia state management
- **Database**: SQL Server 2022 (Docker)
- **Features**: 5 main sections (Apartments, Parking, Storage, Users, Personal Account) with role-based access

## Prerequisites

- .NET 8 SDK
- Node.js 20+ and npm/pnpm
- Docker Desktop (for SQL Server)
- SQL Server drivers for PHP (if using PHP app)

## Quick Start

### 1. Start SQL Server (Docker)

```bash
docker compose up sqlserver -d
```

### 2. Backend Setup

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project GreenQuarter.Api
```

The API will run on `http://localhost:5000`

### 3. Frontend Setup

```bash
cd frontend
npm install  # or pnpm install
npm run dev  # or pnpm dev
```

The frontend will run on `http://localhost:5173`

## Database Setup

The database will be automatically created on first run. To create initial admin user, you can:

1. Use the registration endpoint: `POST /api/auth/register`
2. Or manually insert via SQL Server Management Studio

## Role-Based Access

- **User**: Can view Apartments, Parking, Storage (read-only) + Personal Account
- **Moderator**: Can view everything except Users section + CRUD operations on properties
- **Admin**: Full access including Users management

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new user

### Properties
- `GET /api/apartments` - List apartments (with search/filter)
- `POST /api/apartments` - Create (Moderator+)
- `PUT /api/apartments/{id}` - Update (Moderator+)
- `DELETE /api/apartments/{id}` - Delete (Admin only)

Similar endpoints for `/api/parking` and `/api/storagerooms`

### Users
- `GET /api/users` - List users (Admin only)
- `PUT /api/users/{id}/role` - Update role (Admin only)

### Account
- `GET /api/account/me` - Get current user's properties

## Frontend Features

- Tab navigation with role-based menu items
- Search and filter capabilities on all data tables
- Modal forms for creating/editing properties
- Responsive design with green theme
- JWT token stored in localStorage

## Environment Variables

Backend (`appsettings.json`):
- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `JWT:Key` - Secret key for JWT tokens (min 32 chars)
- `JWT:Issuer` - Token issuer
- `JWT:Audience` - Token audience

Frontend (`.env`):
- `VITE_API_URL` - Backend API URL (default: http://localhost:5000)

## Testing

1. Register a user or use existing credentials
2. Login to get JWT token
3. Navigate through sections based on your role
4. Test search/filter functionality
5. Create/edit properties (if Moderator+)

## Troubleshooting

- **Database connection errors**: Ensure SQL Server container is running
- **CORS errors**: Check `Program.cs` CORS configuration matches frontend URL
- **JWT errors**: Verify JWT key is at least 32 characters
- **404 on frontend routes**: Ensure you're using Vue Router history mode

