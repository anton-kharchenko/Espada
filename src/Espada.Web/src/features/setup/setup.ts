import { z } from 'zod';

const instructionSchema = z.object({
  relativePath: z.string(),
  agent: z.string(),
  contentHash: z.string(),
  content: z.string(),
});

const agentSchema = z.object({
  vendorId: z.number().int().positive(),
  vendor: z.string(),
  isInstalled: z.boolean(),
  isAuthenticated: z.boolean(),
  executablePath: z.string().nullable(),
  version: z.string().nullable(),
});

const mcpConfigurationSchema = z.object({
  agent: z.string(),
  path: z.string(),
  action: z.string(),
});

export const setupPreviewSchema = z.object({
  setupId: z.string().uuid(),
  repositoryRoot: z.string(),
  workspaceName: z.string(),
  projectName: z.string(),
  canonicalRemoteUri: z.string().nullable(),
  instructions: z.array(instructionSchema),
  agents: z.array(agentSchema),
  mcpConfigurations: z.array(mcpConfigurationSchema),
  ports: z.object({ api: z.number(), mcp: z.number(), postgreSql: z.number() }),
  cloudLoginOptional: z.boolean(),
});

export const setupCommitSchema = z.object({
  workspaceId: z.string().uuid(),
  projectId: z.string().uuid(),
  repositorySourceId: z.string().uuid(),
  alreadyCompleted: z.boolean(),
  configuredAgents: z.array(z.string()),
});

export type SetupPreview = z.infer<typeof setupPreviewSchema>;
export type SetupCommit = z.infer<typeof setupCommitSchema>;

export interface SetupCommitRequest {
  setupId: string;
  repositoryPath: string;
  workspaceName: string;
  projectName: string;
  initialInstruction: string;
  agentVendorIds: number[];
  configureMcp: boolean;
  enableCloudLogin: boolean;
  apiPort: number;
  mcpPort: number;
  postgresPort: number;
}
