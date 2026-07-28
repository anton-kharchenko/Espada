import { getWorkspaceQueryKey } from 'shared/config';

export const workspaceQueryKeys = {
  detail: (workspaceId: string) => getWorkspaceQueryKey(workspaceId, 'detail'),
  area: (workspaceId: string, resource: string) => getWorkspaceQueryKey(workspaceId, resource),
};
