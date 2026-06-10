import Navbar from './Navbar.jsx';

export default function Layout({ children }) {
  return (
    <div className="site-shell">
      <Navbar />
      <main className="page-shell">{children}</main>
    </div>
  );
}
