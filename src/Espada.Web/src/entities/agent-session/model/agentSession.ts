import { z } from 'zod';

const agentOptionSchema = z.object({
  vendorId: z.number().int().positive(),
  vendor: z.string().min(1),
  agentProfileId: z.string().uuid().nullable(),
  isInstalled: z.boolean(),
  isAuthenticated: z.boolean(),
});

const agentOptionsSchema = z.object({
  deviceId: z.string().uuid(),
  agents: z.array(agentOptionSchema),
});

const agentSessionSchema = z.object({
  sessionId: z.string().uuid(),
  projectId: z.string().uuid(),
  agentProfileId: z.string().uuid(),
  prompt: z.string(),
  branchName: z.string(),
  status: z.string(),
  createdAtUtc: z.string(),
  finishedAtUtc: z.string().nullable(),
});

const agentSessionEventSchema = z.object({
  eventId: z.string().uuid(),
  sequence: z.number().int().positive(),
  type: z.string(),
  payloadJson: z.string(),
  occurredAtUtc: z.string(),
});

const agentApprovalSchema = z.object({
  approvalId: z.string().uuid(),
  sessionId: z.string().uuid(),
  toolName: z.string(),
  argumentsJson: z.string(),
  status: z.string(),
  requestedAtUtc: z.string(),
  decidedAtUtc: z.string().nullable(),
});

const startAgentSessionsResponseSchema = z.object({
  sessionIds: z.array(z.string().uuid()),
});

export type AgentOption = z.infer<typeof agentOptionSchema>;
export type AgentOptions = z.infer<typeof agentOptionsSchema>;
export type AgentSession = z.infer<typeof agentSessionSchema>;
export type AgentSessionEvent = z.infer<typeof agentSessionEventSchema>;
export type AgentApproval = z.infer<typeof agentApprovalSchema>;

export const parseAgentOptions = (value: unknown): AgentOptions => agentOptionsSchema.parse(value);
export const parseAgentSessions = (value: unknown): AgentSession[] => z.array(agentSessionSchema).parse(value);
export const parseAgentSessionEvents = (value: unknown): AgentSessionEvent[] =>
  z.array(agentSessionEventSchema).parse(value);
export const parseAgentSessionEvent = (value: unknown): AgentSessionEvent => agentSessionEventSchema.parse(value);
export const parseAgentApprovals = (value: unknown): AgentApproval[] => z.array(agentApprovalSchema).parse(value);
export const parseStartAgentSessionsResponse = (value: unknown) => startAgentSessionsResponseSchema.parse(value);
export const ignoreAgentCommandResponse = (): void => undefined;
