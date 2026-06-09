# {FEATURE_NAME} Implementation Tasks

## Task Breakdown Overview

These tasks follow **Test-Driven Development (TDD)** principles:
- **Red**: Write failing test first
- **Green**: Write minimal code to pass
- **Refactor**: Improve while tests stay green

**Legend**:
- `[P]` = Can run in parallel with other `[P]` tasks
- `[C]` = Checkpoint - validate progress before continuing
- `[DB]` = Requires database migration

---

## Phase 1: Feature Setup & Structure

- [ ] Create feature folder structure: `Features/{FEATURE_NAME}/`
- [ ] Create feature DI registration file: `{FEATURE_NAME}ServiceInjection.cs`
- [ ] Update global DI registration in `ServiceDependencyInjections.cs`
- [ ] `[C]` Verify: Feature folder structure follows VSA pattern

---

## Phase 2: Database & Entities (TDD)

### Database Setup
- [ ] Create DbContext extension for new entities in `StockerDbContext`
- [ ] `[DB]` Generate EF Core migration: `dotnet ef migrations add Add{ENTITY}`
- [ ] `[DB]` Apply migration: `dotnet ef database update`

### Entity Implementation (TDD - Red-Green-Refactor)
- [ ] `[P]` **Red**: Write failing test for `{ENTITY}Configuration`
- [ ] `[P]` **Green**: Implement `{ENTITY}` entity class in `Models/Entities/`
- [ ] `[P]` **Refactor**: Clean up entity code, ensure tests pass
- [ ] `[P]` **Red**: Write failing test for another entity configuration
- [ ] `[P]` **Green**: Implement entity class
- [ ] `[P]` **Refactor**: Clean up entity code
- [ ] `[C]` **Checkpoint**: All entity tests pass, migrations applied successfully

---

## Phase 3: Services Layer (TDD)

### Service Tests First (Red)
- [ ] `[P]` Write failing test for `{SERVICE}.{METHOD}_Async` success case
- [ ] `[P]` Write failing test for `{SERVICE}.{METHOD}_Async` validation error
- [ ] `[P]` Write failing test for `{SERVICE}.{METHOD}_Async` not found case
- [ ] `[P]` Write failing test for `{SERVICE}.{METHOD}_Async` edge case

### Service Implementation (Green)
- [ ] Create service interface: `I{SERVICE}`
- [ ] Implement `{SERVICE}` class in `Services/`
- [ ] Implement business logic to make tests pass
- [ ] Register service in `{FEATURE}ServiceInjection.cs`

### Refactor & Additional Tests
- [ ] **Refactor**: Clean up service code while tests stay green
- [ ] Add tests for additional edge cases discovered during implementation
- [ ] Optimize database queries in service layer
- [ ] `[C]` **Checkpoint**: All service tests pass (>80% coverage)

---

## Phase 4: DTOs & Validators (TDD)

### DTO Definition
- [ ] `[P]` Create `{REQUEST}Dto` in `Models/Dtos/`
- [ ] `[P]` Create `{RESPONSE}Dto` in `Models/Dtos/`
- [ ] `[P]` Add XML documentation comments to DTOs

### Validator Tests (Red)
- [ ] `[P]` **Red**: Write failing test for validator happy path
- [ ] `[P]` **Red**: Write failing test for validator required field
- [ ] `[P]` **Red**: Write failing test for validator max length
- [ ] `[P]` **Red**: Write failing test for validator format validation

### Validator Implementation (Green)
- [ ] `[P]` Create `{REQUEST}Validator` in `Models/Validators/`
- [ ] `[P]` Implement FluentValidation rules
- [ ] `[P]` Make all validator tests pass

### Refactor
- [ ] **Refactor**: Clean up validator code
- [ ] Add custom validation logic if needed
- [ ] `[C]` **Checkpoint**: All validator tests pass (>90% coverage)

---

## Phase 5: Endpoints - REPR Pattern (TDD)

### Endpoint Tests First (Red)
- [ ] `[P]` **Red**: Write failing integration test for `{ENDPOINT}` success case
- [ ] `[P]` **Red**: Write failing integration test for `{ENDPOINT}` validation error
- [ ] `[P]` **Red**: Write failing integration test for `{ENDPOINT}` not found
- [ ] `[P]` **Red**: Write failing integration test for `{ENDPOINT}` error case

