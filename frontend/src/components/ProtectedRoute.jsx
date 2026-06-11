import { Outlet } from 'react-router-dom';
import EmptyState from './EmptyState.jsx';
import ErrorState from './ErrorState.jsx';
import LoadingState from './LoadingState.jsx';
import { API_BASE_URL } from '../api.js';
import { useAuth } from '../auth/AuthContext.jsx';

export default function ProtectedRoute() {
  const { status } = useAuth();

  if (status === 'checking') {
    return <LoadingState message="Checking your session..." />;
  }

  if (status === 'signed-in') {
    return <Outlet />;
  }

  if (status === 'unavailable') {
    return (
      <ErrorState
        title="Sign-in status is unavailable"
        message="Start the backend and refresh the page before opening this protected area."
      />
    );
  }

  return (
    <section className="content-section">
      <EmptyState
        title="Sign in required"
        message="Use the Google sign-in button to open this Setlist Social page."
      />
      <a className="button primary-button" href={`${API_BASE_URL}/api/auth/login`}>
        Sign in with Google
      </a>
    </section>
  );
}
