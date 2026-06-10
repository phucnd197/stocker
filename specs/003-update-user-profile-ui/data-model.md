# Data Model: Update User Profile UI

## Existing Entity: `UserProfile`

No schema changes required. Entity is fully defined.

**Table**: `UserProfiles`
**Primary Key**: `UserId` (Guid — matches Auth0 subject claim)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| UserId | Guid | PK, required | Auth0 user ID |
| Image | string | max 200 chars, nullable | MinIO object key, e.g. `avatars/{guid}_{filename}` |
| Nickname | string | max 100 chars, nullable | Alphanumeric, `_`, `-` only |
| Phone | string | max 20 chars, nullable | Flexible format: `+`, digits, spaces, `.`, `-`, `()` |
| Address | string | max 500 chars, nullable | Letters, digits, spaces, `,`, `.`, `#`, `-`, `/` |
| IsDeleted | bool | default false | Soft delete flag |
| DeletedAt | DateTime | — | Set on soft delete |

**Soft delete**: Global query filter in `StockerDataContext` automatically excludes `IsDeleted = true` records.

---

## New API Response Shape: `GetAvatarUrlResponse`

| Field | Type | Notes |
|-------|------|-------|
| AvatarUrl | string | Full public URL to avatar image, or empty string if no avatar |

**URL format**: `http://{MinioEndpoint}/{PublicBucket}/{imageKey}`

Example: `http://localhost:9000/stocker-public/avatars/3f2a1b_profile.jpg`

---

## Frontend Types (Zod schemas)

### `profileFormSchema`

```
nickname: string (max 100, /^[\p{L}0-9_-]+$/u, optional)
phone:    string (max 20, /^\+?[0-9\s.\-()]+$/, optional)
address:  string (max 500, /^[\p{L}0-9\s,.#\-\/]+$/u, optional)
image:    string (max 200, optional)
```

### `UserProfileData` (from `GET /api/profile`)

Inferred from `@stocker/api-contracts`:
```
email:    string
image:    string (MinIO key)
nickname: string
phone:    string
address:  string
```

### `AvatarUrlData` (from `GET /api/profile/avatar-url`)

Inferred from `@stocker/api-contracts` (after codegen):
```
avatarUrl: string
```
