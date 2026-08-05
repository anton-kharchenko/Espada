import { queryOptions, useQuery } from '@tanstack/react-query';
import {
  getArtifact,
  getArtifactRevisions,
  getArtifacts,
  getBindings,
  getImports,
  getProjects,
  getSources,
  getTasks,
  searchMemories,
  type ArtifactArea,
} from '../api/workspaceApi';
import { workspaceQueryKeys } from './workspaceQueryKeys';

export const projectsQueryOptions = (workspaceId: string) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, 'projects'),
    queryFn: ({ signal }) => getProjects(workspaceId, signal),
  });

export const tasksQueryOptions = (workspaceId: string) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, 'tasks'),
    queryFn: ({ signal }) => getTasks(workspaceId, signal),
  });

export const artifactsQueryOptions = (workspaceId: string, area: ArtifactArea) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, area),
    queryFn: ({ signal }) => getArtifacts(workspaceId, area, signal),
  });

export const artifactQueryOptions = (workspaceId: string, artifactId: string) =>
  queryOptions({
    queryKey: [...workspaceQueryKeys.area(workspaceId, 'artifacts'), artifactId],
    queryFn: ({ signal }) => getArtifact(workspaceId, artifactId, signal),
    enabled: artifactId.length > 0,
  });

export const artifactRevisionsQueryOptions = (workspaceId: string, artifactId: string) =>
  queryOptions({
    queryKey: [...workspaceQueryKeys.area(workspaceId, 'artifacts'), artifactId, 'revisions'],
    queryFn: ({ signal }) => getArtifactRevisions(workspaceId, artifactId, signal),
    enabled: artifactId.length > 0,
  });

export const bindingsQueryOptions = (workspaceId: string) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, 'bindings'),
    queryFn: ({ signal }) => getBindings(workspaceId, signal),
  });

export const memoriesQueryOptions = (workspaceId: string, query: string) =>
  queryOptions({
    queryKey: [...workspaceQueryKeys.area(workspaceId, 'memories'), query],
    queryFn: ({ signal }) => searchMemories(workspaceId, query, signal),
    enabled: query.trim().length > 0,
  });

export const sourcesQueryOptions = (workspaceId: string) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, 'sources'),
    queryFn: ({ signal }) => getSources(workspaceId, signal),
  });

export const importsQueryOptions = (workspaceId: string) =>
  queryOptions({
    queryKey: workspaceQueryKeys.area(workspaceId, 'imports'),
    queryFn: ({ signal }) => getImports(workspaceId, signal),
    refetchInterval: 5_000,
  });

export const useProjects = (workspaceId: string) => useQuery(projectsQueryOptions(workspaceId));

export const useTasks = (workspaceId: string) => useQuery(tasksQueryOptions(workspaceId));

export const useArtifacts = (workspaceId: string, area: ArtifactArea) =>
  useQuery(artifactsQueryOptions(workspaceId, area));

export const useArtifact = (workspaceId: string, artifactId: string) =>
  useQuery(artifactQueryOptions(workspaceId, artifactId));

export const useArtifactRevisions = (workspaceId: string, artifactId: string) =>
  useQuery(artifactRevisionsQueryOptions(workspaceId, artifactId));

export const useBindings = (workspaceId: string) => useQuery(bindingsQueryOptions(workspaceId));

export const useMemories = (workspaceId: string, query: string) => useQuery(memoriesQueryOptions(workspaceId, query));

export const useSources = (workspaceId: string) => useQuery(sourcesQueryOptions(workspaceId));

export const useImports = (workspaceId: string) => useQuery(importsQueryOptions(workspaceId));
