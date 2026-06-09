# {FEATURE_NAME} Implementation Plan

## Technical Stack

### Frontend
- **Framework**: React 19.2.x
- **Language**: TypeScript 6.0.x
- **Build**: Vite 8.x
- **State Management**: React hooks (useState, useReducer, useContext)
- **Server State**: TanStack Query (React Query) / SWR
- **Routing**: React Router v6+
- **Forms**: React Hook Form
- **Validation**: Zod / Yup
- **Testing**: Vitest / React Testing Library
- **Type Safety**: TypeScript with api-contracts package

### Backend Integration
- **API Contract**: Generated types from `@stocker/api-contracts`
- **Fetch**: Native fetch / Axios
- **Base URL**: Configured in environment variables

## Architecture Overview

### Component Architecture

```
apps/frontend/src/features/{FEATURE_NAME}/
├── components/
│   ├── {FEATURE}Page.tsx           # Main page component
│   ├── {FEATURE}List.tsx           # List/table component
│   ├── {FEATURE}Item.tsx           # Item component
│   ├── {FEATURE}Form.tsx           # Form component (if CRUD)
│   └── {FEATURE}Card.tsx           # Card component
├── hooks/
│   ├── use{FEATURE}.ts             # Custom feature hook
│   └── use{FEATURE}Api.ts          # API hook (React Query)
├── services/
│   └── {FEATURE}Api.ts             # API client (typed)
├── types/
│   └── {FEATURE}.ts                # Local types (extends api-contracts)
├── tests/
│   ├── {FEATURE}Page.test.tsx
│   ├── {FEATURE}List.test.tsx
│   └── {FEATURE}Form.test.tsx
└── index.ts                         # Feature exports
```

## Implementation Details

### Phase 1: Type Definitions

### Extended Types

Create local types in `types/{FEATURE}.ts`:

```typescript
import type { components } from '@stocker/api-contracts';

// Extend API types with UI-specific types
export type {ENTITY}FromApi = components['schemas']['{ENTITY}'];
export type {REQUEST}FromApi = components['schemas']['{REQUEST}'];
export type {RESPONSE}FromApi = components['schemas']['{RESPONSE}'];

// UI-specific types
export interface {ENTITY}Ui extends {ENTITY}FromApi {
  isSelected: boolean;
  isExpanded: boolean;
}

export interface {FEATURE}State {
  items: {ENTITY}Ui[];
  filter: string;
  sortBy: '{FIELD}';
  isLoading: boolean;
  error: string | null;
}
```

### Phase 2: API Service Layer

### API Client

Create service in `services/{FEATURE}Api.ts`:

