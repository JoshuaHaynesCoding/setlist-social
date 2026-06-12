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
          <h2>What it includes</h2>
          <p>React, Vite, React Router, ASP.NET Core Minimal API, EF Core, SQLite local development, public Last.fm artist search, and SignalR activity updates.</p>
        </article>
        <article className="info-block">
          <h2>How it works</h2>
          <p>Public pages show community data, while signed-in users can manage their own concerts and wishlist through protected backend APIs.</p>
        </article>
      </div>
    </section>
  );
}
