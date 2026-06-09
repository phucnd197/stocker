import { Button } from '@mui/material';
import LogoutIcon from '@mui/icons-material/Logout';
import { useAuth } from '../hooks/useAuth';

export function LogoutButton() {
  const { logout } = useAuth();

  return (
    <Button
      variant="outlined"
      color="inherit"
      size="small"
      startIcon={<LogoutIcon />}
      onClick={logout}
      aria-label="Log out"
    >
      Log Out
    </Button>
  );
}
