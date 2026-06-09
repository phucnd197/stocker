# {FEATURE_NAME} Implementation Plan

## Technical Stack

### Backend
- **Framework**: .NET 10 with ASP.NET Core Web API
- **API Framework**: FastEndpoints 8.x with REPR pattern
- **ORM**: EF Core 10.x
- **Database**: PostgreSQL (production), SQLite (development)
- **Testing**: xUnit 2.x
- **Documentation**: FastEndpoints.Swagger

### Frontend (if applicable)
- **Framework**: React 19.2.x
- **Language**: TypeScript 6.0.x
- **Build**: Vite 8.x
- **Type Safety**: OpenAPI code generation

## Architecture Overview

### Vertical Slice Architecture

This feature follows Vertical Slice Architecture (VSA) principles:

```
Features/{FEATURE_NAME}/
├── {FEATURE_NAME}ServiceInjection.cs    # Feature DI registration
├── Endpoints/                             # API endpoints (REPR pattern)
├── Services/                              # Business logic
├── Models/                                # DTOs, domain models, entities
│   ├── Entities/                          # EF Core entities
│   ├── Dtos/                              # Request/Response DTOs
│   ├── DomainModels/                      # Internal domain models
│   └── Validators/                       # FluentValidation validators
├── Tests/                                 # Feature-specific tests
│   ├── Unit/
│   │   ├── Services/
│   │   └── Validators/
│   └── Integration/
│       └── Endpoints/
└── Constants/                             # Configuration/constants
```

### REPR Pattern Reference

All endpoints follow Request-Endpoint-Response pattern:

```csharp
public class {ENDPOINT_NAME}Endpoint : Endpoint<{REQUEST}Dto, {RESPONSE}Dto>
{
    private readonly {SERVICE}_service;

    public {ENDPOINT_NAME}Endpoint({SERVICE}_service service)
    {
        _service = service;
    }

    public override void Configure()
    {
        {HTTP_METHOD}("/api/{route}");
        {AUTHORIZATION};
        {OPTIONS};
    }

    public override async Task HandleAsync({REQUEST}Dto req, CancellationToken ct)
    {
        // 1. Validate request (if not using auto-validation)
        // 2. Call service layer
        // 3. Map domain model to response DTO
        // 4. Send response
    }
}
```

## Implementation Details

### Phase 1: Database & Entities

#### EF Core Entities

Create entities in `Models/Entities/`:

```csharp
public class {ENTITY_NAME}
{
    public long Id { get; set; }
    
    // Properties
    public required string Property1 { get; set; }
    public string? Property2 { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**EF Core Configuration**:

```csharp
public class {ENTITY_NAME}Configuration : IEntityTypeConfiguration<{ENTITY_NAME}>
{
    public void Configure(EntityTypeBuilder<{ENTITY_NAME}> builder)
    {
        builder.ToTable("{TABLE_NAME}");
        builder.HasKey(x => x.Id);
        
        // Property configurations
        builder.Property(x => x.Property1)
            .IsRequired()
            .HasMaxLength(200);
        
        // Indexes
        builder.HasIndex(x => x.Property1)
            .HasDatabaseName("IX_{TABLE_NAME}_Property1");
    }
}
```

#### DbContext Extension

Extend `StockerDbContext`:

```csharp
public partial class StockerDbContext
{
    public DbSet<{ENTITY_NAME}> {ENTITY_NAME}s => Set<{ENTITY_NAME}>();
}
```

#### Migration

```bash
dotnet ef migrations add Add{ENTITY_NAME}
dotnet ef database update
```

### Phase 2: Services Layer

#### Service Interface

```csharp
public interface I{SERVICE_NAME}
{
    Task<{RESULT}> {METHOD}_Async({PARAMETERS}, CancellationToken ct = default);
}
```

#### Service Implementation

```csharp
public class {SERVICE_NAME} : I{SERVICE_NAME}
{
    private readonly StockerDbContext _dbContext;
    
    public {SERVICE_NAME}(StockerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<{RESULT}> {METHOD}_Async({PARAMETERS}, CancellationToken ct = default)
    {
        // Implementation
    }
}
```

### Phase 3: DTOs & Validators

#### Request DTOs

```csharp
public class {REQUEST}Dto
{
    public required string Property1 { get; set; }
    public string? Property2 { get; set; }
}
```

#### Response DTOs

```csharp
public class {RESPONSE}Dto
{
    public required long Id { get; set; }
    public required string Property1 { get; set; }
    public string? Property2 { get; set; }
}
```

#### FluentValidation Validators

```csharp
public class {REQUEST}Validator : AbstractValidator<{REQUEST}Dto>
{
    public {REQUEST}Validator()
    {
        RuleFor(x => x.Property1)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Property2)
            .Must(BeValidFormat)
            .WithMessage("Invalid format");
    }
    
    private bool BeValidFormat(string? value)
    {
        // Custom validation logic
        return true;
    }
}
```

### Phase 4: Endpoints (REPR Pattern)

#### Endpoint Implementation

```csharp
public class {ENDPOINT}Endpoint : Endpoint<{REQUEST}Dto, {RESPONSE}Dto>
{
    private readonly I{SERVICE}_service;
    
    public {ENDPOINT}Endpoint(I{SERVICE}_service service)
    {
        _service = service;
    }
    
    public override void Configure()
    {
        {HTTP_METHOD}("/api/{route}");
        AllowAnonymous(); // or Policies(...)
        Options(x => x.WithTags("{FEATURE_NAME}"));
    }
    
