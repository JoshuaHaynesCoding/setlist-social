import { useEffect, useState } from 'react';

const API_BASE_URL = 'http://localhost:5050';

export default function AuthStatus() {
  const [status, setStatus] = useState('checking');
  const [profile, setProfile] = useState(null);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    async function loadProfile() {
      try {
        const response = await fetch(`${API_BASE_URL}/api/me`, {
          credentials: 'include',
          signal: controller.signal,
        });

        if (response.status === 401) {
          setStatus('signed-out');
          return;
        }

        if (!response.ok) {
          setStatus('unavailable');
          return;
        }

        const data = await response.json();
        setProfile(data);
        setStatus('signed-in');
      } catch (error) {
        if (error.name !== 'AbortError') {
          setStatus('unavailable');
        }
      }
    }

    loadProfile();

    return () => controller.abort();
  }, []);

  async function handleLogout() {
    setIsLoggingOut(true);

    try {
      await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });
    } finally {
      setIsLoggingOut(false);
      setProfile(null);
      setStatus('signed-out');
    }
  }

  if (status === 'checking') {
    return (
      <span className="auth-note" role="status" aria-live="polite">
        Checking sign-in...
      </span>
    );
  }

  if (status === 'signed-in') {
    const displayName = profile?.displayName ?? 'User';

    return (
      <div className="auth-status">
        <span>{displayName}</span>
        <button
          className="auth-button"
          onClick={handleLogout}
          type="button"
          disabled={isLoggingOut}
        >
          {isLoggingOut ? 'Signing out...' : 'Sign out'}
        </button>
      </div>
    );
  }

  return (
    <a className="auth-button" href={`${API_BASE_URL}/api/auth/login`}>
      Sign in with Google
    </a>
  );
}
