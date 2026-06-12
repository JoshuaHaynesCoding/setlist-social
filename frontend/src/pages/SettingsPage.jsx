import EmptyState from '../components/EmptyState.jsx';

export default function SettingsPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">Settings</p>
      <h1>Account settings.</h1>
      <p className="lede narrow">
        Review the account area connected to your signed-in Setlist Social session.
      </p>
      <EmptyState
        title="No settings to update"
        message="Your current account is managed through Google sign-in."
      />
    </section>
  );
}