    public override async Task HandleAsync({REQUEST}Dto req, CancellationToken ct)
    {
        var result = await _service.{METHOD}_Async(req, ct);
        await Send.OkAsync(result, ct);
    }
}
```

### Phase 5: Dependency Injection

#### Feature DI Registration

```csharp
public static class {FEATURE}ServiceInjection
{
    public static IServiceCollection Add{FEATURE}Dependencies(this IServiceCollection services)
    {
        services.AddScoped<I{SERVICE}, {SERVICE}>();
        return services;
    }
}
```

#### Global DI Registration

Update `ServiceDependencyInjections.cs`:

```csharp
public static class ServiceDependencyInjections
{
    public static IServiceCollection AddFeatureDependencies(this IServiceCollection services)
    {
        services.AddStockRankingDependencies();
        services.Add{FEATURE}Dependencies(); // Add this line
        return services;
    }
}
```

### Phase 6: Testing

#### Unit Tests

```csharp
public class {SERVICE}Tests
{
    [Fact]
    public async Task {METHOD}_{EXPECTED_BEHAVIOR}_{EXPECTED_RESULT}()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

#### Integration Tests

```csharp
public class {ENDPOINT}EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task {HTTP_METHOD}_{ROUTE}_{EXPECTED_BEHAVIOR}()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### Phase 7: OpenAPI Documentation

All endpoints must include:

- Summary: Brief description of what the endpoint does
- Description: Detailed explanation
- Request schema: Defined via DTO
- Response schema: Defined via DTO
- Error responses: Documented

```csharp
public override void Configure()
{
    Post("/api/{route}");
    AllowAnonymous();
    
    // OpenAPI documentation
    Description(s: "Detailed description...", 
        example: "Example usage...");
}
```

## Configuration

### Application Settings

Add to `appsettings.json` if needed:

```json
{
  "{FEATURE_NAME}": {
    "Setting1": "value1",
    "Setting2": "value2"
  }
}
```

### Constants

Create `Constants/{FEATURE}Constants.cs`:

```csharp
public static class {FEATURE}Constants
{
    public const int MAX_ITEMS = 100;
    public const string DEFAULT_SORT = "name";
}
```

## Error Handling Strategy

### Validation Errors (400)
- Return `ValidationProblemDetails` with field-level errors
- Use FluentValidation for automatic validation

### Not Found (404)
- Return `ProblemDetails` with resource identifier

### Server Errors (500)
- Log exception details
- Return generic error message to client
- Include correlation ID for troubleshooting

## Performance Considerations

### Database Queries
- Use projection queries (`Select`) to fetch only needed data
- Use `Include()` for related data to avoid N+1 queries
- Add indexes for frequently queried columns
- Consider pagination for large result sets

### Caching Strategy
- Cache frequently accessed, rarely changing data
- Use appropriate cache expiration policies
- Invalidate cache on data changes

### Async Operations
- Use `async`/`await` throughout (no `.Result` or `.Wait()`)
- Pass `CancellationToken` to all async methods
- Configure `ConfigureAwait(false)` for library code

## Security Considerations

### Authorization
- Use `[AllowAnonymous]` for public endpoints
- Use `Policies("{POLICY_NAME}")` for protected endpoints
- Validate user permissions in service layer

### Input Validation
- Validate all input parameters
- Sanitize user-provided data
- Use parameterized queries (EF Core handles this)

### Sensitive Data
- Never log sensitive data (passwords, tokens)
- Use secure configuration for secrets
- Encrypt data at rest and in transit (HTTPS)

## Testing Strategy

### Unit Tests
- Test service business logic
- Test validation rules
- Test domain calculations
- Mock external dependencies

### Integration Tests
- Test endpoint contracts
- Test database operations
- Test error scenarios
- Use `WebApplicationFactory` for test host

### Test Coverage Goals
- Services: >80% coverage
- Endpoints: >70% coverage
- Validators: >90% coverage

## Deployment Considerations

### Database Migrations
- Migrations run automatically on deployment
- Rollback plan for failed migrations
- Test migrations in staging environment first

### API Versioning
- Use URL-based versioning: `/api/v1/{resource}`
- Document breaking changes
- Support old versions for deprecation period

### Monitoring
- Log endpoint access patterns
- Monitor response times
- Track error rates
- Set up alerts for anomalies

## Rollout Plan

### Phase 1: Development
- Implement feature locally
- Write and run all tests
- Manual testing via Swagger UI

### Phase 2: Code Review
- Review against constitution
- Verify TDD adherence
- Check code quality standards

### Phase 3: Staging
- Deploy to staging environment
- Run integration tests
- Performance testing
- Security review

### Phase 4: Production
- Deploy to production
- Monitor metrics
- Be ready to rollback

## References

- [FastEndpoints REPR Pattern](https://fast-endpoints.com/docs/basics)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [xUnit Documentation](https://xunit.net/)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Project Constitution](../../../.specify/memory/constitution.md)

---

**Checkpoint #2: Human Review Required**

Before proceeding to `/speckit.tasks`, verify:
- [ ] Is the tech stack appropriate?
- [ ] Is the database schema correct?
- [ ] Is the API design RESTful?
- [ ] Are external dependencies justified?
- [ ] Are performance considerations addressed?
- [ ] Are security concerns covered?
- [ ] Is the implementation strategy sound?
