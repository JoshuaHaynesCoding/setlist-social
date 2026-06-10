import EmptyState from '../components/EmptyState.jsx';

export default function MyConcertsPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">My Concerts</p>
      <h1>Your concerts area is ready for CRUD later.</h1>
      <p className="lede narrow">
        This protected route is wired into authentication, but concert create,
        edit, and delete workflows are not implemented yet.
      </p>
      <EmptyState
        title="Concert management is planned"
        message="Personal concert lists will be added in a later feature pass."
      />
    </section>
  );
}
