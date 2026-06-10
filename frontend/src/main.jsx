import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Link, Route, Routes } from 'react-router-dom';
import './styles.css';

function Home() {
  return (
    <main className="app-shell">
      <h1>Setlist Social</h1>
      <p>Frontend starter is ready.</p>
      <Link to="/about">About</Link>
    </main>
  );
}

function About() {
  return (
    <main className="app-shell">
      <h1>About</h1>
      <p>React Router is installed and configured.</p>
      <Link to="/">Home</Link>
    </main>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>,
);
