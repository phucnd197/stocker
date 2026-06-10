import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Avatar,
  Box,
  CircularProgress,
} from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { LogoutButton } from './LogoutButton';
import { useUserProfile } from '../../userProfile';

export function AuthNav() {
  const { isAuthenticated, isLoading, user, login, signup } = useAuth();
  const { data: profile } = useUserProfile();

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Stocker
        </Typography>

        {isLoading && <CircularProgress size={24} color="inherit" />}

        {!isLoading && !isAuthenticated && (
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              color="inherit"
              size="small"
              startIcon={<LoginIcon />}
              onClick={login}
              aria-label="Log in"
            >
              Log In
            </Button>
            <Button
              color="inherit"
              size="small"
              variant="outlined"
              startIcon={<PersonAddIcon />}
              onClick={signup}
              aria-label="Sign up"
            >
              Sign Up
            </Button>
          </Box>
        )}

        {!isLoading && isAuthenticated && user && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box
              component={Link}
              to="/profile"
              sx={{ display: 'flex', alignItems: 'center', gap: 1, color: 'inherit', textDecoration: 'none' }}
            >
              <Typography variant="body2">{user.name || user.email}</Typography>
              <Avatar
                src={profile?.avatarUrl ?? user.picture}
                alt={user.name || 'User'}
                sx={{ width: 32, height: 32 }}
              />
            </Box>
            <LogoutButton />
          </Box>
        )}
      </Toolbar>
    </AppBar>
  );
}