```typescript
import type { components } from '@stocker/api-contracts';

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001';

export class {FEATURE}Api {
  private baseUrl: string;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || API_BASE;
  }

  async getAll{RESOURCE}(): Promise<{RESPONSE}FromApi[]> {
    const response = await fetch(`${this.baseUrl}/api/{route}`);
    if (!response.ok) throw new Error('Failed to fetch {RESOURCE}');
    return response.json();
  }

  async get{RESOURCE}ById(id: number): Promise<{RESPONSE}FromApi> {
    const response = await fetch(`${this.baseUrl}/api/{route}/${id}`);
    if (!response.ok) throw new Error('Failed to fetch {RESOURCE}');
    return response.json();
  }

  async create{RESOURCE}(data: {REQUEST}FromApi): Promise<{RESPONSE}FromApi> {
    const response = await fetch(`${this.baseUrl}/api/{route}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to create {RESOURCE}');
    return response.json();
  }

  async update{RESOURCE}(id: number, data: {REQUEST}FromApi): Promise<{RESPONSE}FromApi> {
    const response = await fetch(`${this.baseUrl}/api/{route}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    if (!response.ok) throw new Error('Failed to update {RESOURCE}');
    return response.json();
  }

  async delete{RESOURCE}(id: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}/api/{route}/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete {RESOURCE}');
  }
}
```

### Phase 3: Custom Hooks

### Feature Hook

Create custom hook in `hooks/use{FEATURE}.ts`:

```typescript
import { useState, useCallback } from 'react';
import { use{FEATURE}Api } from './use{FEATURE}Api';
import type { {ENTITY}Ui, {FEATURE}State } from '../types/{FEATURE}';

export function use{FEATURE}() {
  const { data, isLoading, error, refetch } = use{FEATURE}Api();
  
  const [state, setState] = useState<{FEATURE}State>({
    items: [],
    filter: '',
    sortBy: 'name',
    isLoading: false,
    error: null,
  });

  const filterItems = useCallback((filter: string) => {
    setState(prev => ({ ...prev, filter }));
  }, []);

  const sortItems = useCallback((sortBy: string) => {
    setState(prev => ({ ...prev, sortBy }));
  }, []);

  const toggleSelection = useCallback((id: number) => {
    setState(prev => ({
      ...prev,
      items: prev.items.map(item =>
        item.id === id ? { ...item, isSelected: !item.isSelected } : item
      ),
    }));
  }, []);

  return {
    ...state,
    data,
    isLoading: isLoading || state.isLoading,
    error: error || state.error,
    refetch,
    filterItems,
    sortItems,
    toggleSelection,
  };
}
```

### API Hook (React Query)

Create API hook in `hooks/use{FEATURE}Api.ts`:

```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { {FEATURE}Api } from '../services/{FEATURE}Api';
import type { {REQUEST}FromApi } from '../types/{FEATURE}';

const api = new {FEATURE}Api();

export function use{FEATURE}Api() {
  const queryClient = useQueryClient();

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['{FEATURE}'],
    queryFn: () => api.getAll{RESOURCE}(),
  });

  const createMutation = useMutation({
    mutationFn: (data: {REQUEST}FromApi) => api.create{RESOURCE}(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{FEATURE}'] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: {REQUEST}FromApi }) =>
      api.update{RESOURCE}(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{FEATURE}'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: number) => api.delete{RESOURCE}(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{FEATURE}'] });
    },
  });

  return {
    data,
    isLoading,
    error,
    refetch,
    create: createMutation.mutateAsync,
    update: updateMutation.mutateAsync,
    delete: deleteMutation.mutateAsync,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}
```

### Phase 4: Page Components

### Page Component

Create page component in `components/{FEATURE}Page.tsx`:

```typescript
import { use{FEATURE} } from '../hooks/use{FEATURE}';
import { {FEATURE}List } from './{FEATURE}List';
import { {FEATURE}Form } from './{FEATURE}Form';

export function {FEATURE}Page() {
  const {
    items,
    isLoading,
    error,
    filter,
    filterItems,
    create,
    isCreating,
  } = use{FEATURE}();

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div className="{FEATURE}-page">
      <h1>{FEATURE_TITLE}</h1>
      <{FEATURE}Form onSubmit={create} isSubmitting={isCreating} />
      <{FEATURE}List items={items} filter={filter} onFilterChange={filterItems} />
    </div>
  );
}
```

### List Component

Create list component in `components/{FEATURE}List.tsx`:

```typescript
import type { {ENTITY}Ui } from '../types/{FEATURE}';

interface {FEATURE}ListProps {
  items: {ENTITY}Ui[];
  filter: string;
  onFilterChange: (filter: string) => void;
}

export function {FEATURE}List({ items, filter, onFilterChange }: {FEATURE}ListProps) {
  const filteredItems = items.filter(item =>
    item.name.toLowerCase().includes(filter.toLowerCase())
  );

  return (
    <div className="{FEATURE}-list">
      <input
        type="text"
        value={filter}
        onChange={(e) => onFilterChange(e.target.value)}
        placeholder="Search..."
        aria-label="Search items"
      />
      <ul>
        {filteredItems.map(item => (
          <{FEATURE}Item key={item.id} item={item} />
        ))}
      </ul>
    </div>
  );
}
```

### Item Component

Create item component in `components/{FEATURE}Item.tsx`:

```typescript
import type { {ENTITY}Ui } from '../types/{FEATURE}';

interface {FEATURE}ItemProps {
  item: {ENTITY}Ui;
}

export function {FEATURE}Item({ item }: {FEATURE}ItemProps) {
  return (
    <li className="{FEATURE}-item" data-id={item.id}>
      <h3>{item.name}</h3>
      <p>{item.description}</p>
      {item.isSelected && <span>✓ Selected</span>}
    </li>
  );
}
```

### Form Component (if CRUD)

Create form component in `components/{FEATURE}Form.tsx`:

```typescript
import { useForm } from 'react-hook-form';
import type { {REQUEST}FromApi } from '../types/{FEATURE}';

interface {FEATURE}FormProps {
  onSubmit: (data: {REQUEST}FromApi) => Promise<void>;
  isSubmitting: boolean;
}

export function {FEATURE}Form({ onSubmit, isSubmitting }: {FEATURE}FormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<{
    name: string;
    description: string;
  }>();

  return (
    <form onSubmit={handleSubmit((data) => onSubmit(data as {REQUEST}FromApi))}>
      <div className="form-group">
        <label htmlFor="name">Name</label>
        <input
          id="name"
          {...register('name', { required: 'Name is required' })}
          aria-invalid={errors.name ? 'true' : 'false'}
        />
        {errors.name && <span role="alert">{errors.name.message}</span>}
      </div>

      <div className="form-group">
        <label htmlFor="description">Description</label>
        <textarea
          id="description"
          {...register('description')}
        />
      </div>

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Saving...' : 'Save'}
      </button>
    </form>
  );
}
```

### Phase 5: Styling

### CSS Modules or Tailwind

**Using CSS Modules**:

```css
/* {FEATURE}Page.module.css */
.{FEATURE}-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.{FEATURE}-list {
  margin-top: 2rem;
}

.{FEATURE}-item {
  padding: 1rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}
```

**Using Tailwind CSS**:

```typescript
// In components
className="max-w-6xl mx-auto p-8"
```

### Phase 6: Routing

### Route Configuration

Add to `App.tsx` or routing configuration:

```typescript
import { {FEATURE}Page } from './features/{FEATURE}';

const router = createBrowserRouter([
  {
    path: '/{route}',
    element: <{FEATURE}Page />,
  },
  // ... other routes
]);
```

### Phase 7: Error Handling

### Error Boundary

Create error boundary:

```typescript
import { Component, ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class {FEATURE}ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="error-boundary">
          <h2>Something went wrong</h2>
          <p>{this.state.error?.message}</p>
          <button onClick={() => window.location.reload()}>Reload</button>
        </div>
      );
    }

    return this.props.children;
  }
}
```

### Phase 8: Testing

### Component Tests

Create tests in `tests/{FEATURE}Page.test.tsx`:

```typescript
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { {FEATURE}Page } from '../components/{FEATURE}Page';

describe('{FEATURE}Page', () => {
  it('renders page heading', () => {
    render(<{FEATURE}Page />);
    expect(screen.getByRole('heading', { name: '{FEATURE_TITLE}' })).toBeInTheDocument();
  });

  it('shows loading state while fetching data', () => {
    render(<{FEATURE}Page />);
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('displays error message on fetch failure', async () => {
    // Mock API failure
    render(<{FEATURE}Page />);
    // Assert error display
  });
});
```

### Phase 9: Performance Optimization

### Code Splitting

```typescript
import { lazy, Suspense } from 'react';

const {FEATURE}Page = lazy(() => import('./features/{FEATURE}'));

// In router
<Suspense fallback={<div>Loading...</div>}>
  <{FEATURE}Page />
</Suspense>
```

### Memoization

```typescript
import { memo } from 'react';

export const {FEATURE}Item = memo(function {FEATURE}Item({ item }: {FEATURE}ItemProps) {
  // Component implementation
});
```

## Configuration

### Environment Variables

Add to `.env`:

```bash
VITE_API_BASE_URL=https://localhost:7001
```

### Vite Configuration

Update `vite.config.ts` if needed for aliases or plugins.

## Deployment Considerations

### Build Optimization
- Code splitting enabled
- Tree shaking enabled
- Minification enabled
- Source maps for production (optional)

### Asset Optimization
- Image lazy loading
- Font optimization
- Bundle size monitoring

---

**Checkpoint #2: Human Review Required**

Before proceeding to `/speckit.tasks`, verify:
- [ ] Is the component architecture sound?
- [ ] Are hooks appropriately structured?
- [ ] Is state management strategy clear?
- [ ] Are API integration patterns correct?
- [ ] Is the testing strategy adequate?
- [ ] Are performance considerations addressed?
