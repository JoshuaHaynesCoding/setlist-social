import { useEffect, useState } from 'react';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import { API_BASE_URL } from '../api.js';

const ARTISTS_URL = `${API_BASE_URL}/api/public/artists`;

export default function ArtistsPage() {
  const [artists, setArtists] = useState([]);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');

  useEffect(() => {
    const controller = new AbortController();

    async function loadArtists() {
      try {
        setStatus('loading');
        setError('');

        const response = await fetch(ARTISTS_URL, { signal: controller.signal });

        if (!response.ok) {
          throw new Error(`Backend returned ${response.status}`);
        }

        const data = await response.json();
        setArtists(data);
        setStatus('success');
      } catch (requestError) {
        if (requestError.name === 'AbortError') {
          return;
        }

        setError(`Could not reach the backend at ${API_BASE_URL}. Start the backend and try again.`);
        setStatus('error');
      }
    }

    loadArtists();

    return () => controller.abort();
  }, []);

  return (
    <section className="content-section">
      <p className="eyebrow">Artists</p>
      <h1>Artists in the community database.</h1>
      <p className="lede narrow">
        This public page reads artists from the Setlist Social backend. Last.fm
        search and richer artist profiles are still planned for later.
      </p>

      {status === 'loading' ? <LoadingState message="Loading artists..." /> : null}

      {status === 'error' ? (
        <ErrorState title="Artists are unavailable" message={error} />
      ) : null}

      {status === 'success' && artists.length === 0 ? (
        <EmptyState
          title="No artists yet"
          message="Seed the local development database to see sample artists."
        />
      ) : null}

      {status === 'success' && artists.length > 0 ? (
        <div className="card-grid">
          {artists.map((artist) => (
            <article className="data-card" key={artist.id}>
              <div>
                <p className="card-kicker">Artist</p>
                <h2>{artist.name}</h2>
              </div>
              <dl className="metric-row">
                <div>
                  <dt>Concerts</dt>
                  <dd>{artist.concertCount}</dd>
                </div>
                <div>
                  <dt>Reviews</dt>
                  <dd>{artist.reviewCount}</dd>
                </div>
              </dl>
              {artist.latestConcert ? (
                <p className="card-detail">
                  Latest: {artist.latestConcert.title}
                  {artist.latestConcert.city ? ` in ${artist.latestConcert.city}` : ''}
                </p>
              ) : (
                <p className="card-detail">No concerts attached yet.</p>
              )}
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
