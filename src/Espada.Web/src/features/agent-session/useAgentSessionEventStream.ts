import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import {
  agentSessionKeys,
  getAgentSessionEventStreamUrl,
  parseAgentSessionEvent,
  type AgentSessionEvent,
} from 'entities/agent-session';

export const useAgentSessionEventStream = (workspaceId: string, sessionId: string, lastSequence: number): void => {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!sessionId) return undefined;

    const source = new EventSource(getAgentSessionEventStreamUrl(workspaceId, sessionId, lastSequence));
    source.onmessage = (message) => {
      const sessionEvent = parseAgentSessionEvent(JSON.parse(message.data) as unknown);
      queryClient.setQueryData<AgentSessionEvent[]>(agentSessionKeys.events(workspaceId, sessionId), (current = []) =>
        current.some((item) => item.eventId === sessionEvent.eventId) ? current : [...current, sessionEvent],
      );
    };
    source.onerror = () => source.close();
    return () => source.close();
  }, [lastSequence, queryClient, sessionId, workspaceId]);
};
