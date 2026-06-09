# Feature Scaffold Template: {FEATURE_NAME}

## Overview

Use this template to create a new feature following Stocker project's Vertical Slice Architecture with .NET FastEndpoints.

## Folder Structure to Create

```
Features/{FEATURE_NAME}/
├── {FEATURE_NAME}ServiceInjection.cs
├── Endpoints/
├── Services/
├── Models/
│   ├── Entities/
│   ├── Dtos/
│   ├── DomainModels/
│   └── Validators/
├── Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   └── Validators/
│   └── Integration/
│       └── Endpoints/
└── Constants/
    └── {FEATURE_NAME}Constants.cs
```

## File Templates

### 1. Feature DI Registration: `{FEATURE_NAME}ServiceInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Stocker.Features.{FEATURE_NAME};

public static class {FEATURE_NAME}ServiceInjection
{
    public static IServiceCollection Add{FEATURE_NAME}Dependencies(this IServiceCollection services)
    {
        // Register services here
        // services.AddScoped<I{FEATURE_NAME}Service, {FEATURE_NAME}Service>();
        // services.AddHttpClient<{EXTERNAL}Service>();
        
        return services;
    }
}
```

### 2. Endpoint Template: `Endpoints/{ENDPOINT}Endpoint.cs`

```csharp
using FastEndpoints;
using Stocker.Features.{FEATURE_NAME}.Models.Dtos;
using Stocker.Features.{FEATURE_NAME}.Services;

namespace Stocker.Features.{FEATURE_NAME}.Endpoints;

public class {ENDPOINT}Endpoint : Endpoint<{REQUEST}Dto, {RESPONSE}Dto>
{
    private readonly I{FEATURE_NAME}Service _{FEATURE_LOWER}Service;

    public {ENDPOINT}Endpoint(I{FEATURE_NAME}Service {FEATURE_LOWER}Service)
    {
        _{FEATURE_LOWER}Service = {FEATURE_LOWER}Service;
    }

    public override void Configure()
    {
        {HTTP_METHOD}("/api/{route}");
        AllowAnonymous();
        Options(x => x.WithTags("{FEATURE_NAME}"));
        Description(s: "Endpoint description",
            example: "Example usage");
    }

    public override async Task HandleAsync({REQUEST}Dto req, CancellationToken ct)
    {
        var result = await _{FEATURE_LOWER}Service.{METHOD}_Async(req, ct);
        await Send.OkAsync(result, ct);
    }
}
```

### 3. Service Interface: `Services/I{FEATURE_NAME}Service.cs`

```csharp
using Stocker.Features.{FEATURE_NAME}.Models.Dtos;

namespace Stocker.Features.{FEATURE_NAME}.Services;

public interface I{FEATURE_NAME}Service
{
    Task<{RESPONSE}Dto> {METHOD}_Async({REQUEST}Dto request, CancellationToken ct = default);
}
```

### 4. Service Implementation: `Services/{FEATURE_NAME}Service.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Stocker.Database;
using Stocker.Features.{FEATURE_NAME}.Models.Dtos;

namespace Stocker.Features.{FEATURE_NAME}.Services;

public class {FEATURE_NAME}Service : I{FEATURE_NAME}Service
{
    private readonly StockerDbContext _dbContext;

    public {FEATURE_NAME}Service(StockerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<{RESPONSE}Dto> {METHOD}_Async({REQUEST}Dto request, CancellationToken ct = default)
    {
        // Implementation
        var result = new {RESPONSE}Dto
        {
            // Map properties
        };

        return result;
    }
}
```

### 5. Entity Template: `Models/Entities/{ENTITY}.cs`

```csharp
namespace Stocker.Features.{FEATURE_NAME}.Models.Entities;

public class {ENTITY}
{
    public long Id { get; set; }
    
