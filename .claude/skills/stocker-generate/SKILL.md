# Stocker Feature Generation Skill

This skill automates the complete Spec Kit workflow for Stocker features with both backend and frontend specifications, including implementation.

## Usage

```bash
/stocker-generate Create a health check API endpoint with status, version, and database connectivity
```

## Process

### Phase 1: Specification (/speckit.specify)

1. Generate feature specification from user description
2. 🔍 **Checkpoint**: Review spec before proceeding
3. Optional: Run `/speckit.clarify` if ambiguities exist

### Phase 2: Backend Plan (/speckit.plan --preset dotnet-fastendpoints)

1. Create backend technical plan using .NET FastEndpoints preset
2. 🔍 **Checkpoint**: Review backend plan before proceeding

### Phase 3: Backend Tasks (/speckit.tasks --preset dotnet-fastendpoints)

1. Generate backend implementation tasks with TDD ordering
2. 🔍 **Checkpoint**: Review backend tasks before proceeding

### Phase 4: Frontend Plan (/speckit.plan --preset react-typescript)

1. Create frontend technical plan using React + TypeScript preset
2. 🔍 **Checkpoint**: Review frontend plan before proceeding

### Phase 5: Frontend Tasks (/speckit.tasks --preset react-typescript)

1. Generate frontend implementation tasks
2. 🔍 **Checkpoint**: Review frontend tasks before proceeding

### Phase 6: Analysis (/speckit.analyze)

1. Run cross-artifact consistency analysis on all artifacts (spec, plans, tasks)
2. 🔍 **Checkpoint**: Review analysis results
3. Address any identified issues before implementation

### Phase 7: Backend Implementation (/speckit.implement --preset dotnet-fastendpoints)

1. Execute backend implementation using .NET FastEndpoints preset
2. Generate all backend feature files (endpoints, services, entities, tests)
3. 🔍 **Checkpoint**: Review backend implementation

### Phase 8: Type Generation (npm run codegen)

1. Generate TypeScript types from backend OpenAPI specification
2. Update `@stocker/api-contracts` package
3. 🔍 **Checkpoint**: Verify type generation

### Phase 9: Frontend Implementation (/speckit.implement --preset react-typescript)

1. Execute frontend implementation using React + TypeScript preset
2. Generate all frontend feature files (components, hooks, services)
3. 🔍 **Checkpoint**: Review frontend implementation

### Phase 10: Verification

1. Run backend tests: `dotnet test`
2. Run frontend tests: `npm run test`
3. Build workspace: `npm run build`
4. 🔍 **Checkpoint**: Verify all tests pass and build succeeds

## Instructions

When this skill is invoked:

1. **Extract feature description** from user input

2. **Run specification phase**:
   - Execute: `/speckit.specify [feature description]`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review the generated specification. Is it ready to proceed?"
     - Options: ["Approve & Continue", "Run Clarify", "Make Changes"]
   - If "Run Clarify" selected:
     - Execute: `/speckit.clarify`
     - Re-checkpoint: "Clarification complete. Ready to proceed?"
   - If "Make Changes" selected:
     - Ask user: "What changes would you like me to make to the spec?"
     - Make the requested edits using file tools
     - Re-checkpoint: "Changes added. Ready to proceed?"
     - If user selects "Make Changes" again, repeat this step
   - Only proceed to next phase on approval

3. **Run backend planning phase**:
   - Execute: `/speckit.plan --preset dotnet-fastendpoints`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review the backend technical plan. Is it ready to proceed?"
     - Options: ["Approve & Continue", "Make Changes"]
   - If "Make Changes" selected:
     - Ask user: "What changes would you like me to make to the backend plan?"
     - Make the requested edits using file tools
     - Re-checkpoint: "Changes added. Ready to proceed?"
     - If user selects "Make Changes" again, repeat this step

4. **Run backend tasks phase**:
   - Execute: `/speckit.tasks --preset dotnet-fastendpoints`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review the backend implementation tasks. Are they ready to proceed?"
     - Options: ["Approve & Continue", "Make Changes"]
   - If "Make Changes" selected:
     - Ask user: "What changes would you like me to make to the backend tasks?"
     - Make the requested edits using file tools
     - Re-checkpoint: "Changes added. Ready to proceed?"
     - If user selects "Make Changes" again, repeat this step

5. **Run frontend planning phase**:
   - Execute: `/speckit.plan --preset react-typescript`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review the frontend technical plan. Is it ready to proceed?"
     - Options: ["Approve & Continue", "Make Changes"]
   - If "Make Changes" selected:
     - Ask user: "What changes would you like me to make to the frontend plan?"
     - Make the requested edits using file tools
     - Re-checkpoint: "Changes added. Ready to proceed?"
     - If user selects "Make Changes" again, repeat this step

6. **Run frontend tasks phase**:
   - Execute: `/speckit.tasks --preset react-typescript`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review the frontend implementation tasks. Are they ready to proceed?"
     - Options: ["Approve & Continue", "Make Changes"]
   - If "Make Changes" selected:
     - Ask user: "What changes would you like me to make to the frontend tasks?"
     - Make the requested edits using file tools
     - Re-checkpoint: "Changes added. Ready to proceed?"
     - If user selects "Make Changes" again, repeat this step

