# Spec Kit Quick Start Guide - Stocker Project

## Overview

This guide will help you get started with Spec Kit (Spec-Driven Development) in the Stocker project. You'll learn how to create features systematically using the structured workflow.

## Prerequisites

Before starting, ensure you have:
- ✅ Spec Kit installed and initialized (already done)
- ✅ Claude Code running in the project directory
- ✅ Backend can run on `https://localhost:7001`
- ✅ Frontend can run on `http://localhost:5173`

## Workflow Overview

```
Constitution → Specify → Plan → Tasks → Implement
   (one-time)      (what)     (how)    (how)     (do)
```

Each step has a **human review checkpoint** to ensure quality.

---

## Phase 0: Project Constitution (One-Time Setup)

The constitution defines project principles. It's already created for Stocker.

### View Constitution

```bash
/speckit.constitution
```

This will display the `.specify/memory/constitution.md` with:
- Architecture principles (VSA, REPR, TDD)
- Technology stack
- Development workflow
- Quality standards

### Update Constitution (if needed)

If you need to add principles:

```bash
/speckit.constitution Add principle: All API responses must include correlation ID for tracing
```

---

## Phase 1: Define a Feature (Specify)

### Example: Create a Watchlist Feature

```bash
/speckit.specify 
Create a watchlist management feature for Stocker:
- Users can create personal watchlists
- Add/remove stocks to watchlists
- Maximum 10 watchlists per user
- Each watchlist can hold up to 50 stocks
- View watchlist with real-time stock data
- Watchlists persist to database
```

### What Gets Created

- `specs/001-watchlist-management/spec.md` with:
  - User stories
  - Functional requirements
  - API endpoints
  - Database schema
  - Acceptance criteria

### 🔍 Checkpoint #1: Review the Spec

**Before proceeding, review `specs/001-watchlist-management/spec.md`:**

1. Are requirements complete?
2. Are user stories clear?
3. Are acceptance criteria testable?
4. Are edge cases identified?

**Refine if needed:**

```bash
/speckit.specify
Add clarifications:
- 11th watchlist returns 400 with "Maximum 10 watchlists allowed" message
- 51st stock returns 400 with "Maximum 50 stocks per watchlist" message
- Watchlist names can be duplicated (users can organize how they want)
```

---

## Phase 2: Create Technical Plan

### Example: Plan the Backend Implementation

```bash
/speckit.plan
Tech stack:
- .NET 10 with FastEndpoints (REPR pattern)
- EF Core with PostgreSQL for production, SQLite for development
- xUnit for testing
- Follow existing VSA structure in StockRanking feature

API Endpoints:
- POST /api/watchlists - Create new watchlist
- GET /api/watchlists/{id} - Get watchlist details
- POST /api/watchlists/{id}/stocks - Add stock to watchlist
- DELETE /api/watchlists/{id}/stocks/{symbol} - Remove stock

Database Schema:
- Watchlist table: Id, Name, UserId, CreatedAt
- WatchlistStock table: Id, WatchlistId, Symbol, AddedAt
```

### What Gets Created

- `specs/001-watchlist-management/plan.md` with:
  - Architecture overview
  - Implementation details
  - Database schema
  - Service layer design
  - Testing strategy

### 🔍 Checkpoint #2: Review the Plan

**Before proceeding, review `specs/001-watchlist-management/plan.md`:**

1. Is the tech stack appropriate?
2. Is the database schema correct?
3. Are the API endpoints RESTful?
4. Are security concerns addressed?

**Refine if needed:**

```bash
/speckit.plan
Changes:
- Use SQLite for development (simpler)
- Add indexes on WatchlistId and UserId for performance
- Add FluentValidation for request validation
```

---

## Phase 3: Generate Tasks

### Example: Break Down into Implementation Tasks

```bash
/speckit.tasks
```

### What Gets Created

- `specs/001-watchlist-management/tasks.md` with:
  - TDD-ordered task list
  - Parallel execution markers
  - Checkpoint validations
  - File paths for implementation

### 🔍 Checkpoint #3: Review the Tasks

**Before proceeding, review `specs/001-watchlist-management/tasks.md`:**

1. Are tasks in logical order?
2. Is TDD followed (tests before implementation)?
3. Are all critical paths covered?
4. Are checkpoints included?

**Edit tasks.md if needed:**

You can manually add missing tasks or reorder them:

```markdown
## Additional Tasks

- [ ] Update ServiceDependencyInjections.cs to register WatchlistManagement
- [ ] Add Swagger documentation for all endpoints
- [ ] Verify TypeScript types are synced after implementation
```

---

## Phase 4: Implement the Feature

### Example: Execute Implementation

```bash
/speckit.implement
```

### What Happens

Claude Code will:
1. Parse `tasks.md`
2. Execute tasks in order
3. Create all necessary files
4. Run tests
5. Report progress

### Files Created

Following the VSA structure:

```
Features/WatchlistManagement/
├── WatchlistManagementServiceInjection.cs
├── Endpoints/
│   ├── CreateWatchlistEndpoint.cs
│   ├── GetWatchlistEndpoint.cs
│   ├── AddStockEndpoint.cs
│   └── RemoveStockEndpoint.cs
├── Services/
│   ├── WatchlistService.cs
│   └── TradingViewDataFetcher.cs
├── Models/
│   ├── Entities/
│   │   ├── Watchlist.cs
│   │   └── WatchlistStock.cs
│   ├── Dtos/
│   │   ├── CreateWatchlistRequestDto.cs
│   │   └── WatchlistResponseDto.cs
│   └── Validators/
│       └── CreateWatchlistValidator.cs
├── Tests/
│   ├── Unit/
│   │   └── Services/
│   └── Integration/
│       └── Endpoints/
└── Constants/
    └── WatchlistConstants.cs

Migrations/
└── 20250609_AddWatchlistTables.cs
```

