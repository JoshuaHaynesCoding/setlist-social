import { useAuth } from '../auth/AuthContext.jsx';
import EmptyState from '../components/EmptyState.jsx';

export default function ProfilePage() {
  const { profile } = useAuth();

  return (
    <section className="content-section">
      <p className="eyebrow">Profile</p>
      <h1>Your profile foundation.</h1>
      <p className="lede narrow">
        This protected page confirms the signed-in profile is available. Editing
        profile details is planned for later.
      </p>
      <article className="profile-summary">
        <p className="card-kicker">Current profile</p>
        <h2>{profile?.displayName ?? 'Signed-in user'}</h2>
        <p>{profile?.bio ?? 'No bio has been added yet.'}</p>
      </article>
      <EmptyState
        title="Profile editing is planned"
        message="This page is intentionally a placeholder until protected profile workflows are added."
      />
    </section>
  );
}
