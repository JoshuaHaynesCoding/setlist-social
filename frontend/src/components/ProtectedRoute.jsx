import { Navigate, Outlet, useLocation } from 'react-router-dom';
import ErrorState from './ErrorState.jsx';
import LoadingState from './LoadingState.jsx';
import { useAuth } from '../auth/AuthContext.jsx';

export default function ProtectedRoute() {
  const { status } = useAuth();
  const location = useLocation();

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

  return <Navigate to="/" replace state={{ from: location }} />;
}
