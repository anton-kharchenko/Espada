import { useMemo, useState, type FormEvent } from 'react';
import {
  useAgentApprovals,
  useAgentOptions,
  useAgentSessionEvents,
  useAgentSessions,
  type AgentSessionEvent,
} from 'entities/agent-session';
import { useProjects } from 'entities/workspace';
import {
  useAgentSessionEventStream,
  useApplyAgentSession,
  useCancelAgentSession,
  useDecideAgentApproval,
  useRemoveAgentSessionWorktree,
  useStartAgentSessions,
} from 'features/agent-session';
import { useCreateArtifact } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

interface AgentSessionsPageProps {
  workspaceId: string;
  readOnly: boolean;
}

const eventText = (sessionEvent: AgentSessionEvent): string => {
  try {
    return JSON.stringify(JSON.parse(sessionEvent.payloadJson) as unknown, null, 2);
  } catch {
    return sessionEvent.payloadJson;
  }
};

export const AgentSessionsPage = ({ workspaceId, readOnly }: AgentSessionsPageProps) => {
  const options = useAgentOptions(workspaceId);
  const projects = useProjects(workspaceId);
  const sessions = useAgentSessions(workspaceId);
  const [projectId, setProjectId] = useState('');
  const [prompt, setPrompt] = useState('');
  const [selectedProfiles, setSelectedProfiles] = useState<string[]>([]);
  const [selectedSessionId, setSelectedSessionId] = useState('');
  const activeProjectId = projectId || projects.data?.items[0]?.id || '';
  const activeSessionId = selectedSessionId || sessions.data?.[0]?.sessionId || '';
  const events = useAgentSessionEvents(workspaceId, activeSessionId);
  const approvals = useAgentApprovals(workspaceId, activeSessionId);
  const start = useStartAgentSessions(workspaceId);
  const decide = useDecideAgentApproval(workspaceId, activeSessionId);
  const cancel = useCancelAgentSession(workspaceId);
  const apply = useApplyAgentSession(workspaceId);
  const removeWorktree = useRemoveAgentSessionWorktree(workspaceId);
  const createArtifact = useCreateArtifact(workspaceId, 'artifacts');
  const selectedSession = sessions.data?.find((session) => session.sessionId === activeSessionId);
  const lastSequence = events.data?.at(-1)?.sequence ?? 0;
  useAgentSessionEventStream(workspaceId, activeSessionId, lastSequence);

  const vendorByProfile = useMemo(
    () =>
      new Map(
        (options.data?.agents ?? [])
          .filter((agent) => agent.agentProfileId)
          .map((agent) => [agent.agentProfileId!, agent.vendor]),
      ),
    [options.data],
  );

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!options.data || !activeProjectId || !prompt.trim() || selectedProfiles.length === 0) return;
    try {
      const response = await start.mutateAsync({
        projectId: activeProjectId,
        deviceId: options.data.deviceId,
        prompt: prompt.trim(),
        agentProfileIds: selectedProfiles,
      });
      setPrompt('');
      setSelectedSessionId(response.sessionIds[0] ?? '');
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  };

  const toggleProfile = (profileId: string, checked: boolean) => {
    setSelectedProfiles((current) =>
      checked ? [...new Set([...current, profileId])] : current.filter((value) => value !== profileId),
    );
  };

  const saveResponse = async (sessionEvent: AgentSessionEvent) => {
    await createArtifact.mutateAsync({
      title: 'Agent response ' + (selectedSession?.branchName ?? sessionEvent.eventId),
      typeId: 2,
      kindTypeId: 1,
      content: eventText(sessionEvent),
      instructionRules: null,
      policyRules: null,
      isDraft: true,
    });
  };

  if (options.isPending || projects.isPending || sessions.isPending) {
    return (
      <PageState kind="loading" title="Loading agent runtime" description="Reading local profiles and sessions." />
    );
  }

  if (options.error || projects.error || sessions.error) {
    return (
      <ResourceQueryState
        isPending={false}
        error={options.error ?? projects.error ?? sessions.error}
        resourceName="agent sessions"
        onRetry={() => void Promise.all([options.refetch(), projects.refetch(), sessions.refetch()])}
      />
    );
  }

  return (
    <div className="agent-session-layout">
      <section className="form-panel agent-prompt-panel" aria-labelledby="agent-prompt-title">
        <p className="panel-label">PARALLEL RUN</p>
        <h2 id="agent-prompt-title">Send one prompt</h2>
        <form onSubmit={(event) => void submit(event)}>
          <label>
            Project
            <select value={activeProjectId} onChange={(event) => setProjectId(event.target.value)} disabled={readOnly}>
              {projects.data.items.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </label>
          <fieldset className="agent-options">
            <legend>Agents</legend>
            {options.data.agents.map((agent) => {
              const enabled = Boolean(agent.agentProfileId && agent.isInstalled && agent.isAuthenticated);
              return (
                <label key={agent.vendorId}>
                  <input
                    type="checkbox"
                    disabled={readOnly || !enabled}
                    checked={Boolean(agent.agentProfileId && selectedProfiles.includes(agent.agentProfileId))}
                    onChange={(event) =>
                      agent.agentProfileId && toggleProfile(agent.agentProfileId, event.target.checked)
                    }
                  />
                  <span>{agent.vendor}</span>
                  <small>{enabled ? 'Ready' : agent.isInstalled ? 'Sign in with vendor CLI' : 'Not installed'}</small>
                </label>
              );
            })}
          </fieldset>
          <label>
            Prompt
            <textarea
              value={prompt}
              onChange={(event) => setPrompt(event.target.value)}
              rows={7}
              disabled={readOnly}
              placeholder="Describe the task for the selected agents."
            />
          </label>
          <button
            className="button button-primary"
            type="submit"
            disabled={
              readOnly || start.isPending || !activeProjectId || !prompt.trim() || selectedProfiles.length === 0
            }
          >
            {start.isPending ? 'Starting…' : 'Start sessions'}
          </button>
          <MutationFeedback error={start.error} isSuccess={start.isSuccess} successMessage="Sessions started." />
        </form>
      </section>

      <section className="agent-session-dashboard" aria-labelledby="agent-sessions-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">TIMELINE</p>
            <h2 id="agent-sessions-title">Agent sessions</h2>
          </div>
          <span>{sessions.data.length}</span>
        </div>
        {sessions.data.length === 0 ? (
          <PageState compact kind="empty" title="No sessions yet" description="Select agents and send a prompt." />
        ) : (
          <>
            <div className="agent-session-tabs" role="tablist" aria-label="Agent sessions">
              {sessions.data.map((session) => (
                <button
                  role="tab"
                  aria-selected={session.sessionId === activeSessionId}
                  key={session.sessionId}
                  onClick={() => setSelectedSessionId(session.sessionId)}
                  type="button"
                >
                  <strong>{vendorByProfile.get(session.agentProfileId) ?? 'Agent'}</strong>
                  <span>{session.status}</span>
                </button>
              ))}
            </div>

            {selectedSession && (
              <div className="agent-session-actions">
                <code>{selectedSession.branchName}</code>
                <button
                  className="button"
                  type="button"
                  disabled={readOnly || !['Created', 'Running', 'WaitingForApproval'].includes(selectedSession.status)}
                  onClick={() => void cancel.mutateAsync(selectedSession.sessionId)}
                >
                  Cancel
                </button>
                <button
                  className="button button-primary"
                  type="button"
                  disabled={readOnly || selectedSession.status !== 'Completed' || apply.isPending}
                  onClick={() => void apply.mutateAsync(selectedSession.sessionId)}
                >
                  Apply diff
                </button>
                <button
                  className="button"
                  type="button"
                  disabled={
                    readOnly ||
                    !['Completed', 'Failed', 'Cancelled'].includes(selectedSession.status) ||
                    removeWorktree.isPending
                  }
                  onClick={() => {
                    if (window.confirm('Delete this session worktree and branch? The transcript stays in Espada.')) {
                      void removeWorktree.mutateAsync(selectedSession.sessionId);
                    }
                  }}
                >
                  Delete worktree
                </button>
              </div>
            )}

            {(approvals.data ?? [])
              .filter((approval) => approval.status === 'Pending')
              .map((approval) => (
                <article className="approval-card" key={approval.approvalId}>
                  <div>
                    <p className="panel-label">APPROVAL REQUIRED</p>
                    <h3>{approval.toolName}</h3>
                    <pre>{approval.argumentsJson}</pre>
                  </div>
                  <div>
                    <button
                      className="button"
                      type="button"
                      onClick={() => void decide.mutateAsync({ approvalId: approval.approvalId, approved: false })}
                    >
                      Deny
                    </button>
                    <button
                      className="button button-primary"
                      type="button"
                      onClick={() => void decide.mutateAsync({ approvalId: approval.approvalId, approved: true })}
                    >
                      Approve once
                    </button>
                  </div>
                </article>
              ))}

            <div className="agent-timeline" aria-live="polite">
              {(events.data ?? []).map((sessionEvent) => (
                <article key={sessionEvent.eventId} className={'agent-event event-' + sessionEvent.type.toLowerCase()}>
                  <header>
                    <strong>{sessionEvent.type}</strong>
                    <time dateTime={sessionEvent.occurredAtUtc}>
                      {new Date(sessionEvent.occurredAtUtc).toLocaleTimeString()}
                    </time>
                  </header>
                  <pre>{eventText(sessionEvent)}</pre>
                  {sessionEvent.type === 'AssistantOutput' && (
                    <button
                      className="button"
                      type="button"
                      disabled={readOnly || createArtifact.isPending}
                      onClick={() => void saveResponse(sessionEvent)}
                    >
                      Save as artifact draft
                    </button>
                  )}
                </article>
              ))}
            </div>
          </>
        )}
      </section>
    </div>
  );
};
