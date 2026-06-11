# Stocker

> A full-stack financial analysis application for ranking and analyzing Vietnamese stocks.

Stocker aggregates stock data from TradingView, applies PE and ROIC-based ranking algorithms, and presents results through a modern React dashboard with authenticated user profiles and avatar uploads.

---

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Monorepo | Turborepo | 2.x |
| Backend | .NET + FastEndpoints | 10 |
| ORM | Entity Framework Core | 10.x |
| Database | SQL Server | 2025 |
| Object Storage | MinIO | Latest |
| Auth | Auth0 (JWT) | — |
| Frontend | React + TypeScript | 19 / 6.x |
| Build | Vite | 8.x |
| UI Library | MUI | 9.x |
| Data Fetching | TanStack React Query | 5.x |
| Validation | Zod | 4.x |
| API Types | openapi-typescript | 7.x |
| Testing (Backend) | xUnit | 3.x |
| Testing (Frontend) | Vitest | 1.x |

---

## Prerequisites

Make sure the following are installed on your machine:

- **.NET 10 SDK** — [download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** (npm 11.6.2) — [download](https://nodejs.org/)
- **Docker Desktop** — [download](https://www.docker.com/products/docker-desktop/)
- **Git**

---

## Quick Start

### 1. Clone the repository

```bash
git clone https://github.com/phucnd197/stocker.git
cd stocker
```

### 2. Install dependencies

```bash
npm install
```

This installs all workspace dependencies via npm workspaces (frontend, backend wrappers, and shared packages).

### 3. Start infrastructure

```bash
docker compose up -d
```

This starts SQL Server on port **1433** and MinIO on ports **9000** (API) and **9001** (console). MinIO buckets are created automatically when the backend starts.

### 4. Set up the backend

```bash
cd apps/backend/src
dotnet restore
dotnet ef database update
```

Then run the backend:

```bash
# From apps/backend/src
dotnet watch run --launch-profile https
```

Or use the npm workspace script from the repo root:

```bash
npm run dev -w @stocker/backend-src
```

The API is available at:

- **HTTPS**: `https://localhost:7242`
- **HTTP**: `http://localhost:5113`
- **Swagger UI**: open either URL in your browser

### 5. Set up the frontend

```bash
cp apps/frontend/.env-example apps/frontend/.env
```

Edit `apps/frontend/.env` and fill in your Auth0 credentials:

```
VITE_AUTH0_DOMAIN=your-auth0-domain
VITE_AUTH0_CLIENT_ID=your-auth0-client-id
VITE_AUTH0_AUDIENCE=your-auth0-api-audience
VITE_API_URL=https://localhost:7242
```

Then run:

```bash
cd apps/frontend
npm run dev
```

The frontend is available at `http://localhost:5173`.

### 6. Generate API types

The backend must be running on port **5113** for this step:

```bash
npm run codegen
```

This fetches the OpenAPI spec from the backend and generates TypeScript types into `packages/api-contracts/index.ts`.

---

## Project Structure

```
stocker/
├── apps/
│   ├── backend/
│   │   ├── src/                        # .NET 10 Web API
│   │   │   ├── Features/               # Vertical slice features
│   │   │   │   ├── ServiceDependencyInjection.cs
│   │   │   │   ├── Stock/              # Stock ranking feature
│   │   │   │   └── UserProfile/        # User profile feature
│   │   │   ├── Database/               # EF Core context & migrations
│   │   │   ├── Entities/               # Domain entities
│   │   │   ├── Middleware/             # Global exception handler
│   │   │   └── Program.cs             # App entry point
│   │   └── tests/                      # xUnit test project
│   └── frontend/
│       └── src/
│           ├── features/
│           │   ├── auth/               # Auth0 login, protected routes
│           │   ├── stockRanking/        # Stock ranking page & table
│           │   └── userProfile/         # Profile form & avatar upload
│           ├── App.tsx                  # Root component with routing
│           └── main.tsx                 # Auth0 + React Query providers
├── packages/
│   ├── api-contracts/                  # Auto-generated TS types (OpenAPI)
│   └── ts-configs/                     # Shared Vitest base config
├── docker-compose.yml                  # SQL Server + MinIO
├── turbo.json                          # Turborepo task config
└── package.json                        # Root workspace config
```

---

## Available Scripts

### Root (Monorepo)

| Command | Description |
|---------|-------------|
| `npm run dev` | Start all apps in dev mode (via Turborepo) |
| `npm run build` | Build all packages and apps |
| `npm run test` | Run all tests across the monorepo |
| `npm run codegen` | Generate TypeScript types from OpenAPI spec |

### Backend

All `dotnet` commands should be run from `apps/backend/src` unless noted.

| Command | Description |
|---------|-------------|
| `dotnet watch run --launch-profile https` | Run with hot reload (HTTPS) |
| `dotnet run --launch-profile http` | Run without hot reload (HTTP only) |
| `dotnet build` | Build the project |
| `dotnet test` | Run tests |
| `dotnet ef migrations add <Name>` | Create a new migration |
| `dotnet ef database update` | Apply pending migrations |
| `dotnet ef database update 0` | Rollback all migrations |

### Frontend

All commands should be run from `apps/frontend`.

| Command | Description |
|---------|-------------|
| `npm run dev` | Start Vite dev server (`http://localhost:5173`) |
| `npm run build` | Type-check (`tsc`) then build for production |
| `npm run test` | Run Vitest |
| `npm run lint` | Run ESLint with auto-fix |
| `npm run format` | Format code with Prettier |
| `npm run format:check` | Check formatting without writing |

---

## Environment Variables

### Frontend (`apps/frontend/.env`)

| Variable | Description | Example |
|----------|-------------|---------|
| `VITE_AUTH0_DOMAIN` | Auth0 tenant domain | `dev-xxxxx.us.auth0.com` |
| `VITE_AUTH0_CLIENT_ID` | Auth0 SPA application client ID | `abc123...` |
| `VITE_AUTH0_AUDIENCE` | Auth0 API audience identifier | `https://dev-xxxxx.us.auth0.com/api/v2/` |
| `VITE_API_URL` | Backend API base URL | `https://localhost:7242` |

### Backend (`apps/backend/src/appsettings.Development.json`)

Configuration is read from `appsettings.Development.json`. Default values are committed and work out of the box with `docker compose`:

| Section | Key | Default |
|---------|-----|---------|
| `ConnectionStrings` | `DefaultConnection` | `Server=localhost;Database=Stocker;User Id=sa;Password=123456a@A;TrustServerCertificate=True;` |
| `Auth0` | `Domain` | `dev-lm7amq8xe3q5638v.us.auth0.com` |
| `Auth0` | `Audience` | `https://dev-lm7amq8xe3q5638v.us.auth0.com/api/v2/` |
| `Cors` | — | `["http://localhost:5173"]` |
| `Minio` | `Endpoint` | `localhost:9000` |
| `Minio` | `AccessKey` / `SecretKey` | `user` / `password` |
| `Minio` | `PublicBucket` | `stocker-public` |
| `Minio` | `PrivateBucket` | `stocker-private` |

---

## Infrastructure Services (Docker)

Defined in `docker-compose.yml`. Start with `docker compose up -d`, stop with `docker compose down`.

### SQL Server 2025

| Property | Value |
|----------|-------|
| Image | `mcr.microsoft.com/mssql/server:2025-latest` |
| Port | `1433` |
| User | `sa` |
| Password | `123456a@A` |
| Database | `Stocker` (created by EF Core migrations) |
| Volume | `sqlserver-data` (persistent) |

### MinIO

| Property | Value |
|----------|-------|
| Image | `minio/minio:latest` |
| API Port | `9000` |
| Console Port | `9001` |
| Access Key | `user` |
| Secret Key | `password` |
| Buckets | `stocker-public`, `stocker-private` (auto-created on backend startup) |
| Volume | `minio-data` (persistent) |

Open `http://localhost:9001` in your browser to access the MinIO console.

---

## API Type Generation

The frontend consumes types auto-generated from the backend's OpenAPI spec. This ensures type safety across the stack.

**Workflow:**

1. Make a change to a backend endpoint
2. Ensure the backend is running on `http://localhost:5113`
3. Run `npm run codegen` from the repo root
4. The file `packages/api-contracts/index.ts` is regenerated
5. Frontend imports types from `@stocker/api-contracts`

**Usage in frontend code:**

```typescript
import type { components, operations } from '@stocker/api-contracts';

type RankingResponse = components['schemas']['StockerFeaturesStockStockRankingRankingResponse'];
type RankingRequest = operations['StockerFeaturesStockStockRankingRankStocksEndpoint']['parameters']['query'];
```

> **Note:** Never edit `packages/api-contracts/index.ts` manually. It is fully auto-generated and will be overwritten on the next `npm run codegen`.

---

## Architecture

### Backend — Vertical Slice Architecture

Each feature is a self-contained slice under `Features/{FeatureName}/`. A slice groups its own endpoint, service, domain models, and validation rules. Features do not share business logic with each other.

Key patterns:

- **FastEndpoints** with the REPR pattern (Request-Endpoint-Response)
- **FluentValidation** for request validation
- **EF Core** with global query filters for soft deletes (`ISoftDeletable`)
- **Typed HttpClient** for external API calls (e.g., `TradingViewClient`)
- **Dependency injection** per feature via `ServiceDependencyInjection.cs`

### Frontend — Feature-Based Components

Each feature is organized under `src/features/{FeatureName}/` with a consistent structure:

```
src/features/{featureName}/
├── components/        # React components (pages, forms, tables)
├── hooks/             # Custom hooks (data fetching, auth)
├── services/          # API client functions
├── types/             # Local types extending @stocker/api-contracts
└── index.ts           # Public barrel export
```

Key patterns:

- **Auth0 SPA SDK** for authentication (`useAuth0`, `ProtectedRoute`)
- **TanStack React Query** for server state
- **`useApiFetcher`** hook as a typed, authenticated HTTP client
- **Zod** for form validation
- **MUI 9** component library

### Type Safety Flow

```
Backend (C#) → OpenAPI spec → openapi-typescript → api-contracts package → Frontend (TypeScript)
```

Changes to the backend API automatically propagate to the frontend via `npm run codegen`.

---

## External Services

| Service | URL | Purpose |
|---------|-----|---------|
| Auth0 | `dev-lm7amq8xe3q5638v.us.auth0.com` | Authentication (JWT) for frontend and backend |
| TradingView Scanner | `https://scanner.tradingview.com/vietnam/scan` | Vietnamese stock data source |

---

## Pre-commit Hooks

The project uses two pre-commit hooks that run automatically on `git commit`:

### Gitleaks (Secret Detection)

Configured in [`.pre-commit-config.yaml`](.pre-commit-config.yaml). Scans staged files for accidentally committed secrets (API keys, passwords, tokens).

Requires the [pre-commit](https://pre-commit.com/) framework:

```bash
pip install pre-commit
pre-commit install
```

### Husky + lint-staged (Code Quality)

Configured in [`apps/frontend/.lintstagedrc.json`](apps/frontend/.lintstagedrc.json). Runs on every commit that touches frontend files:

| File pattern | Hooks |
|-------------|-------|
| `*.{js,jsx,ts,tsx}` | ESLint (`--fix`) → Prettier (`--write`) |
| `*.{json,css,scss,md}` | Prettier (`--write`) |

This hook is set up automatically when you run `npm install` (via the `prepare` script in `package.json`).

---

## Useful Links

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [React 19 Documentation](https://react.dev/)
- [TanStack React Query](https://tanstack.com/query/)
- [MUI Component Library](https://mui.com/)
- [Auth0 SPA SDK](https://auth0.com/docs/libraries/auth0-react)
- [MinIO Documentation](https://min.io/docs/minio/linux/index.html)
- [Turborepo Documentation](https://turbo.build/repo/docs)
- [openapi-typescript](https://openapi-ts.pages.dev/)
