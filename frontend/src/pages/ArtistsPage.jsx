import EmptyState from '../components/EmptyState.jsx';

export default function ArtistsPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">Artists</p>
      <h1>Artist browsing is planned.</h1>
      <p className="lede narrow">
        This public route is ready for the future artist list, but it does not
        call Last.fm or show backend artist records yet.
      </p>
      <EmptyState
        title="Artist page placeholder"
        message="Artist search, profiles, and API-backed music data will be added later."
      />
    </section>
  );
}
