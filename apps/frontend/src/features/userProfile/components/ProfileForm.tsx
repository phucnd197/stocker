import { TextField, Stack } from '@mui/material';
import type { ProfileFormValues } from '../types/userProfile';

interface FieldErrors {
  nickname?: string;
  phone?: string;
  address?: string;
}

interface ProfileFormProps {
  email: string;
  values: ProfileFormValues;
  errors: FieldErrors;
  onChange: (field: keyof ProfileFormValues, value: string) => void;
}

export function ProfileForm({
  email,
  values,
  errors,
  onChange,
}: ProfileFormProps) {
  return (
    <Stack spacing={2} sx={{ width: '100%' }}>
      <TextField
        label="Email"
        value={email}
        disabled
        fullWidth
        helperText="Email is managed by your identity provider and cannot be changed here."
      />
      <TextField
        label="Nickname"
        value={values.nickname ?? ''}
        onChange={e => onChange('nickname', e.target.value)}
        error={!!errors.nickname}
        helperText={errors.nickname}
        fullWidth
        slotProps={{ htmlInput: { maxLength: 100 } }}
      />
      <TextField
        label="Phone"
        value={values.phone ?? ''}
        onChange={e => onChange('phone', e.target.value)}
        error={!!errors.phone}
        helperText={errors.phone}
        fullWidth
        slotProps={{ htmlInput: { maxLength: 20 } }}
      />
      <TextField
        label="Address"
        value={values.address ?? ''}
        onChange={e => onChange('address', e.target.value)}
        error={!!errors.address}
        helperText={errors.address}
        fullWidth
        multiline
        rows={3}
        slotProps={{ htmlInput: { maxLength: 500 } }}
      />
    </Stack>
  );
}
