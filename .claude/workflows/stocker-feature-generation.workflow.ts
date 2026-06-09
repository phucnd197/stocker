export const meta = {
  name: 'stocker-feature-generation',
  description: 'Generate complete feature specifications with backend and frontend plans for Stocker project',
  phases: [
    { title: 'Specification', detail: 'Generate feature specification from description' },
    { title: 'Backend Planning', detail: 'Create backend technical plan with dotnet-fastendpoints preset' },
    { title: 'Backend Tasks', detail: 'Generate backend implementation tasks' },
    { title: 'Frontend Planning', detail: 'Create frontend technical plan with react-typescript preset' },
    { title: 'Frontend Tasks', detail: 'Generate frontend implementation tasks' },
  ],
};

/**
 * Main workflow function - orchestrates Spec Kit process with human checkpoints
 * @param {string} args - Feature description
 */
export async function stockerFeatureGeneration(args: string) {
  const featureDescription = args || '';

  if (!featureDescription) {
    throw new Error('Feature description is required. Usage: Workflow "Stocker Feature Generation" "Your feature description"');
  }

  log('🚀 Stocker Feature Generation Workflow');
  log('='.repeat(60));
  log(`📝 Feature: ${featureDescription}`);
  log('');

  // Phase 1: Generate Specification
  phase('Specification');
  log('Creating feature specification...');

  const specAgent = await agent('Generate a comprehensive feature specification for: ' + featureDescription +
    '\n\nInclude:\n' +
    '- User stories\n' +
    '- Functional requirements\n' +
    '- API endpoints (for backend)\n' +
    '- UI components (for frontend)\n' +
    '- Acceptance criteria\n' +
    '- Success criteria');

  log(`✅ Specification created`);
  log('');

  // Checkpoint 1
  log('🔍 Checkpoint #1: Review Specification');
  log('Please review the generated specification and ensure:');
  log('  ✓ Requirements are complete and clear');
  log('  ✓ User stories capture real needs');
  log('  ✓ Acceptance criteria are testable');
  log('');

  // Phase 2: Backend Planning
  phase('Backend Planning');
  log('Creating backend technical plan with dotnet-fastendpoints preset...');

  const backendPlanAgent = await agent(
    'Generate a backend technical plan for: ' + featureDescription + '\n\n' +
    'Use the dotnet-fastendpoints preset (already configured in this project).\n\n' +
    'Include:\n' +
    '- .NET 10 architecture\n' +
    '- FastEndpoints with REPR pattern\n' +
    '- Vertical Slice Architecture structure\n' +
    '- EF Core database design (if needed)\n' +
    '- Service layer design\n' +
    '- Testing strategy (TDD with xUnit)\n' +
    '- OpenAPI documentation requirements',
    {
      label: 'Backend Plan',
      phase: 'Backend Planning'
    }
  );

  log(`✅ Backend plan created`);
  log('');

  // Checkpoint 2
  log('🔍 Checkpoint #2: Review Backend Plan');
  log('Please review the backend plan and ensure:');
  log('  ✓ Tech stack is appropriate');
  log('  ✓ Database schema is correct');
  log('  ✓ API design is RESTful');
  log('  ✓ Security concerns are addressed');
  log('');

  // Phase 3: Backend Tasks
  phase('Backend Tasks');
  log('Generating backend implementation tasks...');

  const backendTasksAgent = await agent(
    'Generate implementation tasks for: ' + featureDescription + '\n\n' +
    'Use the dotnet-fastendpoints preset (already configured in this project).\n\n' +
    'Tasks must be:\n' +
    '- TDD-ordered (tests before implementation)\n' +
    '- Properly sequenced (entities → services → DTOs → endpoints)\n' +
    '- Include EF Core migration steps if database changes needed\n' +
    '- Include checkpoint validations\n' +
    '- Mark parallel tasks with [P]',
    {
      label: 'Backend Tasks',
      phase: 'Backend Tasks'
    }
  );

  log(`✅ Backend tasks created`);
  log('');

  // Checkpoint 3
  log('🔍 Checkpoint #3: Review Backend Tasks');
  log('Please review the backend tasks and ensure:');
  log('  ✓ Tasks are in logical order');
  log('  ✓ TDD is followed (tests first)');
  log('  ✓ Dependencies are correct');
  log('  ✓ No critical tasks are missing');
  log('');

  // Phase 4: Frontend Planning
  phase('Frontend Planning');
  log('Creating frontend technical plan with react-typescript preset...');

  const frontendPlanAgent = await agent(
    'Generate a frontend technical plan for: ' + featureDescription + '\n\n' +
    'Use the react-typescript preset (already configured in this project).\n\n' +
    'Include:\n' +
    '- React 19 component architecture\n' +
    '- TypeScript type safety with api-contracts\n' +
    '- Custom hooks for state and API calls\n' +
    '- React Query for server state\n' +
    '- Component-based structure\n' +
    '- Testing strategy (Vitest + React Testing Library)\n' +
    '- Accessibility considerations (WCAG 2.1)',
    {
      label: 'Frontend Plan',
      phase: 'Frontend Planning'
    }
  );

  log(`✅ Frontend plan created`);
  log('');

  // Checkpoint 4
  log('🔍 Checkpoint #4: Review Frontend Plan');
  log('Please review the frontend plan and ensure:');
  log('  ✓ Component architecture is sound');
  log('  ✓ Type safety is maintained');
  log('  ✓ State management strategy is clear');
  log('  ✓ Testing strategy is adequate');
  log('');

  // Phase 5: Frontend Tasks
  phase('Frontend Tasks');
  log('Generating frontend implementation tasks...');

  const frontendTasksAgent = await agent(
    'Generate implementation tasks for: ' + featureDescription + '\n\n' +
    'Use the react-typescript preset (already configured in this project).\n\n' +
    'Tasks must be:\n' +
    '- Component-driven development (tests before implementation)\n' +
    '- Properly sequenced (types → hooks → components → integration)\n' +
    '- Include testing for components and hooks\n' +
    '- Include accessibility checkpoints\n' +
    '- Mark parallel tasks with [P]',
    {
      label: 'Frontend Tasks',
      phase: 'Frontend Tasks'
    }
  );

  log(`✅ Frontend tasks created`);
  log('');

  // Checkpoint 5
  log('🔍 Checkpoint #5: Review Frontend Tasks');
  log('Please review the frontend tasks and ensure:');
  log('  ✓ Tasks are in logical order');
  log('  ✓ Component testing is included');
  log('  ✓ Parallel tasks are marked correctly');
  log('  ✓ Accessibility requirements are included');
  log('');

  // Complete
  log('='.repeat(60));
  log('✅ Feature Generation Workflow Complete!');
  log('='.repeat(60));
  log('');
  log('📋 All artifacts have been generated:');
  log('  1. ✓ Specification (spec.md)');
  log('  2. ✓ Backend Plan (plan.md)');
  log('  3. ✓ Backend Tasks (tasks.md)');
  log('  4. ✓ Frontend Plan (plan.md)');
  log('  5. ✓ Frontend Tasks (tasks.md)');
  log('');
  log('🎯 Ready for Implementation:');
  log('  When ready, implement the features:');
  log('  - Backend: Run /speckit.implement');
  log('  - Frontend: Run /speckit.implement --preset react-typescript');
  log('  - Sync types: npm run codegen');

  return {
    success: true,
    message: 'Feature generation complete. All artifacts ready for implementation.'
  };
}
