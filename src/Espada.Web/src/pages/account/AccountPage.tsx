import { Link, useNavigate, useOutletContext } from 'react-router';
import type { WorkspaceOutletContext } from 'entities/workspace';
import { useLogoutConsole } from 'features/workspace';
import { ROUTES } from 'shared/config';
import { MutationFeedback } from 'shared/ui';

interface AccountPageProps {
  title: 'Billing' | 'Settings';
}

export const AccountPage = ({ title }: AccountPageProps) => {
  const { session } = useOutletContext<WorkspaceOutletContext>();
  const navigate = useNavigate();
  const logout = useLogoutConsole();

  const logoutSession = async () => {
    try {
      await logout.mutateAsync();
      navigate(ROUTES.home);
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  };

  return (
    <div className="console-page">
      <header className="console-page-header">
        <p>{session.mode === 'local' ? 'Local runtime' : 'Espada Cloud'}</p>
        <h1>{title}</h1>
        <span>
          {title === 'Billing'
            ? 'Plan, entitlement, and usage information.'
            : 'Console session and runtime preferences.'}
        </span>
      </header>
      {title === 'Billing' ? (
        <div className="account-grid">
          <section className="account-card account-card-primary">
            <p className="panel-label">{session.mode === 'local' ? 'COMMUNITY · LOCAL' : 'TEAM · MANAGED'}</p>
            <h2>{session.mode === 'local' ? 'Free' : 'Paid collaboration'}</h2>
            <p>
              {session.mode === 'local'
                ? 'Your local runtime, database, files, and MCP connections remain free and under your control.'
                : 'Managed team workspaces use commercial entitlements and workspace-scoped limits.'}
            </p>
            <Link className="button button-secondary" to={ROUTES.pricing}>
              View product options
            </Link>
          </section>
          <section className="account-card">
            <p className="panel-label">BOUNDARY</p>
            <h2>Pay for managed sharing, not local ownership.</h2>
            <p>
              Espada Cloud covers shared infrastructure, synchronization, access control, and team operations. It is
              optional for local use.
            </p>
          </section>
        </div>
      ) : (
        <div className="account-grid">
          <section className="account-card">
            <p className="panel-label">SESSION</p>
            <h2>{session.user?.displayName ?? 'Local session'}</h2>
            <dl className="resource-meta">
              <div>
                <dt>Runtime</dt>
                <dd>{session.mode === 'local' ? 'Local' : 'Espada Cloud'}</dd>
              </div>
              <div>
                <dt>Workspaces</dt>
                <dd>{session.workspaces.length}</dd>
              </div>
              <div>
                <dt>Access</dt>
                <dd>{session.readOnly ? 'Read only' : 'Read and write'}</dd>
              </div>
            </dl>
          </section>
          <section className="account-card account-card-danger">
            <p className="panel-label">BROWSER SESSION</p>
            <h2>Sign out this browser</h2>
            <p>
              The browser never receives MCP access or refresh tokens. Signing out removes only the HttpOnly BFF
              session.
            </p>
            <button
              className="button button-danger"
              disabled={logout.isPending}
              onClick={() => void logoutSession()}
              type="button"
            >
              {logout.isPending ? 'Signing out…' : 'Sign out'}
            </button>
            <MutationFeedback error={logout.error} />
          </section>
        </div>
      )}
    </div>
  );
};
