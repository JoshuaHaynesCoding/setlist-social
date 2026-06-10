export default function EmptyState({ title = 'Nothing here yet', message }) {
  return (
    <section className="state-panel">
      <h2>{title}</h2>
      <p>{message}</p>
    </section>
  );
}
