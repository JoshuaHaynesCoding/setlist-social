import { useEffect, useMemo, useState } from 'react';
import { apiFetch } from '../api.js';
import EmptyState from '../components/EmptyState.jsx';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';

const emptyForm = {
  title: '',
  artistName: '',
  venueName: '',
  city: '',
  region: '',
  country: '',
  concertDate: '',
};

function toDateInputValue(value) {
  if (!value) {
    return '';
  }

  return new Date(value).toISOString().slice(0, 10);
}

function formatConcertDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
  }).format(new Date(value));
}

function toPayload(form) {
  return {
    title: form.title,
    artistName: form.artistName,
    venueName: form.venueName || null,
    city: form.city || null,
    region: form.region || null,
    country: form.country || null,
    concertDate: form.concertDate ? new Date(`${form.concertDate}T12:00:00`).toISOString() : '',
  };
}

export default function MyConcertsPage() {
  const [concerts, setConcerts] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');
  const [formError, setFormError] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [deletingId, setDeletingId] = useState(null);
  const [deleteSuccessId, setDeleteSuccessId] = useState(null);

  const isEditing = useMemo(() => editingId !== null, [editingId]);

  async function loadConcerts(signal) {
    try {
      setStatus('loading');
      setError('');

      const response = await apiFetch('/api/me/concerts', {
        signal,
      });

      if (response.status === 401) {
        setError('Sign in again to view your concerts.');
        setStatus('error');
        return;
      }

      if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
      }

      const data = await response.json();
      setConcerts(data);
      setStatus('success');
    } catch (requestError) {
      if (requestError.name === 'AbortError') {
        return;
      }

      setError('Could not load your concerts from the backend.');
      setStatus('error');
    }
  }

  useEffect(() => {
    const controller = new AbortController();
    loadConcerts(controller.signal);
    return () => controller.abort();
  }, []);

  function updateForm(event) {
    setForm((current) => ({
      ...current,
      [event.target.name]: event.target.value,
    }));
  }

  function resetForm() {
    setForm(emptyForm);
    setEditingId(null);
    setFormError('');
  }

  function startEditing(concert) {
    setEditingId(concert.id);
    setForm({
      title: concert.title,
      artistName: concert.artistName,
      venueName: concert.venueName ?? '',
      city: concert.city ?? '',
      region: concert.region ?? '',
      country: concert.country ?? '',
      concertDate: toDateInputValue(concert.concertDate),
    });
    setFormError('');
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setIsSaving(true);
    setFormError('');

    try {
      const response = await apiFetch(
        isEditing
          ? `/api/me/concerts/${editingId}`
          : '/api/me/concerts',
        {
          method: isEditing ? 'PUT' : 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(toPayload(form)),
        },
      );

      if (response.status === 401) {
        setFormError('Sign in again before saving a concert.');
        return;
      }

      if (response.status === 400) {
        setFormError('Check the required fields and try again.');
        return;
      }

      if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
      }

      resetForm();
      await loadConcerts();
    } catch {
      setFormError('Could not save this concert.');
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteConcert(concertId) {
    if (!window.confirm('Are you sure you want to delete this concert?')) {
      return;
    }

    setDeletingId(concertId);
    setFormError('');
    setDeleteSuccessId(null);

    try {
      const response = await apiFetch(`/api/me/concerts/${concertId}`, {
        method: 'DELETE',
      });

      if (response.status === 401) {
        setFormError('Sign in again before deleting a concert.');
        return;
      }

      if (!response.ok && response.status !== 404) {
        throw new Error(`Backend returned ${response.status}`);
      }

      if (editingId === concertId) {
        resetForm();
      }

      setDeleteSuccessId(concertId);
      await loadConcerts();
      setTimeout(() => setDeleteSuccessId(null), 2000);
    } catch {
      setFormError('Could not delete this concert.');
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="content-section">
      <p className="eyebrow">My Concerts</p>
      <h1>Your concert list.</h1>
      <p className="lede narrow">
        Add, update, and remove concerts connected to your signed-in profile.
      </p>

      <form className="concert-form" onSubmit={handleSubmit}>
        <div className="form-header">
          <div>
            <p className="card-kicker">{isEditing ? 'Edit concert' : 'New concert'}</p>
            <h2>{isEditing ? 'Update concert details' : 'Add a concert'}</h2>
          </div>
          {isEditing ? (
            <button className="button secondary-button" onClick={resetForm} type="button">
              Cancel edit
            </button>
          ) : null}
        </div>

        <div className="form-grid">
          <label>
            Title
            <input
              name="title"
              onChange={updateForm}
              required
              type="text"
              value={form.title}
            />
          </label>
          <label>
            Artist
            <input
              name="artistName"
              onChange={updateForm}
              required
              type="text"
              value={form.artistName}
            />
          </label>
          <label>
            Date
            <input
              name="concertDate"
              onChange={updateForm}
              required
              type="date"
              value={form.concertDate}
            />
          </label>
          <label>
            Venue
            <input name="venueName" onChange={updateForm} type="text" value={form.venueName} />
          </label>
          <label>
            City
            <input name="city" onChange={updateForm} type="text" value={form.city} />
          </label>
          <label>
            Region
            <input name="region" onChange={updateForm} type="text" value={form.region} />
          </label>
          <label>
            Country
            <input name="country" onChange={updateForm} type="text" value={form.country} />
          </label>
        </div>

        {formError ? (
          <p className="form-error" role="alert" aria-live="polite">
            {formError}
          </p>
        ) : null}

        {deleteSuccessId ? (
          <p className="form-success" role="status" aria-live="polite">
            Concert deleted successfully.
          </p>
        ) : null}

        <button className="button primary-button" disabled={isSaving} type="submit">
          {isSaving ? 'Saving...' : isEditing ? 'Save changes' : 'Create concert'}
        </button>
      </form>

      {status === 'loading' ? <LoadingState message="Loading your concerts..." /> : null}

      {status === 'error' ? <ErrorState title="Concerts unavailable" message={error} /> : null}

      {status === 'success' && concerts.length === 0 ? (
        <EmptyState
          title="No concerts yet"
          message="Create your first concert with the form above."
        />
      ) : null}

      {status === 'success' && concerts.length > 0 ? (
        <div className="concert-list">
          {concerts.map((concert) => (
            <article className="data-card concert-card" key={concert.id}>
              <div>
                <p className="card-kicker">{concert.artistName}</p>
                <h2>{concert.title}</h2>
                <p className="card-detail">
                  {formatConcertDate(concert.concertDate)}
                  {concert.venueName ? ` · ${concert.venueName}` : ''}
                  {concert.city ? ` · ${concert.city}` : ''}
                </p>
              </div>
              <div className="card-actions">
                <button className="button secondary-button" onClick={() => startEditing(concert)} type="button">
                  Edit
                </button>
                <button
                  className="button danger-button"
                  disabled={deletingId === concert.id}
                  onClick={() => deleteConcert(concert.id)}
                  type="button"
                >
                  {deletingId === concert.id ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </article>
          ))}
        </div>
      ) : null}
    </section>
  );
}
