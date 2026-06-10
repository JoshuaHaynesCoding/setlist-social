export default function ErrorState({ title = 'Something went wrong', message }) {
  return (
    <section className="state-panel error-panel" role="alert">
      <h2>{title}</h2>
      <p>{message}</p>
    </section>
  );
}
