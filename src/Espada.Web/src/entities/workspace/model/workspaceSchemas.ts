import { z } from 'zod';
import type {
  ConsoleContextBuildResponse,
  CreateWorkspaceResponse,
  GetArtifactByIdResponse,
  ListArtifactRevisionsResponse,
  ListArtifactsResponse,
  ListBindingsResponse,
  ListImportsResponse,
  ListProjectsResponse,
  ListSourcesResponse,
  ListWorkspaceTasksResponse,
  SearchMemoryResponse,
} from 'shared/api/generated';

const numericSchema = z.union([z.number(), z.string()]);
const nullableStringSchema = z.string().nullable();

const projectSchema = z.object({
  id: z.string(),
  workspaceId: z.string(),
  name: z.string(),
  canonicalRemoteUri: z.string().nullable(),
  localAliases: z.array(z.string()),
  createdAtUtc: z.string(),
  updatedAtUtc: z.string(),
});

const taskSchema = z.object({
  id: z.string(),
  workspaceId: z.string(),
  projectId: z.string(),
  title: z.string(),
  statusTypeId: numericSchema,
  statusTypeName: z.string(),
  createdAtUtc: z.string(),
  updatedAtUtc: z.string(),
  completedAtUtc: nullableStringSchema,
  archivedAtUtc: nullableStringSchema,
});

const artifactSchema = z.object({
  id: z.string(),
  title: z.string(),
  kindTypeId: numericSchema,
  kindTypeName: z.string(),
  typeId: numericSchema,
  typeName: z.string(),
  statusId: numericSchema,
  statusName: z.string(),
  priority: numericSchema,
  currentRevisionId: nullableStringSchema,
  currentRevisionNumber: numericSchema.nullable(),
  revisionCount: numericSchema,
  createdAtUtc: z.string(),
  updatedAtUtc: z.string(),
  archivedAtUtc: nullableStringSchema,
});

const instructionRuleSchema = z.object({
  ruleKey: z.string(),
  text: z.string(),
  priority: numericSchema,
});

const policyRuleSchema = instructionRuleSchema.extend({
  enforcementTypeId: numericSchema,
  enforcementTypeName: z.string(),
});

const artifactDetailSchema = artifactSchema.extend({
  workspaceId: z.string(),
  currentRevision: z
    .object({
      id: z.string(),
      number: numericSchema,
      content: z.string(),
      contentHash: z.string(),
      sizeInBytes: numericSchema,
      createdAtUtc: z.string(),
    })
    .nullable(),
  instructionRules: z.array(instructionRuleSchema),
  policyRules: z.array(policyRuleSchema),
});

const artifactRevisionSchema = z.object({
  id: z.string(),
  number: numericSchema,
  contentHash: z.string(),
  sizeInBytes: numericSchema,
  createdAtUtc: z.string(),
});

const bindingSchema = z.object({
  id: z.string(),
  artifactRevisionId: z.string(),
  workspaceId: z.string(),
  organizationId: nullableStringSchema,
  projectId: nullableStringSchema,
  repositoryCanonicalUri: nullableStringSchema,
  repositoryRelativePathPrefix: nullableStringSchema,
  branch: nullableStringSchema,
  taskId: nullableStringSchema,
  agent: nullableStringSchema,
  createdAtUtc: z.string(),
});

const provenanceSchema = z.object({
  clientIdentity: z.string(),
  sessionIdentity: nullableStringSchema,
  capturedAtUtc: z.string(),
  userConfirmed: z.boolean(),
  supersededMemoryId: nullableStringSchema,
});

const memorySchema = z.object({
  memoryId: z.string(),
  artifactId: z.string(),
  revisionId: z.string(),
  title: z.string(),
  content: z.string(),
  categoryTypeId: numericSchema,
  categoryTypeName: z.string(),
  confidence: numericSchema,
  score: numericSchema,
  provenance: provenanceSchema,
});

const sourceSchema = z.object({
  id: z.string(),
  workspaceId: z.string(),
  name: z.string(),
  locator: z.string(),
  typeId: numericSchema,
  typeName: z.string(),
  statusId: numericSchema,
  statusName: z.string(),
  priority: numericSchema,
  createdAtUtc: z.string(),
});

