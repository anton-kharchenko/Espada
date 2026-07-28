import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { TasksPage } from './TasksPage';

const response = (body: unknown) =>
  new Response(JSON.stringify(body), { headers: { 'Content-Type': 'application/json' } });

describe('TasksPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('keeps Archive available after a task is completed', async () => {
    const fetchMock = vi.fn((input: RequestInfo | URL) => {
      const path = String(input);

      if (path.endsWith('/projects')) {
        return Promise.resolve(
          response({
            items: [
              {
                id: 'project-one',
                workspaceId: 'workspace-one',
                name: 'Espada',
                canonicalRemoteUri: 'https://github.com/antonkharchenko/Espada.git',
                localAliases: [],
                createdAtUtc: '2026-07-28T00:00:00Z',
                updatedAtUtc: '2026-07-28T00:00:00Z',
              },
            ],
          }),
        );
      }

      return Promise.resolve(
        response({
          items: [
            {
              id: 'task-one',
              workspaceId: 'workspace-one',
              projectId: 'project-one',
              title: 'Completed task',
              statusTypeId: 2,
              statusTypeName: 'completed',
              createdAtUtc: '2026-07-28T00:00:00Z',
              updatedAtUtc: '2026-07-28T00:01:00Z',
              completedAtUtc: '2026-07-28T00:01:00Z',
              archivedAtUtc: null,
            },
          ],
        }),
      );
    });
    vi.stubGlobal('fetch', fetchMock);

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <TasksPage readOnly={false} workspaceId="workspace-one" />
      </QueryClientProvider>,
    );

    expect(await screen.findByRole('button', { name: 'Archive' })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Complete' })).not.toBeInTheDocument();
  });
});
