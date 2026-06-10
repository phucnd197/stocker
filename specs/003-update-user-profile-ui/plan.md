# Update User Profile UI — Implementation Plan

**Branch**: `003-update-user-profile-ui` | **Date**: 2026-06-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-update-user-profile-ui/spec.md`

## Summary

Add a React profile edit page at `/profile` that lets authenticated users view and update their nickname, phone, address, and avatar. **All backend endpoints already exist.** Two small backend fixes are needed, then the work is frontend-only:

1. `GetUserProfileEndpoint` — extend `UserProfileResponse` to include a resolved `avatarUrl` field (constructed from the `image` key + MinIO config). This eliminates any need for a separate avatar-url endpoint — the frontend gets everything from one `GET /api/profile` call.
2. `UploadAvatarEndpoint` — `.svg` extension missing leading dot in `allowedExtensions` array.
3. `GetAvatarUrlEndpoint` — already exists but is broken and now **unnecessary**; can be deleted.
4. `api-contracts/index.ts` — needs `npm run codegen` after backend fixes to surface the new `avatarUrl` field and correct `upload-avatar` response (currently shows 204, should be 200 with `{ imageKey }`).

This is predominantly a **frontend UI task** with targeted backend fixes.

---

## Technical Context

**Backend Language/Version**: .NET 10 (C#)

**Backend Dependencies**: FastEndpoints 8.x, EF Core 10.x, MinIO .NET SDK, FluentValidation, xUnit 2.x

**Storage**: MinIO (public bucket: `stocker-public`). Objects accessible at `http://{MinioEndpoint}/{bucket}/{objectKey}`.

**Frontend Framework**: React 19.2 + TypeScript 6.0 + Vite 8

**Frontend Dependencies**: MUI 9.x, Zod 4.x, TanStack React Query v5, React Router DOM v7, `@stocker/api-contracts`

**Performance Goals**: Avatar preview within 3 seconds; profile save feedback immediate

**Constraints**: All profile endpoints require auth; avatar max 5 MB; accepted types jpg/jpeg/png/gif/svg

---

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| Vertical Slice Architecture | ✅ | New `GetAvatarUrl` endpoint stays inside existing `UserProfile` slice |
| REPR Pattern | ✅ | New endpoint: `EndpointWithoutRequest<GetAvatarUrlResponse>` |
| EF Core migrations | ✅ | No schema changes — entity and migration already exist |
| TDD | ✅ | Tests written before implementation in tasks |
| OpenAPI documented | ✅ | New endpoint included in Swagger |
| No `any` types (frontend) | ✅ | Zod inference + `@stocker/api-contracts` types |
| MUI for UI | ✅ | No custom UI components |
| Zod for validation | ✅ | Form schema mirrors backend rules |
| Async/await | ✅ | No `.Result`/`.Wait()` usage |

---

## Project Structure

### Documentation (this feature)

```text
specs/003-update-user-profile-ui/
├── plan.md              ← this file
├── research.md          ← research output
├── data-model.md        ← data model
├── quickstart.md        ← validation guide
├── contracts/           ← API contracts
└── tasks.md             ← /speckit-tasks output
```

### Backend Source (fixes only — no new files)

```text
apps/backend/src/Features/UserProfile/
├── GetUserProfile/
│   └── GetUserProfileEndpoint.cs        ← FIX: add avatarUrl to UserProfileResponse (MinIO URL from image key)
├── GetAvatarUrl/
│   └── GetAvatarUrlEndpoint.cs          ← DELETE: no longer needed
└── UploadAvatar/
    └── UploadAvatarEndpoint.cs          ← FIX: ".svg" missing leading dot (line 12)
```

### Frontend Source (new/changed files)

```text
apps/frontend/src/
├── features/userProfile/
│   ├── components/
│   │   ├── UserProfilePage.tsx          ← Main page at /profile
│   │   ├── AvatarUploader.tsx           ← Avatar preview + file input
│   │   └── ProfileForm.tsx             ← Nickname/phone/address fields
│   ├── hooks/
│   │   ├── useUserProfile.ts            ← React Query: GET profile + avatar URL
│   │   └── useUpdateProfile.ts         ← React Query mutation: upload + upsert
│   ├── services/
│   │   └── userProfileApi.ts           ← Typed API call functions
│   ├── types/
│   │   └── userProfile.ts              ← Zod schemas + inferred types
│   └── index.ts                        ← Barrel export
├── App.tsx                             ← CHANGE: add /profile ProtectedRoute
└── features/auth/components/
    └── AuthNav.tsx                     ← CHANGE: avatar/name → clickable link to /profile
```

---

## Backend Fixes Required

### Fix 1: `GetUserProfileEndpoint.cs` — add `avatarUrl` to response

Extend `UserProfileResponse` to include a resolved public URL alongside the raw image key. Inject `IOptions<MinioOptions>` into the endpoint and construct the URL when returning the response.

```csharp
public class UserProfileResponse
{
    public string Email { get; set; }
    public string Image { get; set; }       // raw MinIO key (keep for upsert round-trip)
    public string AvatarUrl { get; set; }   // NEW: http://{endpoint}/{bucket}/{image}
    public string Nickname { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}
```

URL construction: `http://{minioOptions.Endpoint}/{minioOptions.PublicBucket}/{entity.Image}` — empty string when `entity.Image` is null/empty.

### Fix 2: `UploadAvatarEndpoint.cs` line 12 — dot before svg

