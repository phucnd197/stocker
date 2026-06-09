import { useAuth0 } from '@auth0/auth0-react';
import { useCallback } from 'react';

export interface AuthActions {
  login: () => void;
  signup: () => void;
  logout: () => void;
  getAccessToken: () => Promise<string>;
}

export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: {
    sub?: string;
    email?: string;
    name?: string;
    nickname?: string;
    picture?: string;
  } | null;
}

export type UseAuth = AuthState & AuthActions;

export function useAuth(): UseAuth {
  const {
    isAuthenticated,
    isLoading,
    user,
    loginWithRedirect,
    logout: auth0Logout,
    getAccessTokenSilently,
  } = useAuth0();

  const login = useCallback(() => {
    loginWithRedirect({
      authorizationParams: {
        audience: import.meta.env.VITE_AUTH0_AUDIENCE,
      },
    });
  }, [loginWithRedirect]);

  const signup = useCallback(() => {
    loginWithRedirect({
      authorizationParams: {
        audience: import.meta.env.VITE_AUTH0_AUDIENCE,
        screen_hint: 'signup',
      },
    });
  }, [loginWithRedirect]);

  const logout = useCallback(() => {
    auth0Logout({
      logoutParams: {
        returnTo: window.location.origin,
      },
    });
  }, [auth0Logout]);

  const getAccessToken = useCallback(async () => {
    return getAccessTokenSilently();
  }, [getAccessTokenSilently]);

  return {
    isAuthenticated,
    isLoading,
    user: user
      ? {
          sub: user.sub,
          email: user.email,
          name: user.name,
          nickname: user.nickname,
          picture: user.picture,
        }
      : null,
    login,
    signup,
    logout,
    getAccessToken,
  };
}
