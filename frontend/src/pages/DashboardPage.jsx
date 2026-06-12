import { useEffect, useState } from 'react';
import { apiFetch } from '../api.js';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import StatCard from '../components/StatCard.jsx';

const dashboardStats = [
  { key: 'concerts', label: 'Concerts', detail: 'Concerts connected to your profile' },
  { key: 'reviews', label: 'Reviews', detail: 'Reviews you have written' },
  { key: 'wishlistItems', label: 'Wishlist', detail: 'Saved artists or shows' },
  { key: 'recentActivityEvents', label: 'Activity', detail: 'Recent activity records' },
];

export default function DashboardPage() {
  const [dashboard, setDashboard] = useState(null);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');

  useEffect(() => {
    const controller = new AbortController();

    async function loadDashboard() {
      try {
        setStatus('loading');
        setError('');

        const response = await apiFetch('/api/me/dashboard', {
          signal: controller.signal,
        });

        if (response.status === 401) {
          setError('Your session is not signed in. Sign in again to view your dashboard.');
          setStatus('error');
          return;
        }

        if (!response.ok) {
          throw new Error(`Backend returned ${response.status}`);
        }

        const data = await response.json();
        setDashboard(data);
        setStatus('success');
      } catch (requestError) {
        if (requestError.name === 'AbortError') {
          return;
        }

        setError('Could not load your dashboard from the backend.');
        setStatus('error');
      }
    }

    loadDashboard();

    return () => controller.abort();
  }, []);

  const counts = dashboard?.counts;
  const hasAnyCount = counts && dashboardStats.some((stat) => Number(counts[stat.key] ?? 0) > 0);

  return (
    <section className="content-section">
      <p className="eyebrow">Dashboard</p>
      <h1>Your Setlist Social snapshot.</h1>
      <p className="lede narrow">
        See your profile, concert activity, reviews, and wishlist counts in one place.
      </p>

      {status === 'loading' ? <LoadingState message="Loading your dashboard..." /> : null}

      {status === 'error' ? (
        <ErrorState title="Dashboard unavailable" message={error} />
      ) : null}

      {status === 'success' && !dashboard ? (
        <EmptyState title="No dashboard found" message="Sign out and sign in again to refresh your profile." />
      ) : null}

      {status === 'success' && dashboard ? (
        <>
          <article className="profile-summary">
            <p className="card-kicker">Signed in as</p>
            <h2>{dashboard.profile.displayName}</h2>
            <p>{dashboard.profile.bio ?? 'No bio has been added yet.'}</p>
          </article>

          {!hasAnyCount ? (
            <EmptyState
              title="No personal activity yet"
              message="Add a concert or save artists to your wishlist to start filling in your dashboard."
            />
          ) : null}

          <div className="stats-grid">
            {dashboardStats.map((stat) => (
              <StatCard
                detail={stat.detail}
                key={stat.key}
                label={stat.label}
                value={Number(counts?.[stat.key] ?? 0)}
              />
            ))}
          </div>
        </>
      ) : null}
    </section>
  );
}
