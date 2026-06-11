import { NavLink } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext.jsx';
import AuthStatus from './AuthStatus.jsx';

const navItems = [
  { to: '/', label: 'Home' },
  { to: '/about', label: 'About' },
  { to: '/stats', label: 'Stats' },
  { to: '/artists', label: 'Artists' },
  { to: '/activity', label: 'Activity' },
  { to: '/discover', label: 'Discover' },
];

const protectedNavItems = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/profile', label: 'Profile' },
  { to: '/my-concerts', label: 'My Concerts' },
  { to: '/wishlist', label: 'Wishlist' },
  { to: '/settings', label: 'Settings' },
];

export default function Navbar() {
  const { status } = useAuth();
  const visibleNavItems =
    status === 'signed-in' ? [...navItems, ...protectedNavItems] : navItems;

  return (
    <header className="site-header">
      <nav className="navbar" aria-label="Main navigation">
        <NavLink className="brand" to="/">
          Setlist Social
        </NavLink>
        <ul className="nav-links">
          {visibleNavItems.map((item) => (
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
