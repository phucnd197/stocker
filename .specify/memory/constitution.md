<!--
Sync Impact Report:
- Version change: 1.0 → 1.1 (MINOR — new technology choices added)
- Modified principles: Anti-Patterns section expanded with MUI/Zod rules
- Added sections: None (technology stack updated in-place)
- Removed sections: None
- Templates requiring updates:
  ✅ spec-template.md — no changes needed (UI-agnostic)
  ✅ plan-template.md — no changes needed (generic structure)
  ✅ tasks-template.md — no changes needed (task organization unchanged)
- Follow-up TODOs: None
-->

# Stocker Project Constitution

## Purpose

This constitution defines the governing principles and development guidelines for the Stocker project. All feature development must adhere to these principles to maintain consistency, quality, and scalability.

## Project Overview

**Stocker** is a full-stack financial analysis application for ranking and analyzing stocks based on fundamental metrics from TradingView data.

### Architecture
- **Backend**: .NET 10 with ASP.NET Core Web API
- **API Framework**: FastEndpoints (REPR pattern)
- **Architecture Style**: Vertical Slice Architecture (VSA)
- **Frontend**: React 19 + TypeScript with Vite
- **Build System**: Turborepo monorepo
- **Type Safety**: OpenAPI code generation for frontend-backend contracts

---

## Core Principles

### 1. Architecture - Vertical Slice Architecture

**Principle**: Each feature is a self-contained vertical slice with all its components.

**Rules**:
- Features are organized as `Features/{FeatureName}/`
- Each feature contains its own: Endpoints, Services, Models, Tests, Constants
- Features expose only API contracts (DTOs) to other features
- No shared business logic across feature boundaries

**Example Structure**:
```
Features/WatchlistManagement/
├── WatchlistManagementServiceInjection.cs  # Feature DI registration
├── Endpoints/         # API endpoints
├── Services/          # Business logic
├── Models/            # DTOs, domain models, entities
├── Tests/             # Unit and integration tests
└── Constants/         # Feature configuration
```

### 2. API Design - REPR Pattern

**Principle**: Endpoints follow Request-Endpoint-Response pattern with FastEndpoints.

**Rules**:
- Each endpoint inherits from `EndpointWithoutRequest<TResponse>` or `Endpoint<TRequest, TResponse>`
- `Configure()` method defines: HTTP method, route, authorization
- `HandleAsync()` method contains request handling logic
- Endpoints delegate business logic to services
- Responses use DTOs, not domain models

**Example**:
```csharp
public class GetWatchlistEndpoint : EndpointWithoutRequest<WatchlistResponse>
{
    public override void Configure()
    {
        Get("/api/watchlists/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var service = Resolve<WatchlistService>();
        var result = await service.GetAsync(id, ct);
        await Send.OkAsync(result, ct);
    }
}
```

### 3. Data Access - EF Core with Migrations

**Principle**: Database changes are managed through EF Core migrations.

**Rules**:
- Entities are defined in `Models/Entities/`
- DbContext is extended when new entities are added
- Migrations are generated after entity changes
- Migration files are committed with feature code
- Database schema versioning follows migration timestamp

**Required for database changes**:
- Generate migration: `dotnet ef migrations add Add{EntityName}`
- Apply migration: `dotnet ef database update`

### 4. Testing - Test-Driven Development (TDD)

**Principle**: Tests are written before implementation.

**Rules**:
- **Red**: Write failing test first
- **Green**: Write minimal code to pass
- **Refactor**: Improve code while tests pass
- Unit tests for services, validators, and domain logic
- Integration tests for endpoints using `WebApplicationFactory`
- Test naming: `MethodName_ExpectedBehavior_ExpectedResult`

**Test Structure**:
```
Features/{FeatureName}/Tests/
├── Unit/
│   ├── Services/        # Service logic tests
│   └── Validators/      # Validation logic tests
└── Integration/
    └── Endpoints/       # API endpoint tests
```

### 5. Dependency Injection - Hierarchical Registration

**Principle**: Dependencies are registered hierarchically.

**Rules**:
- Global registration in `ServiceDependencyInjections.cs`
- Feature-specific registration in `{FeatureName}ServiceInjection.cs`
- Extension methods pattern: `Add{FeatureName}Dependencies()`
- HttpClient dependencies use `AddHttpClient<T>()`
- Services use `AddScoped()` for request-scoped lifecycle

