import { useState } from 'react';
import { API_BASE_URL } from '../api.js';
import { useAuth } from '../auth/AuthContext.jsx';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';

export default function DiscoverPage() {
  const { status: authStatus } = useAuth();
  const [artist, setArtist] = useState('');
  const [results, setResults] = useState([]);
  const [status, setStatus] = useState('idle');
  const [error, setError] = useState('');
  const [savedMessage, setSavedMessage] = useState('');
  const [savingArtist, setSavingArtist] = useState('');

  async function searchArtists(event) {
    event.preventDefault();

    if (!artist.trim()) {
      setError('Enter an artist name to search.');
      setStatus('error');
      return;
    }

    try {
      setStatus('loading');
      setError('');
      setSavedMessage('');

      const url = `${API_BASE_URL}/api/external/lastfm/search?artist=${encodeURIComponent(
        artist.trim(),
      )}`;
      const response = await fetch(url);

      if (response.status === 503) {
        setError('Last.fm is not configured on the backend yet.');
        setStatus('error');
        return;
      }

      if (response.status === 400) {
        setError('Enter an artist name to search.');
        setStatus('error');
        return;
      }

      if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
      }

      const data = await response.json();
      setResults(data);
      setStatus('success');
    } catch {
      setError('Could not search Last.fm right now.');
      setStatus('error');
    }
  }

  async function addToWishlist(result) {
    setSavingArtist(result.name);
    setSavedMessage('');
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/me/wishlist`, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          artistName: result.name,
          sourceUrl: result.url ?? null,
          sourceName: 'Last.fm',
        }),
      });

      if (response.status === 401) {
        setError('Sign in to save artists.');
        setStatus('error');
        return;
      }

      if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
      }

      setSavedMessage(`${result.name} was added to your Setlist Social wishlist.`);
    } catch {
      setError('Could not add this artist to your wishlist.');
      setStatus('error');
    } finally {
      setSavingArtist('');
    }
  }

  return (
    <section className="content-section">
      <p className="eyebrow">Discover</p>
      <h1>Search for artists.</h1>
      <p className="lede narrow">
        Find artists through the backend Last.fm integration. This is public
        external data; signed-in users can save artists to their Setlist Social
        wishlist.
      </p>

      <form className="search-form" onSubmit={searchArtists}>
        <label>
          Artist name
          <input
            name="artist"
            onChange={(event) => setArtist(event.target.value)}
            placeholder="Try Cher, Radiohead, or Beyonce"
            type="search"
            value={artist}
          />
        </label>
        <button className="button primary-button" disabled={status === 'loading'} type="submit">
          {status === 'loading' ? 'Searching...' : 'Search'}
        </button>
      </form>

      {status === 'idle' ? (
        <EmptyState
          title="Start with an artist name"
          message="Search results will appear here as data from Last.fm."
        />
      ) : null}

      {status === 'loading' ? <LoadingState message="Searching Last.fm..." /> : null}

      {status === 'error' ? <ErrorState title="Search unavailable" message={error} /> : null}

      {savedMessage ? (
        <section className="state-panel success-panel" role="status" aria-live="polite">
          <h2>Saved</h2>
          <p>{savedMessage}</p>
        </section>
      ) : null}

      {status === 'success' && results.length === 0 ? (
        <EmptyState
          title="No artists found"
          message="Try another spelling or a different artist name."
        />
      ) : null}

      {status === 'success' && results.length > 0 ? (
        <section className="content-section" aria-labelledby="lastfm-results-heading">
          <div>
            <p className="eyebrow">Data from Last.fm</p>
            <h2 className="section-heading" id="lastfm-results-heading">
              Artist results
            </h2>
            {authStatus === 'signed-in' ? (
              <p className="card-detail">
                Save artists here to add them to your Setlist Social wishlist.
              </p>
            ) : (
              <p className="card-detail">Sign in to save artists.</p>
            )}
          </div>
          <div className="card-grid">
            {results.map((result) => (
              <article className="data-card artist-result-card" key={`${result.name}-${result.url}`}>
                {result.imageUrl ? (
                  <img alt="" className="artist-image" src={result.imageUrl} />
                ) : null}
                <div>
                  <p className="card-kicker">Last.fm artist</p>
                  <h2>{result.name}</h2>
                  <p className="card-detail">
                    {result.listeners?.toLocaleString() ?? 'Unknown'} listeners
                  </p>
                </div>
                {result.url ? (
                  <a className="text-link" href={result.url} rel="noreferrer" target="_blank">
                    View on Last.fm
                  </a>
                ) : null}
                {authStatus === 'signed-in' ? (
                  <button
                    className="button secondary-button"
                    disabled={savingArtist === result.name}
                    onClick={() => addToWishlist(result)}
                    type="button"
                  >
                    {savingArtist === result.name ? 'Adding...' : 'Add to wishlist'}
                  </button>
                ) : null}
              </article>
            ))}
          </div>
        </section>
      ) : null}
    </section>
  );
}