    public required string Property1 { get; set; }
    public string? Property2 { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 6. Entity Configuration: `Models/Entities/{ENTITY}Configuration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stocker.Features.{FEATURE_NAME}.Models.Entities;

namespace Stocker.Features.{FEATURE_NAME}.Models.Entities;

public class {ENTITY}Configuration : IEntityTypeConfiguration<{ENTITY}>
{
    public void Configure(EntityTypeBuilder<{ENTITY}> builder)
    {
        builder.ToTable("{TABLE_NAME}");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Property1)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.HasIndex(x => x.Property1)
            .HasDatabaseName("IX_{TABLE_NAME}_Property1");
    }
}
```

### 7. Request DTO: `Models/Dtos/{REQUEST}Dto.cs`

```csharp
using JetBrains.Annotations;

namespace Stocker.Features.{FEATURE_NAME}.Models.Dtos;

public class {REQUEST}Dto
{
    [UsedImplicitly]
    public required string Property1 { get; set; }
    
    [UsedImplicitly]
    public string? Property2 { get; set; }
}
```

### 8. Response DTO: `Models/Dtos/{RESPONSE}Dto.cs`

```csharp
using JetBrains.Annotations;

namespace Stocker.Features.{FEATURE_NAME}.Models.Dtos;

public class {RESPONSE}Dto
{
    [UsedImplicitly]
    public required long Id { get; set; }
    
    [UsedImplicitly]
    public required string Property1 { get; set; }
    
    [UsedImplicitly]
    public string? Property2 { get; set; }
}
```

### 9. Validator: `Models/Validators/{REQUEST}Validator.cs`

```csharp
using FluentValidation;
using Stocker.Features.{FEATURE_NAME}.Models.Dtos;

namespace Stocker.Features.{FEATURE_NAME}.Models.Validators;

public class {REQUEST}Validator : AbstractValidator<{REQUEST}Dto>
{
    public {REQUEST}Validator()
    {
        RuleFor(x => x.Property1)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Property1 is required and must be less than 200 characters");
        
        RuleFor(x => x.Property2)
            .Must(BeValidFormat)
            .WithMessage("Property2 format is invalid");
    }
    
    private bool BeValidFormat(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;
        
        // Custom validation logic
        return true;
    }
}
```

### 10. Unit Test Template: `Tests/Unit/Services/{FEATURE_NAME}ServiceTests.cs`

```csharp
using Xunit;
using Stocker.Features.{FEATURE_NAME}.Services;

namespace Stocker.Features.{FEATURE_NAME}.Tests.Unit.Services;

public class {FEATURE_NAME}ServiceTests
{
    [Fact]
    public async Task {METHOD}_Async_WhenValidRequest_ReturnsSuccess()
    {
        // Arrange
        // Act
        // Assert
    }
    
    [Fact]
    public async Task {METHOD}_Async_WhenInvalidRequest_ThrowsValidationException()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

### 11. Integration Test Template: `Tests/Integration/Endpoints/{ENDPOINT}EndpointTests.cs`

```csharp
using Xunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Stocker.Features.{FEATURE_NAME}.Models.Dtos;

namespace Stocker.Features.{FEATURE_NAME}.Tests.Integration.Endpoints;

public class {ENDPOINT}EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public {ENDPOINT}EndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task {HTTP_METHOD}_{ROUTE}_ReturnsSuccess()
    {
        // Arrange
        var request = new {REQUEST}Dto
        {
            Property1 = "test"
        };

        // Act
        var response = await _client.{HTTP_METHOD}AsJsonAsync("/api/{route}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### 12. Constants: `Constants/{FEATURE_NAME}Constants.cs`

```csharp
namespace Stocker.Features.{FEATURE_NAME}.Constants;

public static class {FEATURE_NAME}Constants
{
    public const int MAX_ITEMS = 100;
    public const string DEFAULT_SORT = "name";
    public const int CACHE_DURATION_MINUTES = 30;
}
```

## Required Modifications

### 1. Update `ServiceDependencyInjections.cs`

Add this line to the `AddFeatureDependencies` method:

```csharp
services.Add{FEATURE_NAME}Dependencies();
```

### 2. Update `StockerDbContext`

Add this property if creating new entities:

```csharp
public DbSet<{ENTITY}> {ENTITY}s => Set<{ENTITY}>();
```

### 3. Generate EF Core Migration (if adding entities)

```bash
dotnet ef migrations add Add{ENTITY}
dotnet ef database update
```

## Usage

When Claude Code creates a new feature, reference these templates to ensure:
1. Consistent folder structure
2. Proper namespace usage
3. Correct base class inheritance
4. Appropriate attribute usage
5. TDD test structure

## Checklist After Scaffolding

- [ ] All files created with correct namespaces
- [ ] DI registration added to `ServiceDependencyInjections.cs`
- [ ] DbContext updated if entities added
- [ ] EF Core migration generated and applied (if applicable)
- [ ] Project compiles without errors
- [ ] Tests scaffolded (will fail initially - TDD Red phase)

---

**Note**: This template is designed for reference by Claude Code during feature creation. Modify placeholders as needed for specific feature requirements.
