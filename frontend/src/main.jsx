import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import App from './App.jsx';
import './styles.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<App />}>
          <Route index element={<App.LandingPage />} />
          <Route path="about" element={<App.AboutPage />} />
          <Route path="stats" element={<App.StatsPage />} />
          <Route path="artists" element={<App.ArtistsPage />} />
          <Route path="activity" element={<App.ActivityPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </React.StrictMode>,
);
