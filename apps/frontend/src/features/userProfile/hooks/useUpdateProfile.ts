import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApiFetcher } from "../../auth/hooks/useApiFetcher";
import { createUserProfileApi } from "../services/userProfileApi";
import { USER_PROFILE_QUERY_KEY } from "./useUserProfile";
import type { ProfileFormValues } from "../types/userProfile";

export function useUpdateProfile() {
  const fetcher = useApiFetcher();
  const api = createUserProfileApi(fetcher);
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      values,
      avatarFile,
      existingImageKey,
    }: {
      values: ProfileFormValues;
      avatarFile: File | null;
      existingImageKey?: string;
    }) => {
      let imageKey = existingImageKey ?? "";

      if (avatarFile) {
        const result = await api.uploadAvatar(avatarFile);
        imageKey = result.imageKey ?? "";
      }

      await api.upsertProfile({
        image: imageKey,
        nickname: values.nickname ?? "",
        phone: values.phone ?? "",
        address: values.address ?? "",
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: USER_PROFILE_QUERY_KEY });
    },
  });
}