const importSchema = z.object({
  id: z.string(),
  sourceId: z.string(),
  workspaceId: z.string(),
  statusId: numericSchema,
  statusName: z.string(),
  stage: z.string(),
  requestedAtUtc: z.string(),
  startedAtUtc: nullableStringSchema,
  completedAtUtc: nullableStringSchema,
  artifactId: nullableStringSchema,
  artifactRevisionId: nullableStringSchema,
  failureCode: nullableStringSchema,
  failureReason: nullableStringSchema,
});

const selectorSchema = z.object({
  selector: z.string(),
  expected: nullableStringSchema,
  actual: nullableStringSchema,
  matched: z.boolean(),
});

const contextItemSchema = z.object({
  bindingId: z.string(),
  artifactId: z.string(),
  revisionId: z.string(),
  artifactKind: z.string(),
  title: z.string(),
  ruleKey: nullableStringSchema,
  enforcement: nullableStringSchema,
  content: z.string(),
  rulePriority: numericSchema,
  artifactPriority: numericSchema,
  userConfirmed: z.boolean().nullable(),
  confidence: numericSchema.nullable(),
  provenance: provenanceSchema.nullable(),
  specificity: z.object({
    agent: numericSchema,
    task: numericSchema,
    branch: numericSchema,
    pathSegments: numericSchema,
    pathBytes: numericSchema,
    repository: numericSchema,
    project: numericSchema,
    organization: numericSchema,
  }),
  selectors: z.array(selectorSchema),
  sizeInBytes: numericSchema,
  decisionCode: z.string(),
});

const contextResponseSchema = z.object({
  context: z.object({
    workspaceId: z.string(),
    organizationId: nullableStringSchema,
    projectId: nullableStringSchema,
    taskId: nullableStringSchema,
    repositoryCanonicalUri: nullableStringSchema,
    repositoryRelativePath: nullableStringSchema,
    branch: nullableStringSchema,
    agent: z.string(),
    includedItems: z.array(contextItemSchema),
    excludedItems: z.array(contextItemSchema),
    conflicts: z.array(
      z.object({
        ruleKey: z.string(),
        conflictCode: z.string(),
        artifactIds: z.array(z.string()),
        winnerArtifactId: nullableStringSchema,
        explanation: z.string(),
      }),
    ),
    explanations: z.array(
      z.object({
        bindingId: z.string(),
        artifactId: z.string(),
        revisionId: z.string(),
        decisionCode: z.string(),
        explanation: z.string(),
      }),
    ),
    budget: z.object({
      requestedBytes: numericSchema,
      hardPolicyBytes: numericSchema,
      usedBytes: numericSchema,
      remainingBytes: numericSchema,
      includedItemCount: numericSchema,
      excludedItemCount: numericSchema,
    }),
  }),
  projection: z.object({
    agent: z.string(),
    format: z.string(),
    mediaType: z.string(),
    content: z.string(),
    sizeInBytes: numericSchema,
  }),
});

export const parseWorkspaceCreated = (value: unknown): CreateWorkspaceResponse =>
  z
    .object({
      workspaceId: z.string(),
      organizationId: nullableStringSchema,
    })
    .parse(value);

export const parseProjects = (value: unknown): ListProjectsResponse =>
  z.object({ items: z.array(projectSchema) }).parse(value);

export const parseTasks = (value: unknown): ListWorkspaceTasksResponse =>
  z.object({ items: z.array(taskSchema) }).parse(value);

export const parseArtifacts = (value: unknown): ListArtifactsResponse =>
  z.object({ items: z.array(artifactSchema) }).parse(value);

export const parseArtifact = (value: unknown): GetArtifactByIdResponse => artifactDetailSchema.parse(value);

export const parseArtifactRevisions = (value: unknown): ListArtifactRevisionsResponse =>
  z.object({ items: z.array(artifactRevisionSchema) }).parse(value);

export const parseBindings = (value: unknown): ListBindingsResponse =>
  z.object({ items: z.array(bindingSchema) }).parse(value);

export const parseMemories = (value: unknown): SearchMemoryResponse =>
  z.object({ items: z.array(memorySchema) }).parse(value);

export const parseSources = (value: unknown): ListSourcesResponse =>
  z.object({ items: z.array(sourceSchema) }).parse(value);

export const parseImports = (value: unknown): ListImportsResponse =>
  z.object({ items: z.array(importSchema) }).parse(value);

export const parseContext = (value: unknown): ConsoleContextBuildResponse => contextResponseSchema.parse(value);

export const ignoreResponse = (): void => undefined;
