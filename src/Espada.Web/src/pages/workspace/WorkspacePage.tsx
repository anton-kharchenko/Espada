import { useOutletContext } from 'react-router';
import type { WorkspaceOutletContext } from 'entities/workspace';
import type { WorkspaceSection } from 'shared/config';
import { PageState, WorkspacePageHeader } from 'shared/ui';
import { ArtifactCollectionPage } from './ArtifactCollectionPage';
import { BindingsPage } from './BindingsPage';
import { ContextPage } from './ContextPage';
import { ImportsPage } from './ImportsPage';
import { MemoriesPage } from './MemoriesPage';
import { OverviewPage } from './OverviewPage';
import { ProjectsPage } from './ProjectsPage';
import { SourcesPage } from './SourcesPage';
import { TasksPage } from './TasksPage';

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

  const content = (() => {
    switch (section.key) {
      case 'overview':
        return <OverviewPage workspaceId={workspace.id} mode={session.mode} readOnly={session.readOnly} />;
      case 'projects':
        return <ProjectsPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'tasks':
        return <TasksPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'instructions':
        return <ArtifactCollectionPage workspaceId={workspace.id} area="instructions" readOnly={session.readOnly} />;
      case 'policies':
        return <ArtifactCollectionPage workspaceId={workspace.id} area="policies" readOnly={session.readOnly} />;
      case 'bindings':
        return <BindingsPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'context-preview':
        return <ContextPage workspaceId={workspace.id} view="preview" />;
      case 'context-explain':
        return <ContextPage workspaceId={workspace.id} view="explain" />;
      case 'memories':
        return <MemoriesPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'sources':
        return <SourcesPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'imports':
        return <ImportsPage workspaceId={workspace.id} readOnly={session.readOnly} />;
      case 'artifacts':
        return <ArtifactCollectionPage workspaceId={workspace.id} area="artifacts" readOnly={session.readOnly} />;
    }
  })();

  return (
    <div className="console-page">
      <WorkspacePageHeader workspaceName={workspace.name} title={section.title} description={section.description} />
      {content}
    </div>
  );
};
