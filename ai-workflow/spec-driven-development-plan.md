# Plan: Spec-Driven Development Workflow for Stocker

## Context

The Stocker project is a well-architected full-stack application with:
- **Backend**: .NET 10 with FastEndpoints (REPR pattern), Vertical Slice Architecture
- **Frontend**: React 19 + TypeScript with Vite
- **Monorepo**: Turborepo for build orchestration
- **Shared types**: OpenAPI code generation

The user wants to create a workflow for Claude agents that enables systematic feature development, similar to **Spec Kit** (GitHub's Spec-Driven Development toolkit), but tailored to Stocker's specific architecture patterns.

## Analysis

### Spec Kit Overview

Spec Kit provides a structured development process:
- `/speckit.constitution` - Project principles
- `/speckit.specify` - Define what to build (requirements, user stories)
- `/speckit.plan` - Technical implementation plan
- `/speckit.tasks` - Actionable task breakdown
- `/speckit.implement` - Execute implementation

### Stocker's Existing Patterns

The backend already follows clear patterns:
1. **Feature-based organization** - Each feature is a self-contained folder
2. **Vertical Slice Architecture** - Endpoints, Services, Models co-located
3. **REPR Pattern** - Request-Endpoint-Response with FastEndpoints
4. **Dependency Injection Hierarchy** - Global → Feature-specific

## Human-in-the-Loop Workflow

**Critical Quality Gates:** Human reviews at strategic points ensure requirements and implementation align with expectations.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SPEC KIT WORKFLOW + HUMAN REVIEWS                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ✅ CONSTITUTION (one-time)                                                 │
│     ↓                                                                       │
│  📝 SPECIFY        ──→  🔍 CHECKPOINT #1: Requirements Validation          │
│     ↓                      ✓ Is this what we want?                          │
│  🏗️  PLAN           ──→  🔍 CHECKPOINT #2: Technical Approach             │
│     ↓                      ✓ Architecture sound?                           │
│  📋 TASKS          ──→  🔍 CHECKPOINT #3: Implementation Strategy        │
│     ↓                      ✓ Ready to code?                                │
│  ⚙️  IMPLEMENT       ──→  🔍 CHECKPOINT #4: Code Review (optional)        │
│     ↓                      ✓ Meets spec?                                    │
│  ✅ VERIFICATION          └─→ End                                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Checkpoint #1: After `/speckit.specify` ⭐ **MOST CRITICAL**

**Why:** Defines WHAT you're building - mistakes propagate everywhere

**Human Review Questions:**
- ✓ Are the requirements complete and correct?
- ✓ Are user stories capturing real needs?
- ✓ Acceptance criteria testable?
- ✓ Missing edge cases?
- ✓ Business rules clear?

**Example:**
```bash
/speckit.specify Create watchlist feature...

# Review spec.md, then refine:
/speckit.specify (refinement)
- 11th watchlist returns 400 error
- Watchlists are private (no sharing)
- Duplicate names allowed
```

### Checkpoint #2: After `/speckit.plan` ⭐ **IMPORTANT FOR ARCHITECTURE**

**Why:** Defines HOW you'll build it - architectural decisions

**Human Review Questions:**
- ✓ Is the tech stack appropriate?
- ✓ Database schema correct?
- ✓ API design RESTful?
- ✓ External dependencies justified?
- ✓ Performance considerations addressed?
- ✓ Security concerns covered?

### Checkpoint #3: After `/speckit.tasks` ⭐ **IMPORTANT BEFORE CODING**

**Why:** Last chance to catch issues before code generation

**Human Review Questions:**
- ✓ Tasks in logical order?
- ✓ Dependencies correct?
- ✓ Tests before implementation (TDD)?
- ✓ Missing tasks?
- ✓ Checkpoints validate progress?

### Checkpoint #4: After `/speckit.implement` (Optional) ⭐ **QUALITY GATE**

**Why:** Validate implementation matches spec

**Human Review Questions:**
- ✓ Code compiles?
- ✓ Tests pass?
- ✓ API works in Swagger UI?
- ✓ Edge cases handled?
- ✓ Error messages appropriate?

## Complete Feature Development Workflow

```bash
# ═══════════════════════════════════════════════════════════════
# FEATURE DEVELOPMENT WITH HUMAN REVIEWS
# ═══════════════════════════════════════════════════════════════

# 0. SETUP (one-time)
/speckit.constitution Create .NET FastEndpoints constitution with TDD, VSA, REPR

# 1. DEFINE FEATURE
/speckit.specify Create watchlist feature: users create personal watchlists...

# 🔍 CHECKPOINT #1: Review spec.md - Is this what we want?

# 2. TECHNICAL PLAN  
/speckit.plan Use .NET 10, FastEndpoints, EF Core + PostgreSQL...

# 🔍 CHECKPOINT #2: Review plan.md - Is this how to build it?

# 3. TASK BREAKDOWN
/speckit.tasks

# 🔍 CHECKPOINT #3: Review tasks.md - Implementation strategy correct?

# 4. IMPLEMENT
/speckit.implement

# 🔍 CHECKPOINT #4: Review implementation - Code compiles? Tests pass? API works?

# ═══════════════════════════════════════════════════════════════
```

## Proposed Approach

### Option A: Adopt Spec Kit with Custom Preset (Recommended)

**Why this is the best approach:**
- ✅ Leverages battle-tested Spec Kit workflow
- ✅ Can create custom preset for .NET/FastEndpoints patterns
- ✅ Supports extensions for additional capabilities
- ✅ Community-driven with ongoing maintenance
- ✅ Agent-agnostic (works with Claude, Copilot, Cursor, etc.)

**Implementation:**
1. Install Spec Kit via Specify CLI
2. Create a custom preset called `dotnet-fastendpoints` that includes:
   - Spec template optimized for API features
   - Plan template with .NET architecture patterns
   - Tasks template following VSA principles
3. Create feature scaffolding templates that match existing patterns
4. Optional: Create extensions for common operations (e.g., `/scaffold-feature`)

### Option B: Custom Workflow Script

**Pros:**
- ✅ Full control over workflow
- ✅ Can be highly specific to Stocker

**Cons:**
- ❌ Reinvents the wheel (Spec Kit already exists)
- ❌ Maintenance burden
- ❌ Less agent-agnostic

### Option C: Hybrid - Spec Kit + Custom Extensions

**Best of both worlds:**
- Use Spec Kit for core workflow (spec, plan, tasks, implement)
- Create custom extensions for Stocker-specific operations:
  - `/scaffold-feature` - Generate feature folder structure
  - `/add-endpoint` - Create new endpoint following REPR pattern
  - `/add-service` - Create service with DI registration
  - `/sync-contracts` - Regenerate TypeScript types from OpenAPI

## Recommendation: Option A with Option C Extensions

**Phase 1: Foundation (Spec Kit adoption)**
1. Initialize Spec Kit in Stocker project
2. Create project constitution with .NET best practices
3. Set up custom preset for FastEndpoints patterns

**Phase 2: Custom Extensions**
1. Create feature scaffolding extension
2. Create endpoint/service generation extensions
3. Create OpenAPI sync extension

**Phase 3: Documentation & Training**
1. Document workflow in CLAUDE.md
2. Create example feature following new workflow
3. Validate workflow with real feature development

## Critical Files to Create/Modify

### New Files
- `.specify/constitution.md` - Project principles
- `.specify/presets/dotnet-fastendpoints/spec-template.md`
- `.specify/presets/dotnet-fastendpoints/plan-template.md`
- `.specify/presets/dotnet-fastendpoints/tasks-template.md`
- `.specify/extensions/feature-scaffold/`
- `.specify/extensions/openapi-sync/`
- `CLAUDE.md` - Update with new workflow documentation

### Templates to Create
Feature scaffolding templates matching existing patterns:
- `Features/{FeatureName}/`
  - `{FeatureName}ServiceInjection.cs`
  - `Endpoints/` (REPR pattern)
  - `Services/` (Business logic)
  - `Models/` (DTOs, domain models)
  - `Constants/` (Configuration)

## Verification

After implementation, verify by:
1. Running `specify init` to confirm Spec Kit integration
2. Using `/speckit.specify` to create a new feature spec
3. Using `/speckit.plan` to generate technical plan
4. Using `/speckit.tasks` to create task breakdown
5. Using `/speckit.implement` to build the feature
6. Validating generated code matches existing patterns
7. Testing the feature end-to-end

## User Requirements (Clarified)

✅ **Full automation** - Generate entire feature folder structure from a single command
✅ **Yes, full TDD** - Generate unit tests and integration tests following TDD principles
✅ **Separate workflows** - Create distinct workflows for backend and frontend features
✅ **Yes, EF Core migrations** - Include EF Core migration generation for database changes

## Detailed Implementation Plan

### Phase 1: Foundation - Spec Kit Setup

**1.1 Install Spec Kit**
```bash
# Install Specify CLI (persistent)
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git@latest

# Initialize in Stocker project
specify init . --integration claude --force
```

**1.2 Create Project Constitution**
Run `/speckit.constitution` to establish:
- .NET 10 + FastEndpoints architectural principles
- Vertical Slice Architecture (VSA) guidelines
- REPR pattern standards
- TDD requirements (xUnit/xUnit.net for .NET)
- EF Core migration conventions
- API contract-first development (OpenAPI/Swagger)

**1.3 Create Custom Preset: `dotnet-fastendpoints`**

Structure:
```
.specify/presets/dotnet-fastendpoints/
├── preset.json              # Preset metadata
├── templates/
│   ├── spec-template.md     # Feature spec template (optimized for API features)
│   ├── plan-template.md     # Technical plan template (.NET architecture)
│   └── tasks-template.md    # Task breakdown template (TDD-focused)
└── catalog.json             # Preset catalog entry
```

**Spec Template Features:**
- API endpoint definition (HTTP method, route, authorization)
- Request/Response DTO structure
- Business requirements
- Acceptance criteria
- Database schema (if applicable)
- External dependencies

**Plan Template Features:**
- .NET project structure guidance
- FastEndpoints REPR pattern reference
- Service layer architecture
- EF Core DbContext and entity design
- DI registration patterns
- OpenAPI documentation requirements
- Test project structure

**Tasks Template Features:**
- TDD-ordered task list (tests before implementation)
- Parallel execution markers for independent tasks
- Checkpoint validation after each user story
- EF Core migration generation steps
- Test project setup and execution

### Phase 2: Custom Extension - Feature Scaffolding

**2.1 Create Extension: `feature-scaffold`**

Extension structure:
```
.specify/extensions/feature-scaffold/
├── extension.json           # Extension metadata
├── templates/
│   ├── feature.md           # Feature scaffold template
│   └── endpoint.md          # Individual endpoint template
├── scripts/
│   ├── scaffold-feature.sh   # Main scaffolding script
│   └── scaffold-endpoint.sh  # Endpoint-specific script
└── catalog.json             # Extension catalog entry
```

**Scaffold Script Capabilities:**
The `/speckit.scaffold-feature` command will:

1. **Parse feature spec** - Extract endpoint, entities, and requirements from spec.md
2. **Generate folder structure:**
   ```
   Features/{FeatureName}/
   ├── {FeatureName}ServiceInjection.cs    # DI registration
   ├── Endpoints/
   │   └── {Endpoint}Endpoint.cs           # REPR endpoint
   ├── Services/
   │   ├── {FeatureName}Service.cs        # Main business logic
   │   ├── {External}Service.cs           # External API/dependency (if needed)
   │   └── {Calculator/Processor}.cs       # Domain logic (if needed)
   ├── Models/
   │   ├── Entities/                      # EF Core entities (if DB needed)
   │   │   └── {Entity}.cs
   │   ├── Dtos/                          # Request/Response DTOs
   │   │   ├── {Request}Dto.cs
   │   │   └── {Response}Dto.cs
   │   ├── DomainModels/                  # Internal domain models
   │   └── Validators/                    # FluentValidation validators (if needed)
   ├── Tests/                             # Feature-specific tests
   │   ├── Unit/
   │   │   ├── Services/
   │   │   └── Validators/
   │   └── Integration/
   │       └── Endpoints/
   └── Constants/
       └── {FeatureName}Constants.cs      # Configuration/constants
   ```

3. **Generate code files:**
   - All classes with proper namespace, base classes, and stubs
   - EF Core entity configuration (if DB entities)
   - FluentValidation validators (if validation needed)
   - xUnit test fixtures and test classes
   - Integration test setup with WebApplicationFactory

4. **Register DI:**
   - Update `ServiceDependencyInjections.cs` to include new feature
   - Generate `{FeatureName}ServiceInjection.cs` with all service registrations

5. **Generate EF Core Migration:**
   - Create initial migration file
   - Update DbContext if new entities added

**2.2 Create Extension: `openapi-sync`**

For syncing TypeScript types from backend OpenAPI spec:
```
.specify/extensions/openapi-sync/
├── extension.json
├── scripts/
│   └── sync-types.sh       # Run openapi-typescript and update api-contracts
└── catalog.json
```

### Phase 3: Frontend Workflow (Separate)

**3.1 Create Frontend Preset: `react-typescript`**

Structure:
```
.specify/presets/react-typescript/
├── preset.json
├── templates/
│   ├── spec-template.md     # UI/UX feature spec template
│   ├── plan-template.md     # React component architecture
│   └── tasks-template.md    # Frontend task breakdown
└── catalog.json
```

**3.2 Create Frontend Extension: `component-scaffold`**

Generate React components with:
```
apps/frontend/src/features/{FeatureName}/
├── components/
│   ├── {FeatureName}Page.tsx        # Main page component
│   ├── {FeatureName}List.tsx       # List/table component
│   ├── {FeatureName}Item.tsx        # Item component
│   └── {FeatureName}Form.tsx       # Form component (if CRUD)
├── hooks/
│   ├── use{FeatureName}.ts         # Custom hook
│   └── use{FeatureName}Api.ts      # API hook (react-query/swr)
├── services/
│   └── {featureName}Api.ts         # API client (typed)
├── types/
│   └── {featureName}.ts            # Local types (extends api-contracts)
└── tests/
    └── {featureName}.test.tsx       # Component tests
```

### Phase 4: Documentation & Validation

**4.1 Update CLAUDE.md**
Add comprehensive workflow documentation:
- Spec Kit commands reference
- Feature development workflow
- TDD requirements and patterns
- EF Core migration guidelines
- Common pitfalls and solutions

**4.2 Create Example Feature**
Build a complete example following the new workflow:
- Create feature spec using `/speckit.specify`
- Generate plan using `/speckit.plan`
- Break down tasks using `/speckit.tasks`
- Scaffold feature using `/speckit.scaffold-feature`
- Implement using `/speckit.implement`
- Run tests and verify
- Sync TypeScript types using `/speckit.sync-types`

**4.3 Create Quick Start Guide**
Add `.specify/quickstart.md` with:
- Installation steps
- First feature walkthrough
- Command reference
- Troubleshooting

## Critical Files to Create/Modify

### New Files
- `.specify/memory/constitution.md` - Project principles
- `.specify/presets/dotnet-fastendpoints/` - Backend preset
- `.specify/presets/react-typescript/` - Frontend preset
- `.specify/extensions/feature-scaffold/` - Backend scaffolding
- `.specify/extensions/component-scaffold/` - Frontend scaffolding
- `.specify/extensions/openapi-sync/` - Type sync
- `CLAUDE.md` - Update with new workflow
- `.specify/quickstart.md` - Quick start guide

### Files to Modify
- `apps/backend/Features/ServiceDependencyInjection.cs` - Add new feature registration
- `apps/backend/Features/StockRanking/` - Reference for patterns
- `apps/frontend/` - Add features folder structure
- `packages/api-contracts/` - Generated types

## Verification Steps

After implementation, verify by:

1. **Run Spec Kit initialization:**
   ```bash
   specify init . --integration claude --force
   ```

2. **Create constitution:**
   ```
   /speckit.constitution Create principles for .NET FastEndpoints development with TDD
   ```

3. **Create a new feature spec:**
   ```
   /speckit.specify Create a feature for managing watchlists - users can add/remove stocks to personal watchlists
   ```

4. **Generate technical plan:**
   ```
   /speckit.plan Use .NET 10, EF Core, PostgreSQL, FastEndpoints, xUnit
   ```

5. **Break down into TDD tasks:**
   ```
   /speckit.tasks
   ```

6. **Scaffold feature:**
   ```
   /speckit.scaffold-feature
   ```

7. **Implement feature:**
   ```
   /speckit.implement
   ```

8. **Verify:**
   - Feature folder structure matches expected pattern
   - All code files compile without errors
   - Tests run and pass (or fail appropriately with TDD red-green)
   - EF Core migration generated (if DB changes)
   - Endpoint appears in Swagger UI
   - TypeScript types synced to api-contracts
   - Frontend can consume the new endpoint

## Success Criteria

✅ Spec Kit commands work in Claude Code
✅ Custom preset applies .NET/FastEndpoints patterns
✅ Feature scaffolding generates complete folder structure
✅ TDD tests generated before implementation
✅ EF Core migrations included for database changes
✅ TypeScript types synced automatically
✅ Example feature demonstrates end-to-end workflow
✅ Documentation enables team to use workflow independently

## Implementation Timeline Estimate

- **Phase 1 (Foundation):** 2-3 hours
  - Install and configure Spec Kit
  - Create constitution
  - Build custom preset templates

- **Phase 2 (Backend Extension):** 3-4 hours
  - Create feature-scaffold extension
  - Write scaffolding scripts
  - Add EF Core migration support
  - Create openapi-sync extension

- **Phase 3 (Frontend Workflow):** 2-3 hours
  - Create React preset
  - Build component-scaffold extension
  - Integrate with api-contracts

- **Phase 4 (Documentation & Validation):** 2-3 hours
  - Update CLAUDE.md
  - Create example feature
  - Write quickstart guide
  - End-to-end testing

**Total: 9-13 hours**

## Next Steps

Once approved, implementation will proceed in phases:
1. Initialize Spec Kit and create custom preset
2. Build feature scaffolding extensions with TDD and EF Core support
3. Create separate frontend workflow for React components
4. Document workflow comprehensively and validate with example feature
