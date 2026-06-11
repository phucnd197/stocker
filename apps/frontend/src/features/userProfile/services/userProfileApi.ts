import type {
  UserProfileResponse,
  UpsertUserProfileRequest,
  UploadAvatarResponse,
} from '../types/userProfile';

export function createUserProfileApi(
  fetcher: <T>(
    path: string,
    options?: RequestInit,
    defaultContentType?: boolean
  ) => Promise<T>
) {
  return {
    getProfile: () => fetcher<UserProfileResponse>('/api/profile'),

    upsertProfile: (data: UpsertUserProfileRequest) =>
      fetcher<void>('/api/profile', {
        method: 'POST',
        body: JSON.stringify(data),
      }),

    // multipart upload — bypasses the fetcher's forced Content-Type header
    uploadAvatar: async (file: File) => {
      // const token = await getToken();
      const formData = new FormData();
      formData.append('file', file);
      const res = await fetcher<UploadAvatarResponse>(
        '/api/profile/upload-avatar',
        {
          method: 'POST',
          body: formData,
        },
        false
      );
      return res;
    },
  };
}
