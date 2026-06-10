import EmptyState from '../components/EmptyState.jsx';

export default function ActivityPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">Activity</p>
      <h1>Public activity is planned.</h1>
      <p className="lede narrow">
        This route reserves space for community activity without adding SignalR
        or real-time features yet.
      </p>
      <EmptyState
        title="Activity feed placeholder"
        message="A public activity feed will be added after the backend activity workflow is ready."
      />
    </section>
  );
}
