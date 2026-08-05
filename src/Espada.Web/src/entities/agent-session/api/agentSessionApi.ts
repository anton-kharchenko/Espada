import { bffDelete, bffGet, bffPost, bffPostCommand } from 'shared/api';
import {
  ignoreAgentCommandResponse,
  parseAgentApprovals,
  parseAgentOptions,
  parseAgentSessionEvents,
  parseAgentSessions,
  parseStartAgentSessionsResponse,
} from '../model/agentSession';

const path = (workspaceId: string): string => `/bff/workspaces/${encodeURIComponent(workspaceId)}/agent-sessions`;

export interface StartAgentSessionsBody {
  projectId: string;
  deviceId: string;
  prompt: string;
  agentProfileIds: string[];
}

export const getAgentOptions = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${path(workspaceId)}/options`, parseAgentOptions, signal);

export const getAgentSessions = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(path(workspaceId), parseAgentSessions, signal);

export const getAgentSessionEvents = (workspaceId: string, sessionId: string, after = 0, signal?: AbortSignal) =>
  bffGet(
    `${path(workspaceId)}/${encodeURIComponent(sessionId)}/events?after=${after}`,
    parseAgentSessionEvents,
    signal,
  );

export const getAgentApprovals = (workspaceId: string, sessionId: string, signal?: AbortSignal) =>
  bffGet(`${path(workspaceId)}/${encodeURIComponent(sessionId)}/approvals`, parseAgentApprovals, signal);

export const startAgentSessions = (workspaceId: string, body: StartAgentSessionsBody, signal?: AbortSignal) =>
  bffPost(path(workspaceId), body, parseStartAgentSessionsResponse, signal);

export const decideAgentApproval = (
  workspaceId: string,
  sessionId: string,
  approvalId: string,
  approved: boolean,
  signal?: AbortSignal,
) =>
  bffPost(
    `${path(workspaceId)}/${encodeURIComponent(sessionId)}/approvals/${encodeURIComponent(approvalId)}/decision`,
    { approved },
    ignoreAgentCommandResponse,
    signal,
  );

export const cancelAgentSession = (workspaceId: string, sessionId: string, signal?: AbortSignal) =>
  bffPostCommand(`${path(workspaceId)}/${encodeURIComponent(sessionId)}/cancel`, signal);

export const applyAgentSession = (workspaceId: string, sessionId: string, signal?: AbortSignal) =>
  bffPostCommand(`${path(workspaceId)}/${encodeURIComponent(sessionId)}/apply`, signal);

export const removeAgentSessionWorktree = (workspaceId: string, sessionId: string, signal?: AbortSignal) =>
  bffDelete(`${path(workspaceId)}/${encodeURIComponent(sessionId)}/worktree`, signal);

export const getAgentSessionEventStreamUrl = (workspaceId: string, sessionId: string, after: number): string =>
  `${path(workspaceId)}/${encodeURIComponent(sessionId)}/events/stream?after=${after}`;
