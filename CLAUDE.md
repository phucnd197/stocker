# Stocker Project - AI Development Guide

## Overview

Stocker is a full-stack financial analysis application built with:
- **Backend**: .NET 10 with FastEndpoints (REPR pattern), Vertical Slice Architecture
- **Frontend**: React 19 + TypeScript with Vite
- **Monorepo**: Turborepo for build orchestration
- **Type Safety**: OpenAPI code generation

## Quick Links

- [Project Constitution](.specify/memory/constitution.md) - Development principles and guidelines
- [Quick Start Guide](.specify/quickstart.md) - Get started with Spec Kit workflow
- [AI Workflow Plan](ai-workflow/spec-driven-development-plan.md) - Complete workflow documentation

---

## Spec Kit Workflow

### Available Commands

The Stocker project uses **Spec Kit** for structured, Spec-Driven Development. When working with this project, use these slash commands:

#### Core Commands

```bash
/speckit.constitution    # View/edit project principles (one-time setup)
/speckit.specify        # Define what to build (requirements, user stories)
/speckit.plan           # Create technical implementation plan
/speckit.tasks          # Break down into actionable tasks
/speckit.implement      # Execute implementation
```

#### Optional Quality Gates

```bash
/speckit.clarify        # Clarify ambiguities in spec (before planning)
/speckit.analyze        # Cross-artifact consistency analysis
/speckit.checklist      # Generate quality checklists
```

### Custom Presets

This project includes custom Spec Kit presets optimized for our architecture:

#### `.NET FastEndpoints` Preset
- **Purpose**: Backend API feature development
- **Templates**: Optimized for Vertical Slice Architecture, REPR pattern, TDD
- **Use for**: Creating API endpoints, services, entities, tests

#### `React + TypeScript` Preset
- **Purpose**: Frontend UI/UX feature development
- **Templates**: Optimized for React 19, components, hooks, type safety
- **Use for**: Creating React components, hooks, API integration

### Custom Extensions

#### Feature Scaffold Extension
- **Command**: `/speckit.scaffold-feature`
- **Purpose**: Provides templates for feature scaffolding
- **Location**: `.specify/extensions/feature-scaffold/`

#### OpenAPI Sync Extension
- **Command**: `/speckit.sync-types`
- **Purpose**: Syncs TypeScript types from backend Swagger
- **Script**: `.specify/extensions/openapi-sync/scripts/sync-types.sh`

---

## Feature Development Workflow

### Backend Features (.NET FastEndpoints)

```bash
# 1. Define the feature
/speckit.specify Create a watchlist feature where users can manage stock watchlists...

# 🔍 CHECKPOINT #1: Review spec.md - Is this what we want?

# 2. Create technical plan
/speckit.plan Use .NET 10, FastEndpoints, EF Core + PostgreSQL...

# 🔍 CHECKPOINT #2: Review plan.md - Is this how to build it?

# 3. Generate tasks
/speckit.tasks

# 🔍 CHECKPOINT #3: Review tasks.md - Implementation strategy correct?

# 4. Implement
/speckit.implement

# 🔍 CHECKPOINT #4: Review implementation - Code compiles? Tests pass?

# 5. Sync types (if needed)
npm run codegen
```

### Frontend Features (React + TypeScript)

```bash
# 1. Define the UI/UX feature
/speckit.specify Create a watchlist UI with filtering, sorting, and real-time updates...

# 🔍 CHECKPOINT #1: Review spec.md - UI requirements clear?

# 2. Create technical plan
/speckit.plan Use React 19, TypeScript, React Query, component-based architecture...

# 🔍 CHECKPOINT #3: Review plan.md - Component structure sound?

# 3. Generate tasks
/speckit.tasks

# 🔍 CHECKPOINT #4: Review tasks.md - Component breakdown correct?

# 4. Implement
/speckit.implement

# 5. Test and verify
npm run test
npm run build
```

---

## Architecture Patterns

### Backend - Vertical Slice Architecture

Features are organized as vertical slices:

