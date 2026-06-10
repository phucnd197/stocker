import { useState } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  Alert,
  Snackbar,
  Typography,
  Paper,
} from '@mui/material';
import { profileFormSchema } from '../types/userProfile';
import type { ProfileFormValues } from '../types/userProfile';
import { useUserProfile } from '../hooks/useUserProfile';
import { useUpdateProfile } from '../hooks/useUpdateProfile';
import { AvatarUploader } from './AvatarUploader';
import { ProfileForm } from './ProfileForm';
import { useAuth0 } from '@auth0/auth0-react';

export function UserProfilePage() {
  const { data: profile, isLoading, isError } = useUserProfile();
  const { user } = useAuth0();
  const updateProfile = useUpdateProfile();

  const [values, setValues] = useState<ProfileFormValues>({
    nickname: profile?.nickname,
    phone: profile?.phone,
    address: profile?.address,
  });
  const [fieldErrors, setFieldErrors] = useState<Partial<ProfileFormValues>>(
    {}
  );
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(
    profile?.avatarUrl ?? ''
  );
  const [avatarError, setAvatarError] = useState<string | null>(null);
  const [successOpen, setSuccessOpen] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  // useEffect(() => {
  //   if (profile) {
  //     setValues(() => ({
  //       nickname: profile.nickname ?? '',
  //       phone: profile.phone ?? '',
  //       address: profile.address ?? '',
  //     }));
  //     setPreviewUrl(profile.avatarUrl ?? null);
  //   }
  // }, [profile]);

  function handleFileSelected(file: File) {
    setAvatarError(null);
    setAvatarFile(file);
    setPreviewUrl(URL.createObjectURL(file));
  }

  function handleFieldChange(field: keyof ProfileFormValues, value: string) {
    setValues(prev => ({ ...prev, [field]: value }));
    setFieldErrors(prev => ({ ...prev, [field]: undefined }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSaveError(null);

    const result = profileFormSchema.safeParse(values);
    if (!result.success) {
      const errors: Partial<ProfileFormValues> = {};
      for (const issue of result.error.issues) {
        const field = issue.path[0] as keyof ProfileFormValues;
        errors[field] = issue.message;
      }
      setFieldErrors(errors);
      return;
    }

    try {
      await updateProfile.mutateAsync({
        values: result.data,
        avatarFile,
        existingImageKey: profile?.image ?? '',
      });
      setAvatarFile(null);
      setSuccessOpen(true);
    } catch (ex) {
      console.log(ex);
      setSaveError('Failed to save profile. Please try again.');
    }
  }

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isError) {
    return (
      <Box sx={{ py: 4 }}>
        <Alert severity="error">
          Failed to load profile. Please refresh the page.
        </Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 480, mx: 'auto', py: 4 }}>
      <Typography variant="h5" gutterBottom>
        Edit Profile
      </Typography>

      <Paper sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit}>
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              mb: 3,
            }}
          >
            <AvatarUploader
              previewUrl={previewUrl}
              onFileSelected={handleFileSelected}
              onError={setAvatarError}
            />
            {avatarError && (
              <Alert severity="error" sx={{ mt: 1, width: '100%' }}>
                {avatarError}
              </Alert>
            )}
          </Box>

          <ProfileForm
            email={user?.email ?? ''}
            values={values}
            errors={fieldErrors}
            onChange={handleFieldChange}
          />

          {saveError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {saveError}
            </Alert>
          )}

          <Button
            type="submit"
            variant="contained"
            fullWidth
            sx={{ mt: 3 }}
            disabled={updateProfile.isPending}
          >
            {updateProfile.isPending ? 'Saving…' : 'Save Profile'}
          </Button>
        </Box>
      </Paper>

      <Snackbar
        open={successOpen}
        autoHideDuration={4000}
        onClose={() => setSuccessOpen(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" onClose={() => setSuccessOpen(false)}>
          Profile saved successfully.
        </Alert>
      </Snackbar>
    </Box>
  );
}
