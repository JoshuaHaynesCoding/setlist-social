import { Link } from 'react-router-dom';

export default function LandingPage() {
  return (
    <section className="hero-grid">
      <div className="hero-copy">
        <p className="eyebrow">Music & Events</p>
        <h1>Track the shows people are still talking about.</h1>
        <p className="lede">
          Setlist Social is a public preview for a concert-centered community app.
          Browse the early project shape, check live local stats, and follow along
          as artist, activity, and review features come online.
        </p>
        <div className="hero-actions">
          <Link className="button primary-button" to="/stats">
            View stats
          </Link>
          <Link className="button secondary-button" to="/about">
            Learn more
          </Link>
        </div>
      </div>
      <aside className="hero-panel" aria-label="Project highlights">
        <div>
          <span className="panel-kicker">Now live</span>
          <strong>Public stats</strong>
          <p>Reads real counts from the ASP.NET Core API.</p>
        </div>
        <div>
          <span className="panel-kicker">Planned</span>
          <strong>Artists and activity</strong>
          <p>Placeholders are ready while the backend grows.</p>
        </div>
      </aside>
    </section>
  );
}