```
Features/{FeatureName}/
├── {FeatureName}ServiceInjection.cs    # Feature DI registration
├── Endpoints/                             # API endpoints (REPR pattern)
├── Services/                              # Business logic
├── Models/                                # DTOs, domain models, entities
│   ├── Entities/                          # EF Core entities
│   ├── Dtos/                              # Request/Response DTOs
│   ├── DomainModels/                      # Internal domain models
│   └── Validators/                       # FluentValidation validators
├── Tests/                                 # Feature-specific tests
│   ├── Unit/                              # Service, validator tests
│   └── Integration/                       # Endpoint tests
└── Constants/                             # Configuration constants
```

**Key Principles**:
- Each feature is self-contained
- No shared business logic across features
- Features expose only API contracts (DTOs)
- Dependency injection is hierarchical

### Frontend - Component Architecture

Features are organized by UI components:

```
apps/frontend/src/features/{FeatureName}/
├── components/                            # React components
│   ├── {Feature}Page.tsx                 # Main page
│   ├── {Feature}List.tsx                 # List component
│   ├── {Feature}Item.tsx                 # Item component
│   └── {Feature}Form.tsx                 # Form component (if CRUD)
├── hooks/                                 # Custom hooks
│   ├── use{Feature}.ts                   # Feature logic hook
│   └── use{Feature}Api.ts                # API hook (React Query)
├── services/                              # API clients
│   └── {feature}Api.ts                   # Typed API client
├── types/                                 # Local types
│   └── {feature}.ts                      # Extends api-contracts
└── tests/                                 # Component tests
```

**Key Principles**:
- Component-based architecture
- Type-safe API integration via `@stocker/api-contracts`
- Custom hooks for state and API calls
- Server state with React Query

---

## TDD Requirements

### Backend TDD

**Process**:
1. **Red**: Write failing test
2. **Green**: Write minimal code to pass
3. **Refactor**: Improve while tests stay green

**Test Structure**:
```
Features/{FeatureName}/Tests/
├── Unit/
│   ├── Services/        # Service logic tests
│   └── Validators/      # Validation tests
└── Integration/
    └── Endpoints/       # API endpoint tests
```

**Test Coverage Goals**:
- Services: >80% coverage
- Endpoints: >70% coverage
- Validators: >90% coverage

### Frontend Testing

**Test Types**:
- Component tests (React Testing Library)
- Hook tests (testing-library hooks)
- Integration tests (E2E with Playwright/Cypress)

**Test Coverage Goals**:
- Components: >70% coverage
- Hooks: >80% coverage
- Critical flows: E2E tests

---

## Database Migrations

### EF Core Migration Workflow

When adding entities or modifying schema:

```bash
# 1. Create migration
dotnet ef migrations add Add{EntityName}

# 2. Review generated migration
# Check Migrations/{timestamp}_Add{EntityName}.cs

# 3. Apply migration
dotnet ef database update

# 4. Verify
# Check database schema
```

### Migration Best Practices

- Always review generated migration code
- Test migrations on local database first
- Use descriptive migration names
- Include migration in feature code commits

---

## API Contract Management

### TypeScript Type Generation

The frontend uses auto-generated TypeScript types from the backend OpenAPI spec:

```bash
# 1. Ensure backend is running
# Backend should be available at https://localhost:7001

# 2. Generate types
npm run codegen

# 3. Verify
# Check packages/api-contracts/index.ts

# 4. Build workspace
npm run build
```

### Type Safety Workflow

1. Backend changes → Run backend
2. Generate types → `npm run codegen`
3. Frontend uses types → Import from `@stocker/api-contracts`
4. Build verifies types → `npm run build`

---

## Common Pitfalls & Solutions

### Backend Pitfalls

| Pitfall | Solution |
|---------|----------|
| Using domain models in API responses | Always use DTOs in endpoints |
| Mixing features | Keep features isolated - VSA principle |
| Skipping EF Core migrations | Always use migrations for schema changes |
| Blocking async calls | Use async/await, no `.Result` or `.Wait()` |
| Missing cancellation tokens | Pass `CancellationToken` to all async methods |

### Frontend Pitfalls

| Pitfall | Solution |
|---------|----------|
| Using `any` type | Always use proper TypeScript types |
| Not handling loading/error states | Show loading spinners and error messages |
| Missing accessibility attributes | Add ARIA labels and keyboard navigation |
| Not memoizing expensive components | Use `React.memo()` for optimization |
| Ignoring responsive design | Test on mobile, tablet, desktop breakpoints |

