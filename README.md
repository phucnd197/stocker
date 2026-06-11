# Stocker

## What It Does

Stocker is a full-stack financial analysis application for ranking and analyzing US stocks. It aggregates stock data from TradingView, applies PE and ROIC-based ranking algorithms, and presents results through a modern React dashboard with authenticated user profiles and avatar uploads.

## Tech Stack

| Layer              | Technology                                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------- |
| **Backend**        | .NET 10, FastEndpoints (REPR pattern), EF Core, SQL Server 2025                                         |
| **Frontend**       | React 19, TypeScript 6, Vite 8, MUI 9, TanStack React Query 5                                           |
| **Auth**           | Auth0 (JWT)                                                                                             |
| **Caching**        | Redis 7.2 (distributed caching via StackExchangeRedis)                                                  |
| **Job Scheduling** | TickerQ (daily stock data refresh)                                                                      |
| **Resilience**     | Polly HTTP resilience (retry + circuit breaker), ASP.NET Core rate limiting                             |
| **Object Storage** | MinIO (avatars and file uploads)                                                                        |
| **API Types**      | Auto-generated TypeScript types via openapi-typescript from OpenAPI spec                                |
| **Observability**  | OpenTelemetry → Prometheus (metrics), Grafana Tempo (traces), Grafana Loki (logs), Grafana (dashboards) |
| **Testing**        | xUnit + Testcontainers + NSubstitute (backend), Vitest + happy-dom (frontend)                           |
| **Tooling**        | Turborepo (monorepo), ESLint + Prettier, Husky + lint-staged, Gitleaks                                  |

## How to Run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) (npm 11.6.2)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Python 3+ &amp; pip](https://www.python.org/downloads/) (for pre-commit / Gitleaks)
- Git

### 1. Clone & Install

```bash
git clone https://github.com/phucnd197/stocker.git
cd stocker
npm install
```

### 2. Start Infrastructure

```bash
docker compose -f docker/docker-compose.yml up -d
```

This starts all infrastructure services:

| Service         | Port                      | Purpose                              |
| --------------- | ------------------------- | ------------------------------------ |
| SQL Server 2025 | 1433                      | Primary database + TickerQ job store |
| MinIO           | 9000 / 9001 (console)     | Object storage (avatars)             |
| Redis           | 6379                      | Distributed cache                    |
| OTel Collector  | 4317 (gRPC) / 4318 (HTTP) | Telemetry pipeline                   |
| Prometheus      | 9090                      | Metrics backend                      |
| Loki            | 3100                      | Logs backend                         |
| Tempo           | 3200                      | Traces backend                       |
| Grafana         | 3000                      | Dashboards (admin/admin)             |

### 3. Start the Backend

```bash
cd apps/backend/src
dotnet restore
dotnet ef database update
dotnet watch run --launch-profile https
```

- **HTTPS**: `https://localhost:7242`
- **HTTP**: `http://localhost:5113`
- **Swagger UI**: open either URL in your browser

### 4. Start the Frontend

Edit `apps/frontend/.env` with your Auth0 credentials:

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

Frontend available at `http://localhost:5173`.

> **Tip:** After first-time setup (steps 3–4), you can use Turborepo commands from the repo root: `npm run dev` (start both apps), `npm run build` (build all), `npm run test` (run all tests).

### 5. Generate API Types

With the backend running on port 5113:

```bash
npm run codegen
```

This regenerates `packages/api-contracts/index.ts` from the OpenAPI spec.

### Useful Commands

| Command                           | Description                                     |
| --------------------------------- | ----------------------------------------------- |
| `npm run dev`                     | Start all apps (Turborepo)                      |
| `npm run build`                   | Build all packages and apps                     |
| `npm run test`                    | Run all tests                                   |
| `npm run codegen`                 | Regenerate TypeScript types from OpenAPI        |
| `dotnet ef migrations add <Name>` | Create a DB migration (from `apps/backend/src`) |
