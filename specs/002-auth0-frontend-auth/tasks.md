# Tasks: Auth0 Login & Signup

**Input**: Design documents from `specs/002-auth0-frontend-auth/`

**Prerequisites**: plan.md (required), spec.md (required)

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install dependencies and create configuration

- [ ] T001 Install dependencies: `@auth0/auth0-react`, `react-router-dom`, `@tanstack/react-query`, `@mui/material`, `@mui/icons-material`, `@emotion/react`, `@emotion/styled`, `zod` in `apps/frontend/`
- [ ] T002 [P] Create `apps/frontend/.env` with Auth0 configuration variables (VITE_AUTH0_DOMAIN, VITE_AUTH0_CLIENT_ID, VITE_AUTH0_AUDIENCE, VITE_API_URL)
- [ ] T003 [P] Create `apps/frontend/src/features/auth/types/auth.ts` with AuthUser Zod schema and inferred type
- [ ] T004 [P] Create `apps/frontend/src/features/auth/services/apiClient.ts` — authenticated fetch wrapper using `getAccessTokenSilently()` with Bearer token injection and 401 error handling

---

## Phase 2: Foundational (Core Auth Infrastructure)

**Purpose**: Auth provider, hook, and route guard that all user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T005 Update `apps/frontend/src/main.tsx` — wrap `<App />` with `Auth0Provider` (domain, clientId, authorizationParams with redirectUri and audience) and `QueryClientProvider`
- [ ] T006 Create `apps/frontend/src/features/auth/hooks/useAuth.ts` — wrap `useAuth0()` with app-specific helpers (login, signup with screen_hint, logout, getAccessToken)
- [ ] T007 Create `apps/frontend/src/features/auth/components/ProtectedRoute.tsx` — auth guard using MUI `CircularProgress` for loading, redirect to `/login` when not authenticated, render children when authenticated

**Checkpoint**: Foundation ready — auth provider, hook, and route guard functional

---

## Phase 3: User Story 1 & 2 — Login & Signup (Priority: P1) 🎯 MVP

**Goal**: Users can log in and sign up via Auth0 Universal Login

**Independent Test**: Click login/signup → redirect to Auth0 → return as authenticated user

### Tests for User Story 1 & 2

- [ ] T008 [P] [US1] Create `apps/frontend/src/features/auth/tests/LoginPage.test.tsx` — test renders login/signup buttons when unauthenticated, redirects when authenticated, calls loginWithRedirect on button clicks

### Implementation for User Story 1 & 2

- [ ] T009 [US1] Create `apps/frontend/src/features/auth/components/LoginPage.tsx` — MUI Container, two Button variants for "Log In" and "Sign Up", redirect to home if already authenticated, CircularProgress loading state

**Checkpoint**: Login and signup flows work via Auth0 Universal Login

---

## Phase 4: User Story 3 — Logout (Priority: P2)

**Goal**: Authenticated users can log out, terminating both local and Auth0 sessions

**Independent Test**: Click logout → session cleared → redirected to login page

### Implementation for User Story 3

- [ ] T010 [US3] Create `apps/frontend/src/features/auth/components/LogoutButton.tsx` — MUI Button that calls logout with returnTo URL, terminates Auth0 session

**Checkpoint**: Logout terminates session and redirects to login page

---

## Phase 5: User Story 4 — Protected Route Access (Priority: P2)

**Goal**: Protected pages require authentication, with redirect-back-after-login support

**Independent Test**: Navigate to protected route unauthenticated → redirected to login → after login, returned to original route

### Tests for User Story 4

- [ ] T011 [P] [US4] Create `apps/frontend/src/features/auth/tests/ProtectedRoute.test.tsx` — test renders children when authenticated, redirects to /login when not authenticated, shows spinner while loading

**Checkpoint**: Protected routes enforce authentication with seamless redirect flow

---

## Phase 6: User Story 5 — Navigation & API Integration (Priority: P2)

**Goal**: Navigation bar shows auth state, API requests include Bearer tokens

**Independent Test**: Nav shows user info when logged in, API calls include Authorization header

### Tests for User Story 5

- [ ] T012 [P] [US5] Create `apps/frontend/src/features/auth/tests/AuthNav.test.tsx` — test shows login/signup buttons when unauthenticated, shows user info and logout when authenticated

### Implementation for User Story 5

- [ ] T013 [US5] Create `apps/frontend/src/features/auth/components/AuthNav.tsx` — MUI AppBar with Toolbar, conditional rendering based on auth state, Avatar with user picture, Typography for name, Login/Signup/Logout MUI Buttons

**Checkpoint**: Navigation adapts to auth state, API client attaches tokens

---

## Phase 7: App Integration

**Purpose**: Wire everything together with React Router

- [ ] T014 Replace `apps/frontend/src/App.tsx` scaffold with React Router — BrowserRouter, Routes for `/login` (public) and `/` (protected via ProtectedRoute), AuthNav at top level
- [ ] T015 [P] Clean up `apps/frontend/src/App.css` — remove Vite scaffold styles, add minimal app layout styles

**Checkpoint**: Full app shell with auth-aware routing and navigation

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T016 [P] Create `apps/frontend/src/features/auth/index.ts` — barrel export for all auth components, hooks, and services
- [ ] T017 Run `npm run build` in `apps/frontend/` to verify no TypeScript or build errors
- [ ] T018 Run `npm run test` in `apps/frontend/` to verify all tests pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Phase 2 completion
  - Phase 3 (Login/Signup) and Phase 4 (Logout) can proceed in parallel
  - Phase 5 (Protected Route tests) can proceed in parallel
  - Phase 6 (Nav) depends on LogoutButton from Phase 4
- **App Integration (Phase 7)**: Depends on Phase 3 and Phase 6
- **Polish (Phase 8)**: Depends on all prior phases

### Parallel Opportunities

- T002, T003, T004 can run in parallel (different files)
- T008 and T011 can run in parallel (different test files)
- T009 and T010 can run in parallel (different components)
- T012, T013 can run after T010
- T015, T016 can run in parallel
