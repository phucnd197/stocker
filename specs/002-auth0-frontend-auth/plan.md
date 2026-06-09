# Auth0 Login & Signup Implementation Plan

**Branch**: `001-auth0-frontend-auth` | **Date**: 2026-06-10 | **Spec**: [spec.md](spec.md)

## Summary

Add Auth0 authentication to the React frontend using Universal Login (hosted redirect flow). The feature includes: Auth0Provider setup, login/signup/logout via redirect, a ProtectedRoute guard component, an authenticated API client with Bearer token injection, and a Material Design navigation bar with auth state display.

## Technical Stack

### Frontend
- **Framework**: React 19.2.x
- **Language**: TypeScript 6.0.x
- **Build**: Vite 8.x
- **Auth SDK**: @auth0/auth0-react
- **Routing**: react-router-dom v7
- **Server State**: @tanstack/react-query v5
- **UI Library**: Material UI (MUI) 7.x
- **Validation**: Zod 3.x (for API response schemas)
- **Testing**: Vitest / React Testing Library

### Backend Integration
- **API Contract**: Generated types from `@stocker/api-contracts`
- **Auth**: Bearer token via Auth0 `getAccessTokenSilently()`
- **Base URL**: Configured in environment variables

## Architecture Overview

### Component Architecture

```
apps/frontend/src/features/auth/
├── components/
│   ├── LoginPage.tsx              # Login/signup landing page
│   ├── LogoutButton.tsx           # Logout trigger button
│   ├── AuthNav.tsx                # Navigation bar with auth state
│   └── ProtectedRoute.tsx         # Route guard for authenticated pages
├── hooks/
│   └── useAuth.ts                 # Auth hook wrapping useAuth0
├── services/
│   └── apiClient.ts               # Authenticated fetch wrapper
├── types/
│   └── auth.ts                    # Auth-related types
└── tests/
    ├── LoginPage.test.tsx
    ├── ProtectedRoute.test.tsx
    └── AuthNav.test.tsx
```

### App-Level Changes

```
apps/frontend/
├── src/
│   ├── main.tsx                   # Add Auth0Provider + QueryClientProvider
│   ├── App.tsx                    # Replace scaffold with React Router
│   └── App.css                    # Remove Vite scaffold styles
├── .env                           # Auth0 + API configuration
└── package.json                   # New dependencies
```

## Implementation Details

### Phase 1: Setup & Configuration

#### Dependencies

```bash
cd apps/frontend
npm install @auth0/auth0-react react-router-dom @tanstack/react-query @mui/material @mui/icons-material @emotion/react @emotion/styled zod
```

#### Environment Variables

Create `apps/frontend/.env`:

```
VITE_AUTH0_DOMAIN=dev-lm7amq8xe3q5638v.auth0.com
VITE_AUTH0_CLIENT_ID=<user-provided>
VITE_AUTH0_AUDIENCE=https://dev-lm7amq8xe3q5638v.us.auth0.com/api/v2/
VITE_API_URL=https://localhost:7001
```

### Phase 2: Auth Types

Create `src/features/auth/types/auth.ts`:

```typescript
import { z } from 'zod';

export const authUserSchema = z.object({
  sub: z.string(),
  email: z.string().email().optional(),
  name: z.string().optional(),
  nickname: z.string().optional(),
  picture: z.string().url().optional(),
});

export type AuthUser = z.infer<typeof authUserSchema>;
```

### Phase 3: API Client

Create `src/features/auth/services/apiClient.ts`:

```typescript
import { getAccessTokenSilently } from '@auth0/auth0-react';

// This will be a utility that wraps fetch with Bearer token injection.
// Uses getAccessTokenSilently() to obtain tokens.
// Handles 401 responses by triggering re-authentication.
```

### Phase 4: Auth Hook

Create `src/features/auth/hooks/useAuth.ts`:

```typescript
// Wraps useAuth0() with app-specific helpers.
// Exposes: isAuthenticated, user, login, signup, logout, getAccessToken, isLoading.
// login() calls loginWithRedirect() with authorizationParams.
// signup() calls loginWithRedirect() with authorizationParams.screen_hint = 'signup'.
```

### Phase 5: Protected Route

Create `src/features/auth/components/ProtectedRoute.tsx`:

```typescript
// Uses MUI CircularProgress for loading state.
// Checks isAuthenticated from useAuth hook.
// Redirects to /login with current location saved in state if not authenticated.
// Renders children if authenticated.
```

### Phase 6: Login Page

Create `src/features/auth/components/LoginPage.tsx`:

```typescript
// Uses MUI Container, Button, Typography.
// Two CTAs: "Log In" and "Sign Up" using MUI Button variants.
// If already authenticated, redirects to home page via Navigate.
// Loading state with MUI CircularProgress while Auth0 processes callback.
```

### Phase 7: Auth Navigation

Create `src/features/auth/components/AuthNav.tsx`:

```typescript
// Uses MUI AppBar, Toolbar, Button, Avatar, Typography.
// Unauthenticated: shows "Log In" and "Sign Up" MUI Buttons.
// Authenticated: shows user Avatar (from picture), name Typography, "Log Out" Button.
// Responsive: uses MUI responsive breakpoints.
```

### Phase 8: App Integration

Update `src/main.tsx`:

```typescript
// Wrap <App /> with:
// 1. Auth0Provider (domain, clientId, authorizationParams with redirectUri and audience)
// 2. QueryClientProvider (for React Query)
```

Update `src/App.tsx`:

```typescript
// Replace Vite scaffold with:
// 1. BrowserRouter
// 2. Routes: /login (public), / (protected via ProtectedRoute)
// 3. AuthNav rendered at the top level
```

### Phase 9: Testing

#### LoginPage Tests

```typescript
// Renders login and signup buttons when unauthenticated.
// Redirects to home when already authenticated.
// Calls loginWithRedirect when buttons are clicked.
```

#### ProtectedRoute Tests

```typescript
// Renders children when authenticated.
// Redirects to /login when not authenticated.
// Shows loading spinner while auth state is resolving.
```

#### AuthNav Tests

```typescript
// Shows login/signup buttons when unauthenticated.
// Shows user info and logout button when authenticated.
// Calls logout when logout button is clicked.
```

## Configuration

### Environment Variables

```bash
VITE_AUTH0_DOMAIN=dev-lm7amq8xe3q5638v.auth0.com
VITE_AUTH0_CLIENT_ID=<user-provided>
VITE_AUTH0_AUDIENCE=https://dev-lm7amq8xe3q5638v.us.auth0.com/api/v2/
VITE_API_URL=https://localhost:7001
```

### Auth0 Dashboard Requirements

- **Allowed Callback URLs**: `http://localhost:5173`
- **Allowed Logout URLs**: `http://localhost:5173`
- **Allowed Web Origins**: `http://localhost:5173`

---

**Checkpoint #2: Human Review Required**

Before proceeding to `/speckit.tasks`, verify:
- [x] Is the component architecture sound?
- [x] Are hooks appropriately structured?
- [x] Is state management strategy clear?
- [x] Are API integration patterns correct?
- [x] Is the testing strategy adequate?
- [x] Are performance considerations addressed?
