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
          <p>React, Vite, React Router, ASP.NET Core Minimal API, EF Core, SQLite local development, public Last.fm artist search, and SignalR activity updates.</p>
        </article>
        <article className="info-block">
          <h2>Planned later</h2>
          <p>Deeper Last.fm features, broader protected workflows, and production deployment.</p>
        </article>
      </div>
    </section>
  );
}
