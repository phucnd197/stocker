# Feature Specification: Update User Profile UI

**Feature Branch**: `003-update-user-profile-ui`

**Created**: 2026-06-10

**Status**: Draft

**Input**: User description: "make UI for updating user profile - the API is already present"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View and Edit Profile (Priority: P1)

A logged-in user navigates to their profile page and can view their current profile information (nickname, phone, address, and avatar). They can edit any field and save the changes, receiving confirmation that the update was successful.

**Why this priority**: Core feature — users must be able to see and update their profile data. This is the primary value of the feature.

**Independent Test**: Can be fully tested by navigating to the profile page, editing a field, saving, and verifying the updated value persists on reload.

**Acceptance Scenarios**:

1. **Given** a logged-in user with an existing profile, **When** they navigate to the profile page, **Then** they see their current nickname, phone, address, and avatar pre-filled in the form.
2. **Given** a user on the profile page, **When** they update their nickname and click Save, **Then** the changes are persisted and a success message is shown.
3. **Given** a user with no existing profile, **When** they navigate to the profile page, **Then** they see an empty form ready to be filled in.
4. **Given** a user on the profile page, **When** they submit the form with invalid data (e.g., nickname with special characters), **Then** validation errors are shown inline without navigating away.

---

### User Story 2 - Upload Avatar (Priority: P2)

A logged-in user can upload a profile picture from their device. After selecting an image, they see a preview before saving. The avatar is updated as part of the profile.

**Why this priority**: Enhances personalization but the profile is still functional without an avatar.

**Independent Test**: Can be tested by uploading a valid image file and verifying the preview appears and the avatar key is stored when the profile is saved.

**Acceptance Scenarios**:

1. **Given** a user on the profile page, **When** they click the avatar area and select a valid image file (jpg, jpeg, png, gif, svg under 5 MB), **Then** a preview of the image is shown immediately.
2. **Given** a user who has selected an avatar image, **When** they save their profile, **Then** the avatar is uploaded and linked to their profile.
3. **Given** a user attempting to upload an invalid file (wrong type or over 5 MB), **Then** an error message explains the file requirements.

---

### Edge Cases

- What happens when the save request fails due to a network error? The user sees an error message and can retry.
- What happens when the avatar upload succeeds but the profile save fails? The user is notified that profile save failed; the uploaded image key is retained so they can re-attempt saving.
- What happens when the user navigates away with unsaved changes? No prompt required (acceptable to lose unsaved state).
- What does the form show if email is present in the GET response but not editable? Email is displayed as read-only since it is managed via the identity provider.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the user's current profile data (nickname, phone, address, avatar, email) when the profile page loads. The current avatar MUST be rendered as an image resolved via a backend endpoint that converts the stored avatar key to a displayable URL.
- **FR-002**: System MUST allow users to update their nickname, phone number, and address via a form.
- **FR-003**: System MUST allow users to upload a profile avatar image (jpg, jpeg, png, gif, svg; max 5 MB).
- **FR-004**: System MUST show a preview of the selected avatar before the profile is saved.
- **FR-005**: System MUST show inline validation errors when form fields contain invalid data.
- **FR-006**: System MUST display a success notification after a profile is saved successfully.
- **FR-007**: System MUST display an error notification if saving the profile fails.
- **FR-008**: Email field MUST be shown as read-only; it cannot be edited through this form.
- **FR-009**: Avatar upload MUST happen as a separate step before the profile save, and the resulting image key is included in the profile save request.
- **FR-010**: Profile page MUST be accessible only to authenticated users.
- **FR-011**: Profile page MUST be reachable via a dedicated `/profile` route and linked from the application navigation bar.
- **FR-012**: Users MUST be able to navigate to the profile page by clicking their profile avatar/name in the navigation bar.

### Key Entities

- **User Profile**: Represents a user's personal information — nickname (display name), phone number, address, and avatar image reference.
- **Avatar**: A profile picture stored remotely; referenced by an image key returned after upload.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view and update their profile information in under 1 minute on a standard connection.
- **SC-002**: Avatar upload and preview feedback appears within 3 seconds of file selection.
- **SC-003**: Validation errors appear immediately upon form submission without a page reload.
- **SC-004**: 100% of profile save operations provide clear success or failure feedback to the user.
- **SC-005**: The profile page correctly pre-populates all existing profile data on load.

## Assumptions

- The backend API endpoints (`GET /api/profile`, `POST /api/profile`, `POST /api/profile/upload-avatar`) are already implemented and functional.
- A backend endpoint exists (or will be added) to resolve an avatar key into a displayable image URL; the frontend does not construct MinIO URLs directly.
- Authentication is handled via Auth0 and the existing `useApiFetcher` hook provides authenticated requests.
- Email is sourced from the identity provider (Auth0) and cannot be changed through the profile form.
- The frontend uses Material UI (MUI) 7.x for UI components and Zod 3.x for form validation.
- TypeScript types for the API are available in `@stocker/api-contracts` (generated via `npm run codegen`).
- No mobile-specific layout is required beyond standard responsive behaviour provided by MUI.
- The profile page is navigated to by clicking the user's profile avatar or name in the existing `AuthNav` navigation bar component.
- The profile UI is a dedicated full page accessible at the `/profile` route, not a modal or drawer.

## Clarifications

### Session 2026-06-10

- Q: Should the profile editing UI be a dedicated full page (new route) or an inline modal/drawer? → A: Dedicated page at `/profile` route
- Q: How should the frontend resolve an avatar key into a displayable image URL? → A: Backend provides an endpoint to resolve key → displayable URL
- Update: SVG added as an accepted avatar file type (jpg, jpeg, png, gif, svg; max 5 MB).
- Update: Profile page navigation entry point is the user's profile avatar/name in the nav bar (clickable link).
