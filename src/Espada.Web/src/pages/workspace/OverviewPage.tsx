import { Link } from 'react-router';
import { useArtifacts, useProjects, useSources, useTasks } from 'entities/workspace';
import { getWorkspaceRoute } from 'shared/config';

interface OverviewPageProps {
  workspaceId: string;
  mode: 'local' | 'cloud';
  readOnly: boolean;
}

export const OverviewPage = ({ workspaceId, mode, readOnly }: OverviewPageProps) => {
  const projects = useProjects(workspaceId);
  const tasks = useTasks(workspaceId);
  const artifacts = useArtifacts(workspaceId, 'artifacts');
  const sources = useSources(workspaceId);
  const summaries = [
    ['Projects', projects.data?.items.length, 'projects'],
    ['Tasks', tasks.data?.items.length, 'tasks'],
    ['Artifacts', artifacts.data?.items.length, 'artifacts'],
    ['Sources', sources.data?.items.length, 'sources'],
  ] as const;

  return (
    <div className="overview-grid">
      <section className="overview-runtime">
        <p className="panel-label">RUNTIME</p>
        <h2>{mode === 'local' ? 'Local and private' : 'Managed cloud'}</h2>
        <p>
          {mode === 'local'
            ? 'Canonical context is served by your local Espada runtime and PostgreSQL database.'
            : 'Canonical context is served through the authenticated Espada Cloud BFF.'}
        </p>
        <dl className="resource-meta">
          <div>
            <dt>Workspace ID</dt>
            <dd>{workspaceId}</dd>
          </div>
          <div>
            <dt>Access</dt>
            <dd>{readOnly ? 'Read only' : 'Read and write'}</dd>
          </div>
        </dl>
      </section>

      <section className="overview-counts" aria-label="Workspace resources">
        {summaries.map(([label, count, path]) => (
          <Link key={label} to={getWorkspaceRoute(workspaceId, path)}>
            <span>{label}</span>
            <strong>{count ?? '—'}</strong>
            <small>{count === undefined ? 'Loading' : 'Open view'}</small>
          </Link>
        ))}
      </section>

      <section className="overview-context">
        <p className="panel-label">AGENT CONTEXT</p>
        <h2>One resolver, four projections</h2>
        <p>
          Build the same canonical context for Codex, Claude, Gemini, or generic MCP without generated files in the
          repository.
        </p>
        <div className="card-actions">
          <Link className="button button-primary" to={getWorkspaceRoute(workspaceId, 'context/preview')}>
            Build context
          </Link>
          <Link className="button button-secondary" to={getWorkspaceRoute(workspaceId, 'context/explain')}>
            Explain decisions
          </Link>
        </div>
      </section>
    </div>
  );
};
