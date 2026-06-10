import { useEffect, useState } from 'react';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';

const ACTIVITY_URL = 'http://localhost:5050/api/public/activity';

function formatDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export default function ActivityPage() {
  const [activity, setActivity] = useState([]);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');

  useEffect(() => {
    const controller = new AbortController();

    async function loadActivity() {
      try {
        setStatus('loading');
        setError('');

        const response = await fetch(ACTIVITY_URL, { signal: controller.signal });

        if (!response.ok) {
          throw new Error(`Backend returned ${response.status}`);
        }

        const data = await response.json();
        setActivity(data);
        setStatus('success');
      } catch (requestError) {
        if (requestError.name === 'AbortError') {
          return;
        }

        setError('Could not reach the backend at http://localhost:5050. Start the backend and try again.');
        setStatus('error');
      }
    }

    loadActivity();

    return () => controller.abort();
  }, []);

  return (
    <section className="content-section">
      <p className="eyebrow">Activity</p>
      <h1>Recent public activity.</h1>
      <p className="lede narrow">
        This page reads recent activity events from the backend. It is a simple
        public list for now; real-time SignalR updates are still planned.
      </p>

      {status === 'loading' ? <LoadingState message="Loading activity..." /> : null}

      {status === 'error' ? (
        <ErrorState title="Activity is unavailable" message={error} />
      ) : null}

      {status === 'success' && activity.length === 0 ? (
        <EmptyState
          title="No activity yet"
          message="Seed the local development database to see sample activity."
        />
      ) : null}

      {status === 'success' && activity.length > 0 ? (
        <div className="activity-list">
          {activity.map((item) => (
            <article className="activity-card" key={item.id}>
              <div className="activity-marker" aria-hidden="true" />
              <div>
                <div className="activity-heading">
                  <span>{item.eventType}</span>
                  <time dateTime={item.createdAt}>{formatDate(item.createdAt)}</time>
                </div>
                <p>{item.summary}</p>
                <p className="card-detail">
                  {item.userDisplayName ? `By ${item.userDisplayName}` : 'Community activity'}
                  {item.concertTitle ? ` · ${item.concertTitle}` : ''}
                </p>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
