# Research: Update User Profile UI

## Avatar URL Resolution

**Decision**: Backend exposes a new `GET /api/profile/avatar-url` endpoint that returns the full public URL for the authenticated user's avatar.

**Rationale**: The avatar is stored in a MinIO `PublicBucket` ("stocker-public"). The bucket is public, so the URL is `http://{MinioEndpoint}/{bucket}/{objectKey}`. However, exposing MinIO's endpoint and bucket name directly to the frontend couples it to the storage infrastructure. A thin backend endpoint that constructs and returns the URL keeps storage config server-side and allows future migration to presigned URLs without frontend changes.

**Alternatives considered**:
- Frontend constructs URL directly: Would require exposing MinIO endpoint to frontend via env vars — tight coupling to storage layer.
- Presigned URL: Unnecessary complexity for a public bucket; public URLs are sufficient.

---

## SVG Upload Support

**Decision**: SVG is already supported in `UploadAvatarEndpoint.cs`. The allowed extensions array includes `"svg"` (note: missing leading dot — this is a minor bug to fix). The image validator regex in `UpsertUserProfileRequest` already allows `svg` extension.

**Action required**: Fix the missing `.` before `svg` in `allowedExtensions` in `UploadAvatarEndpoint.cs` (line 12: `"svg"` should be `".svg"`).

---

## No New Database Migration Required

**Decision**: No schema changes are needed. The `UserProfile` entity and migration already exist (`20260610050747_UserProfile`). This feature is purely a frontend UI addition plus one new backend endpoint.

---

## Frontend Architecture

**Decision**: New React feature at `apps/frontend/src/features/userProfile/` following the existing `auth` feature pattern.

Key libraries already in the project:
- **MUI 7.x**: Use `TextField`, `Avatar`, `Button`, `Snackbar`/`Alert` for UI
- **Zod 3.x**: Schema validation mirroring backend rules
- **React Query** (if present) or `useApiFetcher` hook for API calls
- **React Router**: Add `/profile` route (if React Router is already used)

**Action**: Check if React Router and React Query are installed in the frontend.
