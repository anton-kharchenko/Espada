import { useMutation, useQuery } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { getWorkspaceRoute } from 'shared/config';
import { PageState } from 'shared/ui';
import { commitSetup, getSetupPreview } from './setupApi';
import type { SetupPreview } from './setup';

export const SetupWizard = () => {
  const [searchParams] = useSearchParams();
  const repositoryPath = searchParams.get('path') ?? '';
  const preview = useQuery({
    queryKey: ['local-setup', repositoryPath],
    queryFn: ({ signal }) => getSetupPreview(repositoryPath, signal),
    enabled: repositoryPath.length > 0,
  });

  if (!repositoryPath) {
    return <PageState kind="empty" title="Repository required" description="Run espada init from a Git repository." />;
  }
  if (preview.isPending) {
    return (
      <PageState
        kind="loading"
        title="Inspecting repository"
        description="Reading tracked instructions and local agent installations."
      />
    );
  }
  if (preview.isError) {
    return <PageState kind="unavailable" title="Setup preview failed" description={preview.error.message} />;
  }

  return <SetupForm preview={preview.data} />;
};

const SetupForm = ({ preview }: { preview: SetupPreview }) => {
  const navigate = useNavigate();
  const [workspaceName, setWorkspaceName] = useState(preview.workspaceName);
  const [projectName, setProjectName] = useState(preview.projectName);
  const [initialInstruction, setInitialInstruction] = useState(
    'Use Espada as the canonical context runtime for this workspace.',
  );
  const [agentVendorIds, setAgentVendorIds] = useState<number[]>(
    preview.agents.filter((agent) => agent.isInstalled && agent.isAuthenticated).map((agent) => agent.vendorId),
  );
  const [configureMcp, setConfigureMcp] = useState(true);
  const [enableCloudLogin, setEnableCloudLogin] = useState(false);
  const [apiPort, setApiPort] = useState(preview.ports.api);
  const [mcpPort, setMcpPort] = useState(preview.ports.mcp);
  const [postgresPort, setPostgresPort] = useState(preview.ports.postgreSql);
  const commit = useMutation({
    mutationFn: commitSetup,
    onSuccess: (result) => navigate(getWorkspaceRoute(result.workspaceId), { replace: true }),
  });

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    commit.mutate({
      setupId: preview.setupId,
      repositoryPath: preview.repositoryRoot,
      workspaceName,
      projectName,
      initialInstruction,
      agentVendorIds,
      configureMcp,
      enableCloudLogin,
      apiPort,
      mcpPort,
      postgresPort,
    });
  };

  const toggleAgent = (vendorId: number, selected: boolean) => {
    setAgentVendorIds((current) => (selected ? [...current, vendorId] : current.filter((id) => id !== vendorId)));
  };

  return (
    <form className="setup-wizard" onSubmit={submit}>
      <header>
        <p>Local setup</p>
        <h1>Configure Espada</h1>
        <span>Review everything below. Nothing is created until you confirm.</span>
      </header>

      <section>
        <h2>Workspace and project</h2>
        <label>
          Workspace name
          <input value={workspaceName} onChange={(event) => setWorkspaceName(event.target.value)} required />
        </label>
        <label>
          Project name
          <input value={projectName} onChange={(event) => setProjectName(event.target.value)} required />
        </label>
        <dl>
          <div>
            <dt>Repository</dt>
            <dd>{preview.repositoryRoot}</dd>
          </div>
          <div>
            <dt>Remote</dt>
            <dd>{preview.canonicalRemoteUri ?? 'Local only'}</dd>
          </div>
        </dl>
      </section>

      <section>
        <h2>Initial instruction</h2>
        <textarea
          value={initialInstruction}
          onChange={(event) => setInitialInstruction(event.target.value)}
          rows={5}
          required
        />
        <h3>Tracked vendor instructions</h3>
        {preview.instructions.length === 0 ? (
          <p className="setup-muted">No tracked AGENTS.md, CLAUDE.md, or GEMINI.md files found.</p>
        ) : (
          <ul className="setup-list">
            {preview.instructions.map((instruction) => (
              <li key={instruction.relativePath}>
                <strong>{instruction.relativePath}</strong>
                <span>
                  {instruction.agent} · {instruction.contentHash.slice(0, 12)}
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section>
        <h2>Agents and MCP</h2>
        <ul className="setup-list">
          {preview.agents.map((agent) => {
            const enabled = agent.isInstalled && agent.isAuthenticated;
            return (
              <li key={agent.vendorId}>
                <label className="setup-check">
                  <input
                    type="checkbox"
                    disabled={!enabled}
                    checked={agentVendorIds.includes(agent.vendorId)}
                    onChange={(event) => toggleAgent(agent.vendorId, event.target.checked)}
                  />
                  <strong>{agent.vendor}</strong>
                </label>
                <span>
                  {!agent.isInstalled
                    ? 'Not installed'
                    : !agent.isAuthenticated
                      ? 'Authentication required'
                      : (agent.version ?? 'Detected')}
                </span>
              </li>
            );
          })}
        </ul>
        <label className="setup-check">
          <input type="checkbox" checked={configureMcp} onChange={(event) => setConfigureMcp(event.target.checked)} />
          Update managed MCP entries with backup
        </label>
        {configureMcp && (
          <ul className="setup-list compact">
            {preview.mcpConfigurations.map((item) => (
              <li key={item.agent}>
                <strong>{item.agent}</strong>
                <span>{item.path}</span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section>
        <h2>Local ports</h2>
        <div className="setup-port-grid">
          <label>
            API
            <input
              type="number"
              min="1"
              max="65535"
              value={apiPort}
              onChange={(event) => setApiPort(event.target.valueAsNumber)}
              required
            />
          </label>
          <label>
            MCP
            <input
              type="number"
              min="1"
              max="65535"
              value={mcpPort}
              onChange={(event) => setMcpPort(event.target.valueAsNumber)}
              required
            />
          </label>
          <label>
            PostgreSQL
            <input
              type="number"
              min="1"
              max="65535"
              value={postgresPort}
              onChange={(event) => setPostgresPort(event.target.valueAsNumber)}
              required
            />
          </label>
        </div>
        <p className="setup-muted">Port changes apply on the next runtime start.</p>
      </section>

      <section>
        <h2>Optional cloud</h2>
        <label className="setup-check">
          <input
            type="checkbox"
            checked={enableCloudLogin}
            onChange={(event) => setEnableCloudLogin(event.target.checked)}
          />
          Sign in to Espada Cloud after local setup
        </label>
      </section>

      {commit.isError && (
        <p className="setup-error" role="alert">
          {commit.error.message}
        </p>
      )}
      <button type="submit" disabled={commit.isPending}>
        {commit.isPending ? 'Configuring…' : 'Confirm and configure'}
      </button>
    </form>
  );
};
