import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SetupWizard } from './SetupWizard';

afterEach(() => {
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
});
