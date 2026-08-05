import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { consoleSessionQueryOptions } from 'entities/session';
import { SetupWizard } from './SetupWizard';

afterEach(() => {
  document.cookie = 'Espada.Console.Csrf=; Max-Age=0';
  vi.unstubAllGlobals();
});

describe('SetupWizard', () => {
  it('shows the reviewed repository, instruction, agent, and ports before commit', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            setupId: '11111111-1111-4111-8111-111111111111',
            repositoryRoot: 'C:\\\\code\\\\espada',
            workspaceName: 'Espada',
            projectName: 'Espada',
            canonicalRemoteUri: 'https://example.test/espada.git',
            instructions: [
              {
                relativePath: 'AGENTS.md',
                agent: 'codex',
                contentHash: 'abcdef1234567890',
                content: 'Follow repository style.',
              },
            ],
            agents: [
              {
                vendorId: 1,
                vendor: 'Codex',
                isInstalled: true,
                isAuthenticated: true,
                executablePath: 'codex.exe',
                version: 'codex 1.0',
              },
            ],
            mcpConfigurations: [{ agent: 'Codex', path: 'config.toml', action: 'Update' }],
            ports: { api: 7432, mcp: 7433, postgreSql: 5433 },
            cloudLoginOptional: true,
          }),
          { status: 200, headers: { 'Content-Type': 'application/json' } },
        ),
      ),
    );
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/setup?path=C%3A%5Ccode%5Cespada']}>
          <SetupWizard />
        </MemoryRouter>
      </QueryClientProvider>,
    );

    expect(await screen.findByRole('heading', { name: 'Configure Espada' })).toBeInTheDocument();
    expect(screen.getByText('AGENTS.md')).toBeInTheDocument();
    expect(screen.getByText('codex 1.0')).toBeInTheDocument();
    expect(screen.getByDisplayValue('7432')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Confirm and configure' })).toBeEnabled();
  });
  it('invalidates the console session after setup creates a workspace', async () => {
    const fetchMock = vi.fn().mockImplementation((input: RequestInfo | URL) => {
      const url = input.toString();
      const body = url.includes('/commit')
        ? {
            workspaceId: '22222222-2222-4222-8222-222222222222',
            projectId: '33333333-3333-4333-8333-333333333333',
            repositorySourceId: '44444444-4444-4444-8444-444444444444',
            alreadyCompleted: false,
            configuredAgents: [],
          }
        : {
            setupId: '11111111-1111-4111-8111-111111111111',
            repositoryRoot: 'C:\\code\\espada',
            workspaceName: 'Espada',
            projectName: 'Espada',
            canonicalRemoteUri: null,
            instructions: [],
            agents: [],
            mcpConfigurations: [],
            ports: { api: 7432, mcp: 7433, postgreSql: 5433 },
            cloudLoginOptional: true,
          };
      return Promise.resolve(
        new Response(JSON.stringify(body), { status: 200, headers: { 'Content-Type': 'application/json' } }),
      );
    });
    vi.stubGlobal('fetch', fetchMock);
    document.cookie = 'Espada.Console.Csrf=test';
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    client.setQueryData(consoleSessionQueryOptions.queryKey, {
      authenticated: true,
      mode: 'local',
      user: { displayName: 'Local user', email: null },
      workspaces: [],
      readOnly: false,
    });

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/setup?path=C%3A%5Ccode%5Cespada']}>
          <SetupWizard />
        </MemoryRouter>
      </QueryClientProvider>,
    );

    fireEvent.click(await screen.findByRole('button', { name: 'Confirm and configure' }));

    await waitFor(() => expect(client.getQueryState(consoleSessionQueryOptions.queryKey)?.isInvalidated).toBe(true));
  });
});