### Endpoint Implementation (Green)
- [ ] Create `{ENDPOINT}Endpoint` in `Endpoints/`
- [ ] Inherit from appropriate FastEndpoints base class
- [ ] Implement `Configure()` method with route, authorization, options
- [ ] Implement `HandleAsync()` method calling service layer
- [ ] Make all endpoint tests pass

### Additional Endpoints (Repeat TDD cycle)
- [ ] `[P]` **Red**: Write failing test for next endpoint
- [ ] `[P]` **Green**: Implement endpoint
- [ ] `[P]` **Refactor**: Clean up code
- [ ] Repeat for all endpoints in feature

### Refactor & Documentation
- [ ] **Refactor**: Extract common endpoint logic if applicable
- [ ] Add OpenAPI summaries and descriptions to all endpoints
- [ ] Add example values to DTO documentation
- [ ] `[C]` **Checkpoint**: All endpoint tests pass (>70% coverage)

---

## Phase 6: Integration & Configuration

### Feature Integration
- [ ] Verify feature is registered in DI container
- [ ] Test feature startup in development environment
- [ ] Verify Swagger UI shows all new endpoints

### Constants & Configuration
- [ ] Create `{FEATURE}Constants.cs` in `Constants/`
- [ ] Add feature-specific configuration values
- [ ] Update `appsettings.json` if needed

### OpenAPI Documentation
- [ ] Run backend application
- [ ] Verify Swagger UI displays all new endpoints
- [ ] Check request/response schemas are correct
- [ ] Verify error responses are documented
- [ ] `[C]` **Checkpoint**: API documentation complete and accurate

---

## Phase 7: TypeScript Type Sync

### Type Generation
- [ ] Ensure backend is running on HTTPS port
- [ ] Run `npm run codegen` to generate TypeScript types
- [ ] Verify `packages/api-contracts/index.ts` updated
- [ ] Build workspace to verify no type errors
- [ ] `[C]` **Checkpoint**: TypeScript types synced successfully

---

## Phase 8: Final Verification

### Test Suite
- [ ] Run all unit tests: `dotnet test --filter "FullyQualifiedName~Unit"`
- [ ] Run all integration tests: `dotnet test --filter "FullyQualifiedName~Integration"`
- [ ] Verify all tests pass

### Code Quality
- [ ] Run compiler to check for warnings
- [ ] Verify nullable reference types are enabled
- [ ] Check for proper async/await usage (no `.Result` or `.Wait()`)
- [ ] Verify cancellation tokens are passed correctly

### Performance Check
- [ ] Run endpoint tests and check response times
- [ ] Verify database queries are efficient (no N+1 queries)
- [ ] Check for proper use of indexes

### Security Review
- [ ] Verify authorization is configured correctly
- [ ] Check for proper input validation
- [ ] Verify no sensitive data in logs
- [ ] Check for SQL injection vulnerabilities (EF Core protects, but verify)

### Documentation
- [ ] Update CLAUDE.md with feature overview (if needed)
- [ ] Update API documentation (if custom endpoints)
- [ ] Create feature README if complex

---

## Phase 9: Cleanup & Handoff

### Code Cleanup
- [ ] Remove any TODO comments or temporary code
- [ ] Remove unused using statements
- [ ] Format all code files
- [ ] Run code cleanup tools if configured

### Git Preparation
- [ ] Review all changed files
- [ ] Verify no sensitive data in commits
- [ ] Create descriptive commit message
- [ ] `[C]` **Final Checkpoint**: Ready for code review and merge

---

## Task Execution Notes

### Parallel Execution Guidelines
- Tasks marked `[P]` can be executed in parallel
- Tasks without `[P]` must wait for preceding tasks to complete
- Checkpoints `[C]` must be validated before proceeding to next phase

### TDD Discipline
- **Never skip writing tests first**
- **Never implement code before tests fail**
- **Refactor only when tests are green**

### Database Migration Notes
- Always review generated migration code
- Test migrations on local database first
- Rollback plan: `dotnet ef database update {previous-migration}`

### Testing Guidelines
- Use descriptive test names: `Method_ExpectedBehavior_ExpectedResult`
- One assertion per test (when possible)
- Use test fixtures for common setup
- Mock external dependencies in unit tests

---

**Checkpoint #3: Human Review Required**

Before proceeding to `/speckit.implement`, verify:
- [ ] Are tasks in logical order?
- [ ] Is TDD followed (tests before implementation)?
- [ ] Are checkpoints included?
- [ ] Are all critical paths covered?
- [ ] Is the task breakdown complete?
- [ ] Are parallel tasks marked correctly?
