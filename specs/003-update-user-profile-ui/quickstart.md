# Quickstart Validation Guide: Update User Profile UI

## Prerequisites

- .NET 10 SDK installed
- Node.js 20+ installed
- MinIO running locally on `localhost:9000` (access: `minio` / `minio`)
- SQL Server running with connection string from `appsettings.Development.json`
- Database migrated: `dotnet ef database update` (from `apps/backend/src/`)

## 1. Start the Backend

```bash
cd apps/backend/src
dotnet watch run --launch-profile https
```

Backend available at `https://localhost:7001`. Swagger UI at `https://localhost:7001/swagger`.

## 2. Generate TypeScript Types

After backend is running (required for codegen):

```bash
# From repo root
npm run codegen
```

Verify `packages/api-contracts/index.ts` contains `GetAvatarUrl` response type.

## 3. Start the Frontend

```bash
# From repo root
npm run dev
# or specifically
cd apps/frontend
npm run dev
```

Frontend at `http://localhost:5173`.

## 4. End-to-End Validation Scenarios

### Scenario 1: Navigate to profile page via nav bar

1. Log in via Auth0
2. Click the user avatar or name in the top navigation bar
3. **Expected**: Browser navigates to `/profile`
4. **Expected**: Profile form loads with existing data pre-filled (or empty if first visit)

### Scenario 2: View existing profile data

1. Navigate to `/profile`
2. **Expected**: Email field is visible but disabled (read-only)
3. **Expected**: Nickname, phone, address fields show current values
4. **Expected**: Avatar image displays (if previously set)

### Scenario 3: Update profile fields

1. Navigate to `/profile`
2. Change nickname to `test_user`
3. Change phone to `+1 555 000 1234`
4. Click Save
5. **Expected**: Success notification shown
6. **Expected**: Reload page → values persist

### Scenario 4: Upload avatar

1. Navigate to `/profile`
2. Click the avatar area
3. Select a valid `.jpg` or `.svg` file under 5 MB
4. **Expected**: Image preview appears immediately (before saving)
5. Click Save
6. **Expected**: Profile saved; avatar displays on reload and in nav bar

### Scenario 5: Validation errors

1. Navigate to `/profile`
2. Enter `invalid nickname!!!` in nickname field (special characters)
3. Click Save
4. **Expected**: Inline error shown on the nickname field; no navigation away

### Scenario 6: Invalid avatar file

1. Navigate to `/profile`
2. Attempt to select a `.pdf` or oversized file (>5 MB)
3. **Expected**: Error message shown; avatar preview unchanged

### Scenario 7: Direct URL access when unauthenticated

1. Log out
2. Navigate directly to `http://localhost:5173/profile`
3. **Expected**: Redirected to login page

## 5. Backend Swagger Validation

1. Open `https://localhost:7001/swagger`
2. Verify `GET /api/profile/avatar-url` endpoint is listed under UserProfile tag
3. Authenticate via the Authorize button (use a valid Auth0 token)
4. Execute `GET /api/profile/avatar-url`
5. **Expected**: `{ "avatarUrl": "http://localhost:9000/stocker-public/avatars/..." }` or `{ "avatarUrl": "" }`

## 6. Run Tests

```bash
# Backend tests
cd apps/backend
dotnet test

# Frontend tests
cd apps/frontend
npm run test
```

**Expected**: All tests pass with no failures.
