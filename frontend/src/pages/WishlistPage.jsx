import { useEffect, useState } from 'react';
import { API_BASE_URL } from '../api.js';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';

function formatSavedDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
  }).format(new Date(value));
}

export default function WishlistPage() {
  const [items, setItems] = useState([]);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');
  const [deletingId, setDeletingId] = useState(null);

  async function loadWishlist(signal) {
    try {
      setStatus('loading');
      setError('');

      const response = await fetch(`${API_BASE_URL}/api/me/wishlist`, {
        credentials: 'include',
        signal,
      });

      if (response.status === 401) {
        setError('Sign in again to view your wishlist.');
        setStatus('error');
        return;
      }

      if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
      }

      const data = await response.json();
      setItems(data);
      setStatus('success');
    } catch (requestError) {
      if (requestError.name === 'AbortError') {
        return;
      }

      setError('Could not load your wishlist from the backend.');
      setStatus('error');
    }
  }

  useEffect(() => {
    const controller = new AbortController();
    loadWishlist(controller.signal);
    return () => controller.abort();
  }, []);

  async function deleteWishlistItem(id) {
    setDeletingId(id);
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL}/api/me/wishlist/${id}`, {
        method: 'DELETE',
        credentials: 'include',
      });

      if (response.status === 401) {
        setError('Sign in again before deleting wishlist items.');
        setStatus('error');
        return;
      }

      if (!response.ok && response.status !== 404) {
        throw new Error(`Backend returned ${response.status}`);
      }

      await loadWishlist();
    } catch {
      setError('Could not delete this wishlist item.');
      setStatus('error');
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="content-section">
      <p className="eyebrow">Wishlist</p>
      <h1>Saved artists.</h1>
      <p className="lede narrow">
        This is your Setlist Social wishlist. Artists saved from Discover are
        stored here as signed-in, user-owned data.
      </p>

      {status === 'loading' ? <LoadingState message="Loading your wishlist..." /> : null}

      {status === 'error' ? <ErrorState title="Wishlist unavailable" message={error} /> : null}

      {status === 'success' && items.length === 0 ? (
        <EmptyState
          title="No wishlist artists yet"
          message="Use Discover to search Last.fm and save artists here."
        />
      ) : null}

      {status === 'success' && items.length > 0 ? (
        <section className="content-section" aria-labelledby="wishlist-heading">
          <div>
            <p className="eyebrow">Saved in Setlist Social</p>
            <h2 className="section-heading" id="wishlist-heading">
              Your wishlist
            </h2>
          </div>
          <div className="concert-list">
            {items.map((item) => (
              <article className="data-card concert-card" key={item.id}>
                <div>
                  <p className="card-kicker">{item.sourceName ?? 'Wishlist artist'}</p>
                  <h2>{item.artistName}</h2>
                  <p className="card-detail">Saved {formatSavedDate(item.createdAt)}</p>
                  {item.sourceUrl ? (
                    <a className="text-link" href={item.sourceUrl} rel="noreferrer" target="_blank">
                      View source
                    </a>
                  ) : null}
                </div>
                <div className="card-actions">
                  <button
                    className="button danger-button"
                    disabled={deletingId === item.id}
                    onClick={() => deleteWishlistItem(item.id)}
                    type="button"
                  >
                    {deletingId === item.id ? 'Deleting...' : 'Delete'}
                  </button>
                </div>
              </article>
            ))}
          </div>
        </section>
      ) : null}
    </section>
  );
}
