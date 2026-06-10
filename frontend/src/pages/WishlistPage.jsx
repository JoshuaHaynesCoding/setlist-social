import EmptyState from '../components/EmptyState.jsx';

export default function WishlistPage() {
  return (
    <section className="content-section">
      <p className="eyebrow">Wishlist</p>
      <h1>Your wishlist route is protected.</h1>
      <p className="lede narrow">
        This placeholder reserves space for signed-in wishlist features without
        adding full CRUD yet.
      </p>
      <EmptyState
        title="Wishlist management is planned"
        message="Saved artists and shows will become editable in a later task."
      />
    </section>
  );
}
