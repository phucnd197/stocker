# Auth0 Login & Signup UI/UX Feature Specification

## Overview

Add authentication to the Stocker frontend using Auth0 Universal Login. Users can sign up for new accounts, log in, and log out. Authenticated users see a navigation bar with their profile info. Protected pages require login and redirect unauthenticated visitors to Auth0's hosted login page. An API client automatically attaches Bearer tokens to backend requests.

## User Stories

### Story 1: Visitor Signs Up
**As a** new visitor,
**I want** to create an account using Auth0,
**So that** I can access protected features of the Stocker app.

**Acceptance Criteria**:
- [ ] Clicking "Sign Up" redirects to Auth0 Universal Login with the signup tab active
- [ ] After successful signup, the user is redirected back to the app as an authenticated user
- [ ] The navigation bar updates to show the user's name and profile picture

**UI/UX Requirements**:
- A clearly visible "Sign Up" button in the navigation bar for unauthenticated visitors
- The signup flow is handled entirely by Auth0's hosted page (no custom form needed)

### Story 2: User Logs In
**As a** returning user,
**I want** to log in with my existing credentials,
**So that** I can access my authenticated session.

**Acceptance Criteria**:
- [ ] Clicking "Log In" redirects to Auth0 Universal Login
- [ ] After successful login, the user is redirected back to the app
- [ ] The navigation bar shows the user's name, email, and profile picture
- [ ] Protected pages are now accessible

**UI/UX Requirements**:
- A clearly visible "Log In" button in the navigation bar for unauthenticated visitors
- Seamless redirect flow — the user returns to the page they were trying to access

### Story 3: User Logs Out
**As a** an authenticated user,
**I want** to log out of the application,
**So that** my session is securely terminated.

**Acceptance Criteria**:
- [ ] Clicking "Log Out" terminates both the local session and the Auth0 session
- [ ] The user is redirected to the login page or home page after logout
- [ ] Protected pages are no longer accessible after logout

**UI/UX Requirements**:
- A "Log Out" button visible in the navigation bar for authenticated users
- Logout clears all local tokens and redirects to Auth0's logout endpoint

### Story 4: Protected Route Access
**As a** an authenticated user,
**I want** protected pages to be accessible only after logging in,
**So that** my data and features are secured.

**Acceptance Criteria**:
- [ ] Unauthenticated visitors are redirected to the login page when navigating to a protected route
- [ ] After login, the user is redirected back to the originally requested protected route
- [ ] Public pages (login, home) remain accessible without authentication

**UI/UX Requirements**:
- Protected routes display a loading state while authentication status is being determined
- Clear visual feedback when redirecting to login

### Story 5: API Requests Include Authentication
**As a** an authenticated user,
**I want** my API requests to automatically include my authentication credentials,
**So that** I can access protected backend resources seamlessly.

**Acceptance Criteria**:
- [ ] All API requests from authenticated users include a valid Bearer token in the Authorization header
- [ ] Tokens are refreshed silently when expired without user intervention
- [ ] Failed authentication shows an appropriate error and prompts re-login

## Component Requirements

### Page Components

#### LoginPage
**Purpose**: Landing page for unauthenticated users with login/signup actions

**Route**: `/login`

**Sub-components**:
- Login/Signup call-to-action buttons
- Application branding/logo

**Behavior**:
- Displays "Log In" and "Sign Up" buttons that trigger Auth0 redirect
- If user is already authenticated, redirects to the home page
- Shows a loading spinner while Auth0 processes the authentication callback

### Feature Components

#### AuthNav
**Purpose**: Navigation bar that adapts based on authentication state

**Props Interface**:
```typescript
// No props — reads from Auth0 context via useAuth0()
```

**Behavior**:
- **Unauthenticated**: Shows "Log In" and "Sign Up" buttons
- **Authenticated**: Shows user's name, profile picture, and "Log Out" button
- Displays a loading skeleton while authentication state is being determined

**Accessibility**:
- All buttons have descriptive ARIA labels
- Profile picture has alt text with user's name
- Keyboard navigable in logical tab order

#### ProtectedRoute
**Purpose**: Route guard that restricts access to authenticated users only

**Props Interface**:
```typescript
interface ProtectedRouteProps {
  children: React.ReactNode
}
```

**Behavior**:
- Checks authentication status via `useAuth0().isAuthenticated`
- If not authenticated: redirects to `/login` with the current location saved for return
- If authenticated: renders the child components
- Shows a loading indicator while auth state is resolving

**Accessibility**:
- Loading state is announced to screen readers
- Redirect is seamless without flashing protected content

#### LogoutButton
**Purpose**: Button component that triggers Auth0 logout

**Props Interface**:
```typescript
// No props — reads from Auth0 context
```

**Behavior**:
- Calls `logout()` with `logoutParams.returnTo` set to the app's origin
- Terminates both the local session and Auth0 federated session

