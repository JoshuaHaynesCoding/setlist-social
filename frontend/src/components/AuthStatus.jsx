import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext.jsx';
import { API_BASE_URL } from '../api.js';

export default function AuthStatus() {
  const { profile, signOut, status } = useAuth();
  const navigate = useNavigate();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  async function handleLogout() {
    setIsLoggingOut(true);

    try {
      await signOut();
      navigate('/', { replace: true });
    } finally {
      setIsLoggingOut(false);
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
