import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import { API_BASE_URL, apiFetch } from '../api.js';

const ACTIVITY_URL = `${API_BASE_URL}/api/public/activity`;
const ACTIVITY_HUB_URL = `${API_BASE_URL}/hubs/activity`;

function formatDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

function normalizeActivity(item) {
  const type = item.type ?? item.eventType ?? 'Activity';
  const message = item.message ?? item.summary ?? '';
  const userDisplayName = item.userDisplayName ?? null;
  const createdAt = item.createdAt;
  const artistDisplayText = item.artistDisplayText ?? null;
  const concertDisplayText = item.concertDisplayText ?? item.concertTitle ?? null;
  const key = item.id
    ? `activity-${item.id}`
    : `${type}:${message}:${userDisplayName ?? ''}:${createdAt}`;

  return {
    key,
    type,
    message,
    userDisplayName,
    createdAt,
    artistDisplayText,
    concertDisplayText,
  };
}

function prependActivity(activityItem) {
  const normalizedItem = normalizeActivity(activityItem);

  return (currentActivity) => {
    if (currentActivity.some((item) => item.key === normalizedItem.key)) {
      return currentActivity;
    }

    return [normalizedItem, ...currentActivity].slice(0, 25);
  };
}

function getLiveStatusText(liveStatus) {
  if (liveStatus === 'connected') {
    return 'Live updates connected';
  }

  if (liveStatus === 'reconnecting') {
    return 'Live updates reconnecting';
  }

  if (liveStatus === 'connecting') {
    return 'Connecting live updates';
  }

  return 'Live updates unavailable';
}

export default function ActivityPage() {
  const [activity, setActivity] = useState([]);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');
  const [liveStatus, setLiveStatus] = useState('connecting');

  useEffect(() => {
    const controller = new AbortController();

    async function loadActivity() {
      try {
        setStatus('loading');
        setError('');

        const response = await apiFetch(ACTIVITY_URL, { signal: controller.signal });

        if (!response.ok) {
          throw new Error(`Backend returned ${response.status}`);
        }

        const data = await response.json();
        setActivity(data.map(normalizeActivity));
        setStatus('success');
      } catch (requestError) {
        if (requestError.name === 'AbortError') {
          return;
        }

        setError(`Could not reach the backend at ${API_BASE_URL}. Start the backend and try again.`);
        setStatus('error');
      }
    }

    loadActivity();

    return () => controller.abort();
  }, []);

  useEffect(() => {
    let isMounted = true;
    const connection = new HubConnectionBuilder()
      .withUrl(ACTIVITY_HUB_URL)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('activityCreated', (activityItem) => {
      setActivity(prependActivity(activityItem));
      setStatus((currentStatus) => (currentStatus === 'error' ? 'success' : currentStatus));
    });

    connection.onreconnecting(() => {
      if (isMounted) {
        setLiveStatus('reconnecting');
      }
    });

    connection.onreconnected(() => {
      if (isMounted) {
        setLiveStatus('connected');
      }
    });

    connection.onclose(() => {
      if (isMounted) {
        setLiveStatus('unavailable');
      }
    });

    async function connect() {
      try {
        setLiveStatus('connecting');
        await connection.start();

        if (isMounted) {
          setLiveStatus('connected');
        }
      } catch {
        if (isMounted) {
          setLiveStatus('unavailable');
        }
      }
    }

    connect();

    return () => {
      isMounted = false;

      if (connection.state !== HubConnectionState.Disconnected) {
        connection.stop();
      }
    };
  }, []);

  return (
    <section className="content-section">
      <p className="eyebrow">Activity</p>
      <h1>Recent public activity.</h1>
      <p className="lede narrow">
        This public feed loads recent activity first, then listens for live
        updates from signed-in community actions.
      </p>
      <p
        className={`live-status live-status-${liveStatus}`}
        role="status"
        aria-label="Live updates"
        aria-live="polite"
      >
        {getLiveStatusText(liveStatus)}
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
            <article className="activity-card" key={item.key}>
              <div className="activity-marker" aria-hidden="true" />
              <div>
                <div className="activity-heading">
                  <span>{item.type}</span>
                  <time dateTime={item.createdAt}>{formatDate(item.createdAt)}</time>
                </div>
                <p>{item.message}</p>
                <p className="card-detail">
                  {item.userDisplayName ? `By ${item.userDisplayName}` : 'Community activity'}
                  {item.concertDisplayText ? ` · ${item.concertDisplayText}` : ''}
                </p>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
