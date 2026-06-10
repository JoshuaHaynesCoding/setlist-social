import { NavLink } from 'react-router-dom';
import AuthStatus from './AuthStatus.jsx';

const navItems = [
  { to: '/', label: 'Home' },
  { to: '/about', label: 'About' },
  { to: '/stats', label: 'Stats' },
  { to: '/artists', label: 'Artists' },
  { to: '/activity', label: 'Activity' },
];

export default function Navbar() {
  return (
    <header className="site-header">
      <nav className="navbar" aria-label="Main navigation">
        <NavLink className="brand" to="/">
          Setlist Social
        </NavLink>
        <ul className="nav-links">
          {navItems.map((item) => (
            <li key={item.to}>
              <NavLink
                className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
                end={item.to === '/'}
                to={item.to}
              >
                {item.label}
              </NavLink>
            </li>
          ))}
        </ul>
        <AuthStatus />
      </nav>
    </header>
  );
}
