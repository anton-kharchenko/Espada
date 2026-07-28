import { Navigate, useOutletContext } from 'react-router-dom';
import type { ConsoleSession } from 'entities/session';
import { getWorkspaceRoute } from 'shared/config';
import { PageState } from 'shared/ui';

export const WorkspaceRedirect = () => {
  const session = useOutletContext<ConsoleSession>();
  const workspace = session.workspaces[0];

  if (!workspace) {
    return (
      <PageState
        kind="empty"
        title="No workspaces yet"
        description="Create a workspace with the Espada CLI, then reopen the console."
      />
    );
  }

  return <Navigate to={getWorkspaceRoute(workspace.id)} replace />;
};
