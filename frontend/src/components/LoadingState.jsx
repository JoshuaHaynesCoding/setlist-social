export default function LoadingState({ message = 'Loading...' }) {
  return (
    <section className="state-panel" aria-live="polite">
      <div className="loading-dot" aria-hidden="true" />
      <p>{message}</p>
    </section>
  );
}
