# API Contracts: Update User Profile UI

## Existing Endpoints (unchanged)

### GET /api/profile
Returns the authenticated user's profile data.

**Response 200**:
```json
{
  "email": "user@example.com",
  "image": "avatars/3f2a1b_profile.jpg",
  "nickname": "john_doe",
  "phone": "+1 555 123 4567",
  "address": "123 Main St, Springfield"
}
```
**Response 401**: Unauthorized

---

### POST /api/profile
Creates or updates the authenticated user's profile.

**Request Body**:
```json
{
  "image": "avatars/3f2a1b_profile.jpg",
  "nickname": "john_doe",
  "phone": "+1 555 123 4567",
  "address": "123 Main St, Springfield"
}
```
**Response 204**: No Content (success)
**Response 400**: Validation errors
```json
{
  "errors": [
    { "name": "nickname", "reason": "..." }
  ]
}
```
**Response 401**: Unauthorized

---

### POST /api/profile/upload-avatar
Uploads an avatar image to MinIO. Returns the object key to be saved via POST /api/profile.

**Request**: `multipart/form-data` with a single image file
- Allowed types: jpg, jpeg, png, gif, svg
- Max size: 5 MB

**Response 200**:
```json
{
  "imageKey": "avatars/3f2a1b93-1234-5678-abcd-efgh_profile.jpg"
}
```
**Response 400**: Invalid file error
**Response 401**: Unauthorized

---

## New Endpoint

### GET /api/profile/avatar-url
Resolves the authenticated user's stored avatar key to a full public URL.

**Response 200**:
```json
{
  "avatarUrl": "http://localhost:9000/stocker-public/avatars/3f2a1b_profile.jpg"
}
```
Returns `{ "avatarUrl": "" }` if the user has no profile or no avatar set.

**Response 401**: Unauthorized

---

## Frontend API Call Signatures

```typescript
// GET /api/profile
getUserProfile(): Promise<UserProfileResponse>

// POST /api/profile
upsertUserProfile(data: UpsertUserProfileRequest): Promise<void>

// POST /api/profile/upload-avatar
uploadAvatar(file: File): Promise<{ imageKey: string }>

// GET /api/profile/avatar-url
getAvatarUrl(): Promise<{ avatarUrl: string }>
```
