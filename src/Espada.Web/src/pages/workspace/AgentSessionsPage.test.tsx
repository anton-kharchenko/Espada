import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AgentSessionsPage } from './AgentSessionsPage';

const response = (body: unknown) =>
  new Response(JSON.stringify(body), { headers: { 'Content-Type': 'application/json' } });

describe('AgentSessionsPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('shows unavailable agents disabled without blocking ready agents', async () => {
    const workspaceId = '10000000-0000-1000-8000-000000000001';
    const projectId = '20000000-0000-2000-8000-000000000001';
    const deviceId = '30000000-0000-3000-8000-000000000001';
    const profileId = '40000000-0000-4000-8000-000000000001';
    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL) => {
        const path = String(input);
        if (path.endsWith('/agent-sessions/options')) {
          return Promise.resolve(
            response({
              deviceId,
              agents: [
                {
                  vendorId: 1,
                  vendor: 'Codex',
                  agentProfileId: profileId,
                  isInstalled: true,
                  isAuthenticated: true,
                },
                {
                  vendorId: 2,
                  vendor: 'Claude',
                  agentProfileId: null,
                  isInstalled: false,
                  isAuthenticated: false,
                },
              ],
            }),
          );
        }

        if (path.endsWith('/projects')) {
          return Promise.resolve(
            response({
              items: [
                {
                  id: projectId,
                  workspaceId,
                  name: 'Espada',
                  canonicalRemoteUri: null,
                  localAliases: ['C:\\Startups\\Espada'],
                  createdAtUtc: '2026-08-05T00:00:00Z',
                  updatedAtUtc: '2026-08-05T00:00:00Z',
                },
              ],
            }),
          );
        }

        return Promise.resolve(response([]));
      }),
    );

    render(
      <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
        <AgentSessionsPage readOnly={false} workspaceId={workspaceId} />
      </QueryClientProvider>,
    );

    expect(await screen.findByText('Ready')).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: /Claude/ })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Start sessions' })).toBeDisabled();
  });
});
