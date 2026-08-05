import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  agentSessionKeys,
  applyAgentSession,
  cancelAgentSession,
  decideAgentApproval,
  removeAgentSessionWorktree,
  startAgentSessions,
  type StartAgentSessionsBody,
} from 'entities/agent-session';

export const useStartAgentSessions = (workspaceId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: StartAgentSessionsBody) => startAgentSessions(workspaceId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: agentSessionKeys.all(workspaceId) }),
  });
};

export const useDecideAgentApproval = (workspaceId: string, sessionId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ approvalId, approved }: { approvalId: string; approved: boolean }) =>
      decideAgentApproval(workspaceId, sessionId, approvalId, approved),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: agentSessionKeys.approvals(workspaceId, sessionId) }),
        queryClient.invalidateQueries({ queryKey: agentSessionKeys.events(workspaceId, sessionId) }),
      ]),
  });
};

export const useCancelAgentSession = (workspaceId: string) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (sessionId: string) => cancelAgentSession(workspaceId, sessionId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: agentSessionKeys.all(workspaceId) }),
  });
};

export const useApplyAgentSession = (workspaceId: string) =>
  useMutation({
    mutationFn: (sessionId: string) => applyAgentSession(workspaceId, sessionId),
  });
export const useRemoveAgentSessionWorktree = (workspaceId: string) =>
  useMutation({
    mutationFn: (sessionId: string) => removeAgentSessionWorktree(workspaceId, sessionId),
  });
