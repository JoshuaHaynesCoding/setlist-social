import { useState } from 'react';
import { useAuth } from '../auth/AuthContext.jsx';

export default function AuthStatus() {
  const { profile, signOut, status } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  async function handleLogout() {
    setIsLoggingOut(true);

    try {
      await signOut();
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
    <a className="auth-button" href="/api/auth/login">
      Sign in with Google
    </a>
  );
}