```csharp
// Before (bug)
private static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", "svg"];
// After
private static readonly string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".svg"];
```

### Fix 3: Delete `GetAvatarUrlEndpoint.cs`

The `GetAvatarUrl/` folder is now unnecessary and should be removed entirely.

### Fix 4: Codegen — re-run after backend fixes

`npm run codegen` must run after the backend fixes so `@stocker/api-contracts` reflects:
- `GET /api/profile` → response includes new `avatarUrl: string` field
- `POST /api/profile/upload-avatar` → 200 with `{ imageKey: string }` (currently shows 204 — stale)

---

## Frontend Implementation Details

### Zod Validation Schema

Mirrors backend FluentValidation rules (in `types/userProfile.ts`):
- `nickname`: max 100 chars, `/^[\p{L}0-9_-]+$/u`
- `phone`: max 20 chars, `/^\+?[0-9\s.\-()]+$/`
- `address`: max 500 chars, `/^[\p{L}0-9\s,.#\-\/]+$/u`
- `image`: optional string, max 200 chars

### React Query Strategy

**`useUserProfile`**: single `useQuery` with key `['userProfile']` on `GET /api/profile`, enabled only when the user is authenticated. Called from both `AuthNav` (on app load after login) and `UserProfilePage` — both read from the same cache entry so only one network request is made. The response includes `avatarUrl` for display and `image` key for the upsert round-trip.

**`useUpdateProfile`** mutation sequence on form submit:
1. If new file selected → `POST /api/profile/upload-avatar` (multipart) → get `imageKey`
2. `POST /api/profile` with form values + `imageKey` (or existing `image` key if no new file)
3. On success → invalidate `['userProfile']` query (updates both `AuthNav` avatar and profile form) + show success Snackbar

### Component Responsibilities

- **`UserProfilePage`**: Layout, loading state (`CircularProgress`), error state (`Alert`), success Snackbar, composes `AvatarUploader` + `ProfileForm`
- **`AvatarUploader`**: Clickable MUI `Avatar` (displays preview URL or resolved avatarUrl), hidden `<input type="file" accept=".jpg,.jpeg,.png,.gif,.svg">`, client-side file validation (type + ≤5 MB), `URL.createObjectURL` for preview
- **`ProfileForm`**: MUI `TextField` fields for nickname, phone, address; disabled `TextField` for email (read-only); Zod-based validation errors in `helperText`

### `AuthNav.tsx` change

Two changes:
1. Call `useUserProfile()` (enabled only when `isAuthenticated === true`) so profile data is fetched and cached immediately after login. Use `avatarUrl` from the response for the `Avatar` `src` — falling back to `user.picture` from Auth0 if no stored avatar exists yet.
2. Wrap the authenticated user `Box` (containing `Avatar` + `Typography` name) with `<Link to="/profile">` from `react-router-dom`, styled to remove underline.

This means `GET /api/profile` fires once on app load after login. React Query caches the result under a shared query key (e.g. `['userProfile']`). When the user navigates to `/profile`, `useUserProfile()` inside `UserProfilePage` reads from the same cache — no second network call.

### `App.tsx` change

Add route inside `<Routes>`:
```tsx
<Route path="/profile" element={<ProtectedRoute><UserProfilePage /></ProtectedRoute>} />
```

---

## API Contracts

| Method | Route | Request | Response | Auth |
|--------|-------|---------|----------|------|
| GET | `/api/profile` | — | `UserProfileResponse` (email, image, **avatarUrl**, nickname, phone, address) | Required |
| POST | `/api/profile` | `UpsertUserProfileRequest` (image, nickname, phone, address) | 204 No Content | Required |
| POST | `/api/profile/upload-avatar` | multipart file | `{ imageKey: string }` | Required |

---

## Error Handling

- **400 Validation errors**: field errors from React Query `error` + displayed via MUI `helperText`
- **401**: handled by `useApiFetcher` (throws) → `ProtectedRoute` redirects to login
- **Avatar upload fail**: show `Alert` error; keep file preview so user can retry
- **Network error on save**: persistent `Snackbar` error; form stays populated

---

## Security

- All endpoints require Auth0 JWT (existing FastEndpoints middleware)
- File upload: MIME type + extension validated on backend
- MinIO URL constructed server-side; bucket/endpoint not exposed to frontend

---

## Testing Strategy

### Backend (xUnit)

New tests in `Features/UserProfile/Tests/Integration/Endpoints/`:
- `GetAvatarUrl_WithExistingProfile_ReturnsPublicUrl`
- `GetAvatarUrl_WithNoProfile_ReturnsEmptyString`
- `GetAvatarUrl_Unauthenticated_Returns401`

### Frontend (Vitest + React Testing Library)

- `UserProfilePage` renders loaded profile data correctly
- `AvatarUploader` shows error for oversized file
- `AvatarUploader` shows error for unsupported file type
- `ProfileForm` shows validation error for invalid nickname
- `useUpdateProfile` calls avatar upload before profile upsert when file is selected

---

**Checkpoint #2: Human Review Required**

Before proceeding to `/speckit-tasks`, verify:
- [ ] Is `GET /api/profile/avatar-url` the right approach for resolving the avatar URL?
- [ ] Is the two-step save flow (upload avatar → upsert profile) correct?
- [ ] Are the frontend component boundaries appropriate?
- [ ] Is MUI + React Query + Zod usage aligned with project constitution?
- [ ] Are TDD test cases sufficient?
