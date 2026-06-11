import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import App from './App.jsx';
import { AuthProvider } from './auth/AuthContext.jsx';
import ProtectedRoute from './components/ProtectedRoute.jsx';
import './styles.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<App />}>
            <Route index element={<App.LandingPage />} />
            <Route path="about" element={<App.AboutPage />} />
            <Route path="stats" element={<App.StatsPage />} />
            <Route path="artists" element={<App.ArtistsPage />} />
            <Route path="activity" element={<App.ActivityPage />} />
            <Route path="discover" element={<App.DiscoverPage />} />
            <Route element={<ProtectedRoute />}>
              <Route path="dashboard" element={<App.DashboardPage />} />
              <Route path="profile" element={<App.ProfilePage />} />
              <Route path="my-concerts" element={<App.MyConcertsPage />} />
              <Route path="wishlist" element={<App.WishlistPage />} />
              <Route path="settings" element={<App.SettingsPage />} />
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  </React.StrictMode>,
);