**Example**:
```csharp
public static class WatchlistManagementServiceInjection
{
    public static IServiceCollection AddWatchlistManagementDependencies(this IServiceCollection services)
    {
        services.AddHttpClient<TradingViewDataFetcher>();
        services.AddScoped<WatchlistService>();
        return services;
    }
}
```

### 6. API Contracts - OpenAPI/Swagger Documentation

**Principle**: All endpoints are documented via Swagger/OpenAPI.

**Rules**:
- FastEndpoints.Swagger is configured
- Endpoints include route summaries and descriptions
- Response schemas are explicitly defined via DTOs
- Frontend TypeScript types are generated from OpenAPI spec
- Run `npm run codegen` to sync types after API changes

**Type Generation**:
```bash
# Backend must be running first
npm run codegen  # Runs openapi-typescript on Swagger endpoint
```

### 7. Code Quality - Standards and Consistency

**Principle**: Code follows consistent patterns and standards.

**Rules**:
- C# nullable reference types enabled
- `required` modifier for mandatory properties
- Async/await used throughout (no `.Result` or `.Wait()`)
- `CancellationToken` passed to async methods
- Proper exception handling with specific exception types
- No silent failures - log errors appropriately

### 8. Performance - Efficient Data Access

**Principle**: Database queries are optimized and efficient.

**Rules**:
- Use projection queries (`Select`) to fetch only needed data
- Avoid N+1 queries - use `Include()` for related data
- Consider pagination for large result sets
- Cache frequently accessed, rarely changing data
- Use appropriate indexes for query performance

---

## Development Workflow

### Feature Development Process

All features follow this workflow:

1. **Specify** - Define requirements using `/speckit.specify`
2. **Review** - Human review checkpoint #1: Validate requirements
3. **Plan** - Create technical plan using `/speckit.plan`
4. **Review** - Human review checkpoint #2: Validate architecture
5. **Tasks** - Break down into TDD tasks using `/speckit.tasks`
6. **Review** - Human review checkpoint #3: Validate task breakdown
7. **Implement** - Execute using `/speckit.implement`
8. **Review** - Human review checkpoint #4: Validate implementation
9. **Verify** - Tests pass, API works, types synced

### Quality Gates

**Checkpoint #1 (After spec)**:
- Are requirements complete and correct?
- Are user stories capturing real needs?
- Are acceptance criteria testable?

**Checkpoint #2 (After plan)**:
- Is tech stack appropriate?
- Is database schema correct?
- Are security concerns covered?

**Checkpoint #3 (After tasks)**:
- Are tasks in logical order?
- Is TDD followed (tests before implementation)?
- Are checkpoints included?

**Checkpoint #4 (After implement)**:
- Does code compile?
- Do tests pass?
- Does API work in Swagger UI?
- Are TypeScript types synced?

---

## Technology Stack

### Backend
- **Framework**: .NET 10
- **API**: FastEndpoints 8.x
- **ORM**: EF Core 10.x
- **Database**: PostgreSQL (production), SQLite (development)
- **Testing**: xUnit 2.x
- **Documentation**: FastEndpoints.Swagger

### Frontend
- **Framework**: React 19.2.x
- **Language**: TypeScript 6.0.x
- **Build**: Vite 8.x
- **UI Library**: Material UI (MUI) 7.x — Material Design component system
- **Form Validation**: Zod 3.x — Schema-based validation for forms and API data
- **Type Safety**: OpenAPI code generation
- **Testing**: Vitest / React Testing Library

### DevOps
- **Monorepo**: Turborepo 2.x
- **Package Manager**: npm 11.x
- **Version Control**: Git
- **Code Quality**: ESLint, TypeScript compiler

---

## Anti-Patterns to Avoid

❌ **Don't** create shared business logic utilities
❌ **Don't** use domain models in API responses (use DTOs)
❌ **Don't** write tests after implementation (violates TDD)
❌ **Don't** mix features (keep vertical slices pure)
❌ **Don't** skip EF Core migrations (manual schema changes)
❌ **Don't** use blocking calls like `.Result` or `.Wait()`
❌ **Don't** ignore cancellation tokens in async methods
❌ **Don't** skip OpenAPI documentation updates
❌ **Don't** build custom UI components when MUI provides an equivalent
❌ **Don't** write manual form validation when Zod schemas can be used
❌ **Don't** use `any` type — always use proper TypeScript types or Zod inference

---

## References

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [REPR Pattern](https://github.com/thangchung/csharp-repr-pattern)
- [Spec Kit Workflow](https://github.com/github/spec-kit)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)

---

**Last Updated**: 2026-06-10
**Version**: 1.1
**Status**: Active