**Accessibility**:
- Button has clear "Log Out" label
- Focus management after logout redirect

## Data Flow

### API Integration

#### Authenticated API Calls
- **Client**: Centralized fetch wrapper (`apiClient`)
- **Authentication**: Bearer token attached via `getAccessTokenSilently()`
- **Error Handling**: 401 responses trigger re-authentication flow

### State Management

#### Auth State (Auth0 SDK)
```typescript
const { isAuthenticated, user, loginWithRedirect, logout, getAccessTokenSilently, isLoading } = useAuth0();
```

#### Server State (React Query)
```typescript
// Future: API calls will use React Query with the authenticated apiClient
const queryClient = new QueryClient();
```

## User Interactions

### Action 1: Login
**Trigger**: User clicks "Log In" button in navigation

**Expected Response**:
- Browser redirects to Auth0 Universal Login page
- After successful authentication, browser returns to the app

**Loading State**: Full-page loading spinner during Auth0 redirect and callback processing

**Error State**: Error message displayed if Auth0 authentication fails

**Success State**: Navigation bar updates with user info, protected content becomes accessible

### Action 2: Sign Up
**Trigger**: User clicks "Sign Up" button in navigation

**Expected Response**:
- Browser redirects to Auth0 Universal Login page with signup tab active
- After successful registration, browser returns to the app

**Loading State**: Full-page loading spinner during Auth0 redirect and callback processing

**Error State**: Error message displayed if Auth0 registration fails

**Success State**: Navigation bar updates with user info, protected content becomes accessible

### Action 3: Logout
**Trigger**: User clicks "Log Out" button in navigation

**Expected Response**:
- Local tokens are cleared
- Browser redirects to Auth0 logout endpoint
- Browser returns to the app's login page

**Loading State**: Brief redirect, no loading state needed

**Error State**: If logout fails, display error and offer retry

**Success State**: User sees the unauthenticated navigation bar and login page

### Action 4: Access Protected Route
**Trigger**: User navigates to a protected page

**Expected Response**:
- If authenticated: page renders normally
- If not authenticated: redirect to `/login` with return URL saved

**Loading State**: Loading indicator while auth state resolves

**Error State**: If auth check fails, redirect to login with error message

**Success State**: Protected content is displayed

## Error Handling

### User-Facing Errors
- **Authentication Failed**: "Unable to log in. Please try again."
- **Session Expired**: "Your session has expired. Please log in again."
- **Network Error**: "Unable to connect. Please check your connection."
- **Token Refresh Failed**: Automatically prompts re-login

### Error Boundaries
- Auth callback errors handled at the `Auth0Provider` level
- Component-level error handling for login/logout failures

## Testing Requirements

### Component Tests
- `LoginPage` renders login and signup buttons for unauthenticated users
- `LoginPage` redirects authenticated users away
- `ProtectedRoute` renders children when authenticated
- `ProtectedRoute` redirects to `/login` when not authenticated
- `AuthNav` shows login/signup buttons when unauthenticated
- `AuthNav` shows user info and logout when authenticated
- `LogoutButton` calls logout on click

### Hook Tests
- `useAuth` returns expected auth state and methods

### Integration Tests
- Full login redirect flow simulation
- Protected route access after authentication
- Logout clears session and redirects

## Out of Scope

- **User profile editing**: Will be implemented in a future phase
- **Role-based authorization**: Not needed for this phase
- **Database user storage**: Will be planned in a separate task
- **Custom login forms**: Auth0 Universal Login is sufficient
- **Password reset flow**: Handled by Auth0 Universal Login natively
- **Multi-factor authentication**: Can be enabled in Auth0 dashboard without code changes
- **Social login configuration**: Can be enabled in Auth0 dashboard without code changes

## Assumptions

- The user has an Auth0 SPA application configured with the correct Client ID, domain, and callback URLs
- The backend Auth0 JWT validation is already configured and working
- Auth0 Universal Login (hosted page) is the desired login experience
- Single Sign-On across subdomains is not required at this stage
- The app runs on `http://localhost:5173` during development

## References

- [Auth0 React SDK Documentation](https://auth0.com/docs/libraries/auth0-react)
- [Auth0 Universal Login](https://auth0.com/docs/authenticate/login/auth0-universal-login)
- [React Router Documentation](https://reactrouter.com/)
- [React Query Documentation](https://tanstack.com/query/latest)

---

**Checkpoint #1: Human Review Required**

Before proceeding to `/speckit.plan`, verify:
- [x] Are UI/UX requirements clear?
- [x] Are user stories capturing real user needs?
- [x] Are acceptance criteria testable from UI perspective?
- [x] Is the component structure logical?
- [x] Are accessibility requirements identified?
- [x] Is the responsive design strategy sound?
