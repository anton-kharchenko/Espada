import { queryOptions, useQuery } from '@tanstack/react-query';
import { getAgentApprovals, getAgentOptions, getAgentSessionEvents, getAgentSessions } from '../api/agentSessionApi';

export const agentSessionKeys = {
  all: (workspaceId: string) => ['workspaces', workspaceId, 'agent-sessions'] as const,
  options: (workspaceId: string) => [...agentSessionKeys.all(workspaceId), 'options'] as const,
  events: (workspaceId: string, sessionId: string) =>
    [...agentSessionKeys.all(workspaceId), sessionId, 'events'] as const,
  approvals: (workspaceId: string, sessionId: string) =>
    [...agentSessionKeys.all(workspaceId), sessionId, 'approvals'] as const,
};

export const useAgentOptions = (workspaceId: string) =>
  useQuery(
    queryOptions({
      queryKey: agentSessionKeys.options(workspaceId),
      queryFn: ({ signal }) => getAgentOptions(workspaceId, signal),
    }),
  );

export const useAgentSessions = (workspaceId: string) =>
  useQuery(
    queryOptions({
      queryKey: agentSessionKeys.all(workspaceId),
      queryFn: ({ signal }) => getAgentSessions(workspaceId, signal),
      refetchInterval: 2_000,
    }),
  );

export const useAgentSessionEvents = (workspaceId: string, sessionId: string) =>
  useQuery(
    queryOptions({
      queryKey: agentSessionKeys.events(workspaceId, sessionId),
      queryFn: ({ signal }) => getAgentSessionEvents(workspaceId, sessionId, 0, signal),
      enabled: sessionId.length > 0,
    }),
  );

export const useAgentApprovals = (workspaceId: string, sessionId: string) =>
  useQuery(
    queryOptions({
      queryKey: agentSessionKeys.approvals(workspaceId, sessionId),
      queryFn: ({ signal }) => getAgentApprovals(workspaceId, sessionId, signal),
      enabled: sessionId.length > 0,
      refetchInterval: 1_000,
    }),
  );
