import { useAuth } from '../auth/AuthContext.jsx';
import EmptyState from '../components/EmptyState.jsx';

export default function ProfilePage() {
  const { profile } = useAuth();

  return (
    <section className="content-section">
      <p className="eyebrow">Profile</p>
      <h1>Your profile.</h1>
      <p className="lede narrow">
        View the profile attached to your Setlist Social session.
      </p>
      <article className="profile-summary">
        <p className="card-kicker">Current profile</p>
        <h2>{profile?.displayName ?? 'Signed-in user'}</h2>
        <p>{profile?.bio ?? 'No bio has been added yet.'}</p>
      </article>
      <EmptyState
        title="Profile details"
        message="Your display name and bio appear here when they are available."
      />
    </section>
  );
}
