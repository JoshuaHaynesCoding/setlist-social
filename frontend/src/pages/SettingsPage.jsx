import EmptyState from '../components/EmptyState.jsx';

export default function SettingsPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">Settings</p>
      <h1>Account settings placeholder.</h1>
      <p className="lede narrow">
        This protected route exists as a foundation for future preferences and
        account controls.
      </p>
      <EmptyState
        title="Settings are planned"
        message="Production account settings and preferences are not implemented yet."
      />
    </section>
  );
}
