import { Box, Button, Container, Typography, CircularProgress } from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export function LoginPage() {
  const { isAuthenticated, isLoading, login, signup } = useAuth();

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
          gap: 4,
        }}
      >
        <Typography variant="h3" component="h1" sx={{ fontWeight: 'bold' }}>
          Stocker
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Financial analysis, simplified.
        </Typography>

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, width: '100%', maxWidth: 320 }}>
          <Button
            variant="contained"
            size="large"
            startIcon={<LoginIcon />}
            onClick={login}
            fullWidth
          >
            Log In
          </Button>
          <Button
            variant="outlined"
            size="large"
            startIcon={<PersonAddIcon />}
            onClick={signup}
            fullWidth
          >
            Sign Up
          </Button>
        </Box>
      </Box>
    </Container>
  );
}
