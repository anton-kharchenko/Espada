import { useOutletContext } from 'react-router-dom';
import type { WorkspaceOutletContext } from 'entities/workspace';
import type { WorkspaceSection } from 'shared/config';
import { PageState } from 'shared/ui';

interface WorkspacePageProps {
  section: WorkspaceSection;
}

export const WorkspacePage = ({ section }: WorkspacePageProps) => {
  const { session, workspace } = useOutletContext<WorkspaceOutletContext>();

  if (!workspace) {
    return (
      <PageState
        kind="forbidden"
        title="Workspace unavailable"
        description="The workspace must be present in both the URL and the active console session."
      />
    );
  }

  return (
    <div className="console-page">
      <header className="console-page-header">
        <p>{workspace.name}</p>
        <h1>{section.title}</h1>
        <span>{section.description}</span>
      </header>
      {section.key === 'overview' ? (
        <section className="workspace-overview" aria-label="Workspace summary">
          <dl>
            <div>
              <dt>Workspace ID</dt>
              <dd>{workspace.id}</dd>
            </div>
            <div>
              <dt>Runtime</dt>
              <dd>{session.mode === 'local' ? 'Local' : 'Cloud'}</dd>
            </div>
            <div>
              <dt>Access</dt>
              <dd>{session.readOnly ? 'Read only' : 'Read and write'}</dd>
            </div>
          </dl>
          <PageState
            kind="empty"
            compact
            title="Workspace data is not connected yet"
            description="The console session is active. Resource views will load when their same-origin BFF contracts are available."
          />
        </section>
      ) : (
        <PageState
          kind="empty"
          title={`No ${section.title.toLowerCase()} available`}
          description="This view is ready for its same-origin BFF contract and does not fabricate workspace records."
        />
      )}
    </div>
  );
};
