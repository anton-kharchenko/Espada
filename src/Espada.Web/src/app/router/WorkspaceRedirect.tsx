import { Navigate, useOutletContext } from 'react-router';
import type { ConsoleSession } from 'entities/session';
import { WorkspaceStartPage } from 'pages/workspace';
import { getWorkspaceRoute } from 'shared/config';

export const WorkspaceRedirect = () => {
  const session = useOutletContext<ConsoleSession>();
  const workspace = session.workspaces[0];

  if (!workspace) {
    return <WorkspaceStartPage mode={session.mode} />;
  }

  return <Navigate to={getWorkspaceRoute(workspace.id)} replace />;
};
