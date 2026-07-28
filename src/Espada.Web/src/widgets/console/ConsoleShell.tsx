import type { ChangeEvent } from 'react';
import { NavLink, Outlet, useLocation, useNavigate, useOutletContext, useParams } from 'react-router-dom';
import type { ConsoleSession } from 'entities/session';
import type { WorkspaceOutletContext } from 'entities/workspace';
import { ROUTES, getWorkspaceRoute, workspaceSections } from 'shared/config';
import { LogoMark } from 'shared/ui';
import { PageState } from 'shared/ui';

const groups = ['Workspace', 'Context', 'Knowledge'] as const;

const Brand = () => (
  <a className="console-brand" href="/" aria-label="Espada home">
    <LogoMark />
    <span>Espada</span>
  </a>
);

export const ConsoleShell = () => {
  const session = useOutletContext<ConsoleSession>();
  const { workspaceId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const workspace = workspaceId
    ? (session.workspaces.find(({ id }) => id === workspaceId) ?? null)
    : (session.workspaces[0] ?? null);

  if (session.workspaces.length === 0) {
    return (
      <PageState
        kind="empty"
        title="No workspaces yet"
        description="Create a workspace with the Espada CLI, then reopen the console."
      />
    );
  }

  if (workspaceId && !workspace) {
    return (
      <PageState
        kind="forbidden"
        title="Workspace unavailable"
        description="This console session cannot access the workspace in the URL."
      />
    );
  }

  const changeWorkspace = (event: ChangeEvent<HTMLSelectElement>) => {
    navigate(getWorkspaceRoute(event.target.value));
  };

  const workspaceContext: WorkspaceOutletContext = { session, workspace };
  const userLabel = session.user?.displayName ?? (session.mode === 'local' ? 'Local session' : 'Cloud session');

  return (
    <div className="console-shell">
      <a className="skip-link" href="#console-content">
        Skip to console content
      </a>
      <aside className="console-sidebar">
        <Brand />
        <div className="workspace-switcher">
          <label htmlFor="workspace-switcher">Workspace</label>
          <select id="workspace-switcher" value={workspace?.id ?? ''} onChange={changeWorkspace}>
            {session.workspaces.map((item) => (
              <option key={item.id} value={item.id}>
                {item.name}
              </option>
            ))}
          </select>
        </div>
        <nav className="console-nav" aria-label="Workspace navigation">
          {groups.map((group) => (
            <div className="console-nav-group" key={group}>
              <p>{group}</p>
              {workspace &&
                workspaceSections
                  .filter((section) => section.group === group)
                  .map((section) => (
                    <NavLink key={section.key} to={getWorkspaceRoute(workspace.id, section.path)}>
                      {section.title}
                    </NavLink>
                  ))}
            </div>
          ))}
        </nav>
        <nav className="console-nav console-nav-secondary" aria-label="Account navigation">
          <NavLink to={ROUTES.billing}>Billing</NavLink>
          <NavLink to={ROUTES.settings}>Settings</NavLink>
        </nav>
      </aside>
      <div className="console-main">
        <header className="console-header">
          <div>
            <span className={`runtime-indicator runtime-${session.mode}`} aria-hidden="true" />
            {session.mode === 'local' ? 'Local runtime' : 'Espada Cloud'}
          </div>
          <span>{userLabel}</span>
        </header>
        {session.readOnly && (
          <div className="read-only-banner" role="status">
            Read-only session. Viewing is available; changes are disabled.
          </div>
        )}
        <main id="console-content" className="console-content" key={location.pathname}>
          <Outlet context={workspaceContext} />
        </main>
      </div>
    </div>
  );
};
