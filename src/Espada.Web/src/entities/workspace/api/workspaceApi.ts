import type {
  ConsoleBuildContextRequest,
  ConsoleCreateArtifactRequest,
  ConsoleCreateProjectRequest,
  ConsoleCreateProjectTaskRequest,
  ConsoleRememberMemoryRequest,
  ConsoleReviseArtifactRequest,
  ConsoleSetBindingRequest,
  CreateWorkspaceRequest,
} from 'shared/api/generated';
import { bffDelete, bffGet, bffPost, bffPostCommand } from 'shared/api';
import {
  ignoreResponse,
  parseArtifact,
  parseArtifactRevisions,
  parseArtifacts,
  parseBindings,
  parseContext,
  parseImports,
  parseMemories,
  parseProjects,
  parseSources,
  parseTasks,
  parseWorkspaceCreated,
} from '../model/workspaceSchemas';

export type ArtifactArea = 'artifacts' | 'instructions' | 'policies';

export type SourceDefinitionRequest =
  | {
      type: 'file';
      localPath: string;
      blob: null;
      fileName: string;
      mediaType: string;
    }
  | {
      type: 'webPage';
      uri: string;
    }
  | {
      type: 'plainText';
      title: string;
      content: string;
    }
  | {
      type: 'conversation';
      title: string;
      messages: Array<{
        role: string;
        author: null | string;
        content: string;
        timestamp: null;
      }>;
    }
  | {
      type: 'connector';
      pluginId: string;
      version: string;
      resource: string;
      arguments: Record<string, unknown>;
    };

export interface RegisterSourceBody {
  name: string;
  definition: SourceDefinitionRequest;
}

export interface RequestImportBody {
  sourceId: string;
  options: {
    embeddingModel?: string;
    chunkingStrategy: string;
    maxCharacters: number;
    overlapCharacters: number;
    semanticThreshold: number;
  };
}

const workspacePath = (workspaceId: string): string => `/bff/workspaces/${encodeURIComponent(workspaceId)}`;

export const createWorkspace = (request: CreateWorkspaceRequest, signal?: AbortSignal) =>
  bffPost('/bff/workspaces', request, parseWorkspaceCreated, signal);

export const getProjects = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/projects`, parseProjects, signal);

export const createProject = (workspaceId: string, request: ConsoleCreateProjectRequest, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/projects`, request, ignoreResponse, signal);

export const getTasks = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/tasks`, parseTasks, signal);

export const createTask = (workspaceId: string, request: ConsoleCreateProjectTaskRequest, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/tasks`, request, ignoreResponse, signal);

export const completeTask = (workspaceId: string, taskId: string, signal?: AbortSignal) =>
  bffPostCommand(`${workspacePath(workspaceId)}/tasks/${encodeURIComponent(taskId)}/complete`, signal);

export const archiveTask = (workspaceId: string, taskId: string, signal?: AbortSignal) =>
  bffPostCommand(`${workspacePath(workspaceId)}/tasks/${encodeURIComponent(taskId)}/archive`, signal);

export const getArtifacts = (workspaceId: string, area: ArtifactArea, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/${area}`, parseArtifacts, signal);

export const createArtifact = (
  workspaceId: string,
  area: ArtifactArea,
  request: ConsoleCreateArtifactRequest,
  signal?: AbortSignal,
) => bffPost(`${workspacePath(workspaceId)}/${area}`, request, ignoreResponse, signal);

export const getArtifact = (workspaceId: string, artifactId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/artifacts/${encodeURIComponent(artifactId)}`, parseArtifact, signal);

export const getArtifactRevisions = (workspaceId: string, artifactId: string, signal?: AbortSignal) =>
  bffGet(
    `${workspacePath(workspaceId)}/artifacts/${encodeURIComponent(artifactId)}/revisions`,
    parseArtifactRevisions,
    signal,
  );

export const reviseArtifact = (
  workspaceId: string,
  area: ArtifactArea,
  artifactId: string,
  request: ConsoleReviseArtifactRequest,
  signal?: AbortSignal,
) =>
  bffPost(
    `${workspacePath(workspaceId)}/${area}/${encodeURIComponent(artifactId)}/revisions`,
    request,
    ignoreResponse,
    signal,
  );

export const getBindings = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/bindings`, parseBindings, signal);

export const setBinding = (workspaceId: string, request: ConsoleSetBindingRequest, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/bindings`, request, ignoreResponse, signal);

export const removeBinding = (workspaceId: string, bindingId: string, signal?: AbortSignal) =>
  bffDelete(`${workspacePath(workspaceId)}/bindings/${encodeURIComponent(bindingId)}`, signal);

export const searchMemories = (workspaceId: string, query: string, signal?: AbortSignal) => {
  const parameters = new URLSearchParams({ q: query, topK: '25' });
  return bffGet(`${workspacePath(workspaceId)}/memories/search?${parameters}`, parseMemories, signal);
};

export const rememberMemory = (workspaceId: string, request: ConsoleRememberMemoryRequest, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/memories`, request, ignoreResponse, signal);

export const getSources = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/sources`, parseSources, signal);

export const registerSource = (workspaceId: string, request: RegisterSourceBody, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/sources`, request, ignoreResponse, signal);

export const getImports = (workspaceId: string, signal?: AbortSignal) =>
  bffGet(`${workspacePath(workspaceId)}/imports`, parseImports, signal);

export const requestImport = (workspaceId: string, request: RequestImportBody, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/imports`, request, ignoreResponse, signal, crypto.randomUUID());

export const cancelImport = (workspaceId: string, importId: string, signal?: AbortSignal) =>
  bffPostCommand(`${workspacePath(workspaceId)}/imports/${encodeURIComponent(importId)}/cancel`, signal);

export const buildContext = (workspaceId: string, request: ConsoleBuildContextRequest, signal?: AbortSignal) =>
  bffPost(`${workspacePath(workspaceId)}/context`, request, parseContext, signal);

export const logoutConsole = (signal?: AbortSignal) => bffPostCommand('/bff/session/logout', signal);