7. **Run analysis phase** (AFTER all artifacts are created):
   - Execute: `/speckit.analyze`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review cross-artifact analysis. Any inconsistencies to address?"
     - Options: ["No Issues - Ready to Implement", "Address Issues", "Skip Analysis"]
   - If "Address Issues" selected:
     - Ask user: "Which issues should I address?"
     - Make the requested fixes to the identified artifacts
     - Execute: `/speckit.analyze` again to verify fixes
     - Re-checkpoint: "Fixes applied. Any remaining issues?"
     - If user selects "Address Issues" again, repeat this step
   - If "Skip Analysis" selected: Proceed to implementation phase

8. **Run backend implementation phase**:
   - Execute: `/speckit.implement --preset dotnet-fastendpoints`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Review backend implementation. Did code generation succeed?"
     - Options: ["Approved - Continue to Codegen", "Fix Issues"]
   - If "Fix Issues" selected:
     - Ask user: "What issues need to be fixed in the backend implementation?"
     - Make the requested fixes using file tools
     - Re-checkpoint: "Fixes applied. Ready to proceed to codegen?"
     - If user selects "Fix Issues" again, repeat this step

9. **Run type generation phase** (IMPORTANT: After backend implementation):
   - Execute: `npm run codegen`
   - Use `AskUserQuestion` to present checkpoint:
     - Question: "Type generation complete. Are TypeScript types synced correctly?"
     - Options: ["Types Synced - Continue", "Fix Issues"]
   - If "Fix Issues" selected:
     - Ask user: "What codegen issues need to be addressed?"
     - Check if backend is running on https://localhost:7001
     - Fix the identified issues (may require backend fixes or manual adjustments)
     - Execute: `npm run codegen` again to verify
     - Re-checkpoint: "Fixes applied. Types now synced?"
     - If user selects "Fix Issues" again, repeat this step

10. **Run frontend implementation phase**:
    - Execute: `/speckit.implement --preset react-typescript`
    - Use `AskUserQuestion` to present checkpoint:
      - Question: "Review frontend implementation. Did code generation succeed?"
      - Options: ["Approved - Continue to Verification", "Fix Issues"]
    - If "Fix Issues" selected:
      - Ask user: "What issues need to be fixed in the frontend implementation?"
      - Make the requested fixes using file tools
      - Re-checkpoint: "Fixes applied. Ready to proceed to verification?"
      - If user selects "Fix Issues" again, repeat this step

11. **Run verification phase**:
    - Execute: `dotnet test` and capture results
    - Execute: `npm run test` and capture results
    - Execute: `npm run build` and capture results
    - Use `AskUserQuestion` to present checkpoint:
      - Question: "Verification complete. Do all tests pass and build succeed?"
      - Options: ["All Passed - Workflow Complete", "Fix Issues"]
    - If "Fix Issues" selected:
      - Display which tests failed or build errors occurred
      - Ask user: "What issues should I fix?"
      - Make the requested fixes using file tools
      - Execute verification commands again
      - Re-checkpoint: "Fixes applied. All passing now?"
      - If user selects "Fix Issues" again, repeat this step

12. **Complete workflow**:
    - Summarize all generated artifacts
    - Confirm feature is ready for use

## Output

All artifacts are generated in the appropriate directories:

### Specification Artifacts (in `specs/` directory):
- `spec.md` - Feature specification
- `plan.md` (backend) - Backend technical plan
- `tasks.md` (backend) - Backend implementation tasks
- `plan.md` (frontend) - Frontend technical plan
- `tasks.md` (frontend) - Frontend implementation tasks
- `checklists/` - Quality checklists

### Implementation Artifacts:
- **Backend** (`apps/backend/Features/{FeatureName}/`):
  - Endpoints/ - API endpoints (REPR pattern)
  - Services/ - Business logic
  - Models/ - Entities, DTOs, validators
  - Tests/ - Unit and integration tests
  - Constants/ - Configuration constants

- **Frontend** (`apps/frontend/src/features/{FeatureName}/`):
  - components/ - React components
  - hooks/ - Custom hooks
  - services/ - API clients
  - types/ - Local type definitions
  - tests/ - Component tests

- **Shared Types** (`packages/api-contracts/`):
  - `index.ts` - Auto-generated TypeScript types from OpenAPI

## Error Handling

If any Spec Kit command fails:

1. Display error message to user
2. Show command that failed
3. Ask user: "Retry, Skip phase, or Abort workflow?"
4. Take appropriate action based on user choice

If codegen fails:
1. Check if backend is running on https://localhost:7001
2. Display codegen error details
3. Ask user: "Start backend and retry, or Abort workflow?"

If tests fail:
1. Display test failure details
2. Show which tests failed
3. Ask user: "Fix implementation and retry, or Abort workflow?"

## Notes

- **Backend implement requires preset** (`--preset dotnet-fastendpoints`) for consistent code generation
- **Codegen runs AFTER backend implementation** to ensure frontend has updated types
- **Analysis runs AFTER all planning artifacts** - This ensures cross-artifact consistency checks can validate the entire feature specification
- **Interactive editing**: When user selects "Make Changes", the skill can make edits on their behalf and re-checkpoint
- **Iterative refinement**: User can request multiple rounds of changes at any checkpoint
- Always use the appropriate preset for each phase
- Respect user decisions at checkpoints
- Never proceed to next phase without explicit approval
- Maintain context of feature description throughout workflow
- Backend must be running before codegen phase
