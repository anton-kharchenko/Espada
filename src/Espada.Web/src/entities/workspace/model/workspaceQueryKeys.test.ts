import { describe, expect, it } from 'vitest';
import { workspaceQueryKeys } from './workspaceQueryKeys';

describe('workspaceQueryKeys', () => {
  it('scopes detail and area data by workspace', () => {
    expect(workspaceQueryKeys.detail('workspace-one')).toEqual(['workspaces', 'workspace-one', 'detail']);
    expect(workspaceQueryKeys.area('workspace-one', 'memories')).toEqual(['workspaces', 'workspace-one', 'memories']);
  });
});
