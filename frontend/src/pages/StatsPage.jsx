import { useEffect, useState } from 'react';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import StatCard from '../components/StatCard.jsx';
import { API_BASE_URL } from '../api.js';

const STATS_URL = `${API_BASE_URL}/api/public/stats`;

const statLabels = [
  { key: 'users', label: 'Users', detail: 'Public user profiles' },
  { key: 'artists', label: 'Artists', detail: 'Artists in the local database' },
  { key: 'concerts', label: 'Concerts', detail: 'Concerts tracked so far' },
  { key: 'reviews', label: 'Reviews', detail: 'Community concert notes' },
  { key: 'wishlistItems', label: 'Wishlist', detail: 'Shows or artists saved for later' },
  { key: 'activityEvents', label: 'Activity', detail: 'Public activity records' },
  { key: 'tags', label: 'Tags', detail: 'Labels attached to concerts' },
];

export default function StatsPage() {
  const [stats, setStats] = useState(null);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');

  useEffect(() => {
    const controller = new AbortController();

    async function loadStats() {
      try {
        setStatus('loading');
        setError('');

        const response = await fetch(STATS_URL, { signal: controller.signal });

        if (!response.ok) {
          throw new Error(`Backend returned ${response.status}`);
        }

        const data = await response.json();
        setStats(data);
        setStatus('success');
      } catch (requestError) {
        if (requestError.name === 'AbortError') {
          return;
        }

        setError(`Could not reach the backend at ${API_BASE_URL}. Start the backend, apply migrations, and try again.`);
        setStatus('error');
      }
    }

    loadStats();

    return () => controller.abort();
  }, []);

  const hasStats = stats && statLabels.some((stat) => Number(stats[stat.key] ?? 0) > 0);

  return (
    <section className="content-section">
      <p className="eyebrow">Community stats</p>
      <h1>What is in the Setlist Social database?</h1>
      <p className="lede narrow">
        These cards read from the public backend stats endpoint. Seed the local
        development database to see sample data.
      </p>

      {status === 'loading' ? <LoadingState message="Loading community stats..." /> : null}

      {status === 'error' ? (
        <ErrorState title="Backend is not available" message={error} />
      ) : null}

      {status === 'success' && !hasStats ? (
        <EmptyState
          title="No stats yet"
          message="The backend is running, but the database has no sample data yet."
        />
      ) : null}

      {status === 'success' && hasStats ? (
        <div className="stats-grid">
          {statLabels.map((stat) => (
            <StatCard
              detail={stat.detail}
              key={stat.key}
              label={stat.label}
              value={Number(stats[stat.key] ?? 0)}
            />
          ))}
        </div>
      ) : null}
    </section>
  );
}
