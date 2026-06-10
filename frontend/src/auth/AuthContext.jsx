import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { API_BASE_URL } from '../api.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [profile, setProfile] = useState(null);
  const [status, setStatus] = useState('checking');

  const refreshProfile = useCallback(async (signal) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/me`, {
        credentials: 'include',
        signal,
      });

      if (response.status === 401) {
        setProfile(null);
        setStatus('signed-out');
        return null;
      }

      if (!response.ok) {
        setProfile(null);
        setStatus('unavailable');
        return null;
      }

      const data = await response.json();
      setProfile(data);
      setStatus('signed-in');
      return data;
    } catch (error) {
      if (error.name !== 'AbortError') {
        setProfile(null);
        setStatus('unavailable');
      }

      return null;
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    refreshProfile(controller.signal);
    return () => controller.abort();
  }, [refreshProfile]);

  const signOut = useCallback(async () => {
    await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      credentials: 'include',
    });

    setProfile(null);
    setStatus('signed-out');
  }, []);

  const value = useMemo(
    () => ({
      profile,
      refreshProfile,
      signOut,
      status,
    }),
    [profile, refreshProfile, signOut, status],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }

  return context;
}
