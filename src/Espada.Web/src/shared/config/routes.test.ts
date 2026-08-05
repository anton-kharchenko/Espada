import { describe, expect, it } from 'vitest';
import { getWorkspaceQueryKey, getWorkspaceRoute, workspaceSections } from './routes';

describe('workspace routes', () => {
  it('requires the workspace in route and query builders', () => {
    expect(getWorkspaceRoute('workspace/one', 'context/preview')).toBe(
      '/app/workspaces/workspace%2Fone/context/preview',
    );
    expect(getWorkspaceQueryKey('workspace-one', 'memories')).toEqual(['workspaces', 'workspace-one', 'memories']);
  });

  it('defines every workspace console destination once', () => {
    expect(workspaceSections.map(({ path }) => path)).toEqual([
      'overview',
      'projects',
      'tasks',
      'instructions',
      'policies',
      'bindings',
      'context/preview',
      'context/explain',
      'memories',
      'sources',
      'imports',
      'artifacts',
    ]);
  });
});
