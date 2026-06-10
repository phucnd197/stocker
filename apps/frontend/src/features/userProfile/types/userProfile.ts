import { z } from "zod";
import type { components } from "@stocker/api-contracts";

export type UserProfileResponse =
  components["schemas"]["StockerFeaturesUserProfileGetUserProfileUserProfileResponse"] & {
    avatarUrl?: string | null;
  };

export type UpsertUserProfileRequest =
  components["schemas"]["StockerFeaturesUserProfileUpsertUserProfileUpsertUserProfileRequest"];

export type UploadAvatarResponse =
  components["schemas"]["StockerFeaturesUserProfileUploadAvatarUploadAvatarResponse"];

export const profileFormSchema = z.object({
  nickname: z
    .string()
    .max(100, "Nickname must be 100 characters or less")
    .regex(
      /^[\p{L}0-9_-]*$/u,
      "Nickname can only contain letters, numbers, underscores, and dashes",
    )
    .optional()
    .or(z.literal("")),
  phone: z
    .string()
    .max(20, "Phone must be 20 characters or less")
    .regex(/^\+?[0-9\s.\-()]*$/, "Invalid phone number format")
    .optional()
    .or(z.literal("")),
  address: z
    .string()
    .max(500, "Address must be 500 characters or less")
    .regex(/^[\p{L}0-9\s,.#\-\/]*$/u, "Address contains invalid characters")
    .optional()
    .or(z.literal("")),
});

export type ProfileFormValues = z.infer<typeof profileFormSchema>;

export const AVATAR_MAX_SIZE = 5 * 1024 * 1024; // 5 MB
export const AVATAR_ACCEPTED_TYPES = [
  ".jpg",
  ".jpeg",
  ".png",
  ".gif",
  ".svg",
  ".webp",
];
export const AVATAR_ACCEPTED_MIME =
  "image/jpeg,image/png,image/gif,image/svg+xml,image/webp";
