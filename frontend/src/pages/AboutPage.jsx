export default function AboutPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">About</p>
      <h1>A focused social space for live music memories.</h1>
      <p className="lede narrow">
        Setlist Social is a class final project exploring how music fans might
        collect concerts, reviews, wishlists, tags, and public activity in one
        lightweight experience.
      </p>
      <div className="info-grid">
        <article className="info-block">
          <h2>Current foundation</h2>
          <p>React, Vite, React Router, ASP.NET Core Minimal API, EF Core, and SQLite local development.</p>
        </article>
        <article className="info-block">
          <h2>Planned later</h2>
          <p>Google OAuth/OIDC, Last.fm integration, SignalR activity updates, and production deployment.</p>
        </article>
      </div>
    </section>
  );
}
