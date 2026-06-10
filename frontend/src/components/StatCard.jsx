export default function StatCard({ label, value, detail }) {
  return (
    <article className="stat-card">
      <p className="stat-label">{label}</p>
      <p className="stat-value">{value.toLocaleString()}</p>
      {detail ? <p className="stat-detail">{detail}</p> : null}
    </article>
  );
}