### 🔍 Checkpoint #4: Review the Implementation

**After implementation completes:**

1. **Run tests:**
   ```bash
   dotnet test
   ```

2. **Check compilation:**
   ```bash
   dotnet build
   ```

3. **Verify Swagger UI:**
   - Open `https://localhost:7001/swagger`
   - Check endpoints are documented
   - Try endpoints manually

4. **Check database:**
   ```bash
   # View tables created
   dotnet ef database update
   ```

5. **Sync TypeScript types:**
   ```bash
   npm run codegen
   ```

6. **Build frontend:**
   ```bash
   npm run build
   ```

---

## Complete Example Workflow

### Backend Feature Example

```bash
# 1. Define the feature
/speckit.specify Create a portfolio tracking feature where users can track their stock holdings and calculate returns...

# 2. Review spec
# Read: specs/002-portfolio-tracking/spec.md
# Edit if needed

# 3. Create plan
/speckit.plan Use .NET 10, FastEndpoints, EF Core with SQLite...

# 4. Review plan
# Read: specs/002-portfolio-tracking/plan.md
# Edit if needed

# 5. Generate tasks
/speckit.tasks

# 6. Review tasks
# Read: specs/002-portfolio-tracking/tasks.md
# Edit if needed

# 7. Implement
/speckit.implement

# 8. Verify
dotnet test
dotnet build
npm run codegen
npm run build
```

### Frontend Feature Example

```bash
# 1. Define the UI feature
/speckit.specify Create a watchlist UI component with search, filter, and real-time updates...

# 2. Review spec
# Read: specs/003-watchlist-ui/spec.md
# Edit if needed

# 3. Create plan
/speckit.plan Use React 19, TypeScript, React Query, component-based architecture...

# 4. Review plan
# Read: specs/003-watchlist-ui/plan.md
# Edit if needed

# 5. Generate tasks
/speckit.tasks

# 6. Review tasks
# Read: specs/003-watchlist-ui/tasks.md
# Edit if needed

# 7. Implement
/speckit.implement

# 8. Verify
npm run test
npm run build
```

---

## Custom Presets

### Using .NET FastEndpoints Preset

The project includes a custom preset optimized for .NET FastEndpoints:

```bash
# The preset is automatically used when you run:
/speckit.specify Create backend feature...

# Templates include:
# - API endpoint definitions
# - Database schema considerations
# - TDD test structure
# - EF Core migration steps
```

### Using React + TypeScript Preset

The project includes a custom preset optimized for React:

```bash
# The preset is automatically used when you run:
/speckit.specify Create frontend UI feature...

# Templates include:
# - Component structure
# - Hook patterns
# - TypeScript type safety
# - Testing strategies
```

---

## Troubleshooting

### Common Issues

#### Issue: "Spec Kit command not found"

**Solution**: Ensure Spec Kit is initialized:
```bash
specify check
```

#### Issue: "Backend not accessible when syncing types"

**Solution**: Ensure backend is running:
```bash
# In backend folder
dotnet watch run --launch-profile https

# Then sync types
npm run codegen
```

#### Issue: "Tests failing after implementation"

**Solution**: Review test failures and refine implementation:
```bash
dotnet test --logger "console;verbosity=detailed"
```

#### Issue: "Build errors after type sync"

**Solution**: Check for type mismatches between frontend and backend:
```bash
# Review generated types
cat packages/api-contracts/index.ts

# Rebuild after fixes
npm run build
```

### Getting Help

- Review [CLAUDE.md](../CLAUDE.md) for detailed documentation
- Check [Project Constitution](memory/constitution.md) for principles
- Read [AI Workflow Plan](../../ai-workflow/spec-driven-development-plan.md) for complete workflow

---

## Tips for Success

### Before You Start

1. **Understand the requirement**: Know what you're building
2. **Review existing patterns**: Look at similar features (StockRanking)
3. **Plan your approach**: Think about architecture before coding

### During Development

1. **Follow TDD**: Write tests before implementation
2. **Review at checkpoints**: Don't skip human reviews
3. **Run tests frequently**: Catch issues early
4. **Refactor continuously**: Improve code while tests pass

### After Implementation

1. **Test manually**: Verify feature works end-to-end
2. **Check performance**: Ensure acceptable response times
3. **Review code**: Check for quality and patterns
4. **Document**: Add comments if logic is complex

---

## Next Steps

Now that you've completed this quick start:

1. ✅ Try creating a simple feature using the workflow
2. ✅ Review the generated artifacts (spec, plan, tasks)
3. ✅ Experiment with human review checkpoints
4. ✅ Explore custom presets and extensions
5. ✅ Build your first complete feature with Spec Kit

---

## Additional Resources

- [Spec Kit Documentation](https://github.com/github/spec-kit)
- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/core/)
- [React Documentation](https://react.dev/)

---

**Quick Start Guide Version**: 1.0  
**Last Updated**: 2025-06-09  
**For questions or issues**, refer to the project documentation or create an issue in the repository.
