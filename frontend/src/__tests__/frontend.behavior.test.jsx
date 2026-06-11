import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import AuthStatus from '../components/AuthStatus.jsx';
import ProtectedRoute from '../components/ProtectedRoute.jsx';
import DiscoverPage from '../pages/DiscoverPage.jsx';
import MyConcertsPage from '../pages/MyConcertsPage.jsx';
import WishlistPage from '../pages/WishlistPage.jsx';

const authMock = vi.hoisted(() => ({
  value: {
    profile: null,
    signOut: () => Promise.resolve(),
    status: 'signed-out',
  },
}));

vi.mock('../auth/AuthContext.jsx', () => ({
  useAuth: () => authMock.value,
}));

function setAuth(value) {
  authMock.value = {
    profile: null,
    signOut: vi.fn(() => Promise.resolve()),
    status: 'signed-out',
    ...value,
  };
}

function mockJsonResponse(body, init = {}) {
  return {
    ok: init.ok ?? true,
    status: init.status ?? 200,
    json: vi.fn(() => Promise.resolve(body)),
  };
}

function renderProtectedRoute() {
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route element={<ProtectedRoute />}>
          <Route path="/protected" element={<p>Protected child content</p>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    setAuth({ status: 'signed-out' });
  });

  it('shows sign-in required when unauthenticated', () => {
    renderProtectedRoute();

    expect(screen.getByText('Sign in required')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /sign in with google/i })).toHaveAttribute(
      'href',
      'https://setlist-social.onrender.com/api/auth/login',
    );
  });

  it('renders children when authenticated', () => {
    setAuth({ status: 'signed-in', profile: { displayName: '@crateghost' } });

    renderProtectedRoute();

    expect(screen.getByText('Protected child content')).toBeInTheDocument();
  });
});

describe('AuthStatus', () => {
  it('shows signed-out sign-in state', () => {
    setAuth({ status: 'signed-out' });

    render(<AuthStatus />);

    expect(screen.getByRole('link', { name: /sign in with google/i })).toHaveAttribute(
      'href',
      'https://setlist-social.onrender.com/api/auth/login',
    );
  });

  it('shows signed-in display name', () => {
    setAuth({ status: 'signed-in', profile: { displayName: '@sadefiles' } });

    render(<AuthStatus />);

    expect(screen.getByText('@sadefiles')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign out/i })).toBeInTheDocument();
  });
});

describe('DiscoverPage', () => {
  beforeEach(() => {
    setAuth({ status: 'signed-out' });
  });

  it('validates empty artist search', async () => {
    const user = userEvent.setup();
    vi.stubGlobal('fetch', vi.fn());

    render(<DiscoverPage />);
    await user.click(screen.getByRole('button', { name: /search/i }));

    expect(screen.getByText('Enter an artist name to search.')).toBeInTheDocument();
    expect(fetch).not.toHaveBeenCalled();
  });

  it('shows Last.fm results from mocked API', async () => {
    const user = userEvent.setup();
    vi.stubGlobal(
      'fetch',
      vi.fn(() =>
        Promise.resolve(
          mockJsonResponse([
            {
              name: 'Sade',
              url: 'https://www.last.fm/music/Sade',
              listeners: 123456,
              imageUrl: null,
            },
          ]),
        ),
      ),
    );

    render(<DiscoverPage />);
    await user.type(screen.getByLabelText(/artist name/i), 'Sade');
    await user.click(screen.getByRole('button', { name: /search/i }));

    expect(await screen.findByRole('heading', { name: 'Sade' })).toBeInTheDocument();
    expect(screen.getByText('123,456 listeners')).toBeInTheDocument();
    expect(screen.getByText('Sign in to save artists.')).toBeInTheDocument();
  });

  it('shows an error state when Last.fm search fails', async () => {
    const user = userEvent.setup();
    vi.stubGlobal(
      'fetch',
      vi.fn(() => Promise.resolve(mockJsonResponse({}, { ok: false, status: 500 }))),
    );

    render(<DiscoverPage />);
    await user.type(screen.getByLabelText(/artist name/i), 'Sade');
    await user.click(screen.getByRole('button', { name: /search/i }));

    expect(await screen.findByText('Could not search Last.fm right now.')).toBeInTheDocument();
  });
});

describe('WishlistPage', () => {
  it('shows empty state when no wishlist items exist', async () => {
    vi.stubGlobal('fetch', vi.fn(() => Promise.resolve(mockJsonResponse([]))));

    render(<WishlistPage />);

    expect(await screen.findByText('No wishlist artists yet')).toBeInTheDocument();
  });

  it('does not delete when confirmation is canceled', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(
        mockJsonResponse([
          {
            id: 7,
            artistName: 'Kendrick Lamar',
            sourceUrl: 'https://www.last.fm/music/Kendrick+Lamar',
            sourceName: 'Last.fm',
            createdAt: '2026-06-11T00:00:00Z',
            updatedAt: '2026-06-11T00:00:00Z',
          },
        ]),
      );
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
    const user = userEvent.setup();
    vi.stubGlobal('fetch', fetchMock);

    render(<WishlistPage />);

    const card = await screen.findByRole('heading', { name: 'Kendrick Lamar' });
    await user.click(within(card.closest('article')).getByRole('button', { name: /delete/i }));

    expect(confirmSpy).toHaveBeenCalledWith(
      'Are you sure you want to remove this artist from your wishlist?',
    );
    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(fetchMock).not.toHaveBeenCalledWith(
      expect.stringContaining('/api/me/wishlist/7'),
      expect.objectContaining({ method: 'DELETE' }),
    );
  });
});

describe('MyConcertsPage', () => {
  it('uses required form fields to prevent empty submit', async () => {
    const user = userEvent.setup();
    const fetchMock = vi.fn(() => Promise.resolve(mockJsonResponse([])));
    vi.stubGlobal('fetch', fetchMock);

    render(<MyConcertsPage />);
    await screen.findByText('No concerts yet');
    await user.click(screen.getByRole('button', { name: /create concert/i }));

    expect(screen.getByLabelText(/title/i)).toBeInvalid();
    expect(screen.getByLabelText(/artist/i)).toBeInvalid();
    expect(screen.getByLabelText(/date/i)).toBeInvalid();
    expect(fetchMock).toHaveBeenCalledTimes(1);

    await waitFor(() => {
      expect(fetchMock).not.toHaveBeenCalledWith(
        expect.stringContaining('/api/me/concerts'),
        expect.objectContaining({ method: 'POST' }),
      );
    });
  });
});
