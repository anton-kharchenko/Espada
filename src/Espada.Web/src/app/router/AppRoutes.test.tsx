import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { MemoryRouter, useLocation } from 'react-router-dom';
import type { ConsoleSession } from 'entities/session';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AppRoutes } from './AppRoutes';

const authenticatedSession: ConsoleSession = {
  authenticated: true,
  mode: 'local',
  user: { displayName: 'Anton' },
  workspaces: [{ id: 'workspace-one', name: 'Espada' }],
  readOnly: false,
};

const LocationProbe = () => {
  const location = useLocation();
  return <output data-testid="location">{location.pathname}</output>;
};

const TestProviders = ({ path, children }: { path: string; children: ReactNode }) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        {children}
        <LocationProbe />
      </MemoryRouter>
    </QueryClientProvider>
  );
};

const renderRoutes = (path: string) =>
  render(
    <TestProviders path={path}>
      <AppRoutes />
    </TestProviders>,
  );

const respondWithSession = (session: ConsoleSession, status = 200) => {
  vi.stubGlobal(
    'fetch',
    vi.fn().mockResolvedValue(
      new Response(JSON.stringify(session), {
        status,
        headers: { 'Content-Type': 'application/json' },
      }),
    ),
  );
};

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('AppRoutes', () => {
  it('renders the pending protected state', () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() => new Promise(() => undefined)),
    );

    renderRoutes('/app');

    expect(screen.getByRole('heading', { name: 'Loading console' })).toBeInTheDocument();
  });

  it('renders the unauthenticated protected state', async () => {
    respondWithSession({
      authenticated: false,
      mode: 'local',
      user: null,
      workspaces: [],
      readOnly: false,
    });

    renderRoutes('/app');

    expect(await screen.findByRole('heading', { name: 'Console session required' })).toBeInTheDocument();
  });

  it('renders the offline protected state', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')));

    renderRoutes('/app');

    expect(await screen.findByRole('heading', { name: 'Espada is offline' })).toBeInTheDocument();
  });

  it('renders the forbidden protected state', async () => {
    respondWithSession(authenticatedSession, 403);

    renderRoutes('/app');

    expect(await screen.findByRole('heading', { name: 'Access denied' })).toBeInTheDocument();
  });

  it('renders the rate-limited protected state', async () => {
    respondWithSession(authenticatedSession, 429);

    renderRoutes('/app');

    expect(await screen.findByRole('heading', { name: 'Too many requests' })).toBeInTheDocument();
  });

  it('redirects an authenticated session to its first workspace', async () => {
    respondWithSession(authenticatedSession);

    renderRoutes('/app');

    await waitFor(() => {
      expect(screen.getByTestId('location')).toHaveTextContent('/app/workspaces/workspace-one/overview');
    });
    expect(await screen.findByRole('heading', { name: 'Overview' })).toBeInTheDocument();
  });

  it('denies a workspace that is not in the active session', async () => {
    respondWithSession(authenticatedSession);

    renderRoutes('/app/workspaces/workspace-two/overview');

    expect(await screen.findByRole('heading', { name: 'Workspace unavailable' })).toBeInTheDocument();
  });

  it('renders the read-only banner for an authenticated workspace', async () => {
    respondWithSession({ ...authenticatedSession, readOnly: true });

    renderRoutes('/app/workspaces/workspace-one/overview');

    expect(await screen.findByText(/Read-only session/)).toBeInTheDocument();
  });

  it('navigates between workspace sections without losing workspace scope', async () => {
    respondWithSession(authenticatedSession);
    renderRoutes('/app/workspaces/workspace-one/overview');
    const memoriesLink = await screen.findByRole('link', { name: 'Memories' });

    fireEvent.click(memoriesLink);

    await waitFor(() => {
      expect(screen.getByTestId('location')).toHaveTextContent('/app/workspaces/workspace-one/memories');
    });
    expect(await screen.findByRole('heading', { name: 'Memories' })).toBeInTheDocument();
  });
});
