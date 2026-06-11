import { useRef } from 'react';
import { Avatar, Box, Typography } from '@mui/material';
import { AVATAR_ACCEPTED_MIME, AVATAR_MAX_SIZE } from '../types/userProfile';

interface AvatarUploaderProps {
  previewUrl: string | null;
  onFileSelected: (file: File) => void;
  onError: (message: string) => void;
}

const validTypes = [
  'image/jpeg',
  'image/png',
  'image/gif',
  'image/svg+xml',
  'image/webp',
];
export function AvatarUploader({
  previewUrl,
  onFileSelected,
  onError,
}: AvatarUploaderProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > AVATAR_MAX_SIZE) {
      onError('File must be 5 MB or smaller.');
      e.target.value = '';
      return;
    }

    if (!validTypes.includes(file.type)) {
      onError('Only JPG, PNG, GIF, SVG, and WebP files are allowed.');
      e.target.value = '';
      return;
    }

    onFileSelected(file);
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 1,
      }}
    >
      <Avatar
        src={previewUrl ?? undefined}
        sx={{ width: 96, height: 96, cursor: 'pointer', fontSize: 40 }}
        onClick={() => inputRef.current?.click()}
        aria-label="Upload profile picture"
      />
      <Typography variant="caption" color="text.secondary">
        Click to change photo
      </Typography>
      <input
        ref={inputRef}
        type="file"
        accept={AVATAR_ACCEPTED_MIME}
        style={{ display: 'none' }}
        onChange={handleFileChange}
      />
    </Box>
  );
}
