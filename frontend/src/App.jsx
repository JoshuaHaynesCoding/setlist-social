import { Outlet } from 'react-router-dom';
import Layout from './components/Layout.jsx';
import AboutPage from './pages/AboutPage.jsx';
import ActivityPage from './pages/ActivityPage.jsx';
import ArtistsPage from './pages/ArtistsPage.jsx';
import LandingPage from './pages/LandingPage.jsx';
import StatsPage from './pages/StatsPage.jsx';

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

export default App;
