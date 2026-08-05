interface WorkspacePageHeaderProps {
  workspaceName: string;
  title: string;
  description: string;
}

export const WorkspacePageHeader = ({ workspaceName, title, description }: WorkspacePageHeaderProps) => (
  <header className="console-page-header">
    <p>{workspaceName}</p>
    <h1>{title}</h1>
    <span>{description}</span>
  </header>
);
