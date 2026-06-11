import { Outlet } from 'react-router-dom';
import Layout from './components/Layout.jsx';
import AboutPage from './pages/AboutPage.jsx';
import ActivityPage from './pages/ActivityPage.jsx';
import ArtistsPage from './pages/ArtistsPage.jsx';
import DashboardPage from './pages/DashboardPage.jsx';
import DiscoverPage from './pages/DiscoverPage.jsx';
import LandingPage from './pages/LandingPage.jsx';
import MyConcertsPage from './pages/MyConcertsPage.jsx';
import ProfilePage from './pages/ProfilePage.jsx';
import SettingsPage from './pages/SettingsPage.jsx';
import StatsPage from './pages/StatsPage.jsx';
import WishlistPage from './pages/WishlistPage.jsx';

function App() {
  return (
    <Layout>
      <Outlet />
    </Layout>
  );
}

App.LandingPage = LandingPage;
App.AboutPage = AboutPage;
App.StatsPage = StatsPage;
App.ArtistsPage = ArtistsPage;
App.ActivityPage = ActivityPage;
App.DiscoverPage = DiscoverPage;
App.DashboardPage = DashboardPage;
App.ProfilePage = ProfilePage;
App.MyConcertsPage = MyConcertsPage;
App.WishlistPage = WishlistPage;
App.SettingsPage = SettingsPage;

export default App;