### Development Workflow Pitfalls

| Pitfall | Solution |
|---------|----------|
| Skipping checkpoints | Always review at each human checkpoint |
| Writing tests after implementation | Follow TDD - tests first |
| Not syncing types after API changes | Run `npm run codegen` after backend changes |
| Ignoring test failures | Never commit failing tests |
| Missing EF Core migrations | Generate and apply migrations when schema changes |

---

## Shell Commands Reference

### Backend Development

```bash
# Run backend with hot reload
dotnet watch run --launch-profile https

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Create EF Core migration
dotnet ef migrations add MigrationName

# Apply EF Core migration
dotnet ef database update

# Build backend
dotnet build

# Publish backend
dotnet publish -c Release
```

### Frontend Development

```bash
# Run frontend dev server
npm run dev

# Build frontend
npm run build

# Run tests
npm run test

# Generate TypeScript types from OpenAPI
npm run codegen

# Lint code
npm run lint

# Format code
npm run format
```

### Monorepo Commands

```bash
# Run all apps in dev mode
npm run dev

# Build all packages
npm run build

# Run all tests
npm run test

# Clean all builds
npm run clean
```

---

## Environment Setup

### Prerequisites

- **.NET 10 SDK** - Backend development
- **Node.js 20+** - Frontend development
- **PostgreSQL** - Production database (or SQLite for dev)
- **Git** - Version control

### Installation

```bash
# Clone repository
git clone <repository-url>
cd Stocker

# Install dependencies
npm install

# Initialize Spec Kit (already done)
# specify init . --integration claude

# Run backend
cd apps/backend
dotnet restore
dotnet watch run

# Run frontend (in separate terminal)
cd apps/frontend
npm run dev
```

---

## Project Structure

```
Stocker/
├── apps/
│   ├── backend/                 # .NET 10 Web API
│   │   ├── Features/            # Vertical slice features
│   │   ├── Program.cs           # Application entry point
│   │   └── Stocker.csproj      # Project file
│   └── frontend/                # React 19 + TypeScript
│       ├── src/
│       │   ├── features/        # Feature-based components
│       │   └── main.tsx         # App entry point
│       └── package.json
├── packages/
│   └── api-contracts/           # Shared TypeScript types
│       └── index.ts             # Generated from OpenAPI
├── .specify/                    # Spec Kit configuration
│   ├── memory/
│   │   └── constitution.md      # Project principles
│   ├── presets/                 # Custom presets
│   │   ├── dotnet-fastendpoints/
│   │   └── react-typescript/
│   ├── extensions/              # Custom extensions
│   │   ├── feature-scaffold/
│   │   └── openapi-sync/
│   └── templates/               # Base templates
├── ai-workflow/                 # Workflow documentation
│   └── spec-driven-development-plan.md
├── turbo.json                   # Turborepo configuration
├── package.json                 # Root package.json
└── CLAUDE.md                    # This file
```

---

## Getting Help

### Resources

- [Spec Kit Documentation](https://github.com/github/spec-kit)
- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/core/)
- [React 19 Documentation](https://react.dev/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)

### Project-Specific Help

- Review [Project Constitution](.specify/memory/constitution.md) for principles
- Check [Quick Start Guide](.specify/quickstart.md) for workflow walkthrough
- Read [AI Workflow Plan](ai-workflow/spec-driven-development-plan.md) for detailed workflow

---

## Development Best Practices

### Code Quality

- Follow existing code patterns and naming conventions
- Write self-documenting code with clear names
- Add comments only when "why" is unclear, not "what"
- Keep functions small and focused
- Extract complex logic into well-named methods

### Git Workflow

- Create feature branches from `main`
- Write descriptive commit messages
- Include feature spec reference in commits
- Never commit failing tests
- Ensure build passes before committing

### Testing Standards

- Write tests before implementation (TDD)
- Test behavior, not implementation details
- Use descriptive test names
- One assertion per test (when possible)
- Mock external dependencies

---

**Last Updated**: 2025-06-09  
**Spec Kit Version**: 0.9.6.dev0  
**Project Version**: 1.0
