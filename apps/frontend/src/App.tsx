import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Container, Typography, Box } from '@mui/material';
import { AuthNav } from './features/auth/components/AuthNav';
import { LoginPage } from './features/auth/components/LoginPage';
import { ProtectedRoute } from './features/auth/components/ProtectedRoute';

function HomePage() {
  return (
    <Box sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>
        Welcome to Stocker
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Your financial analysis dashboard. More features coming soon.
      </Typography>
    </Box>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AuthNav />
      <Container maxWidth="lg">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <HomePage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </Container>
    </BrowserRouter>
  );
}

export default App;
