import { useMutation, useQueryClient } from '@tanstack/react-query';
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
import { consoleSessionQueryOptions } from 'entities/session';
import {
  archiveTask,
  buildContext,
  cancelImport,
  completeTask,
  createArtifact,
  createProject,
  createTask,
  createWorkspace,
  logoutConsole,
  registerSource,
  rememberMemory,
  removeBinding,
  requestImport,
  reviseArtifact,
  setBinding,
  workspaceQueryKeys,
  type ArtifactArea,
  type RegisterSourceBody,
  type RequestImportBody,
} from 'entities/workspace';

export const useCreateWorkspace = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateWorkspaceRequest) => createWorkspace(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: consoleSessionQueryOptions.queryKey }),
  });
};

export const useCreateProject = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ConsoleCreateProjectRequest) => createProject(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'projects') }),
  });
};

export const useCreateTask = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ConsoleCreateProjectTaskRequest) => createTask(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'tasks') }),
  });
};

export const useCompleteTask = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (taskId: string) => completeTask(workspaceId, taskId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'tasks') }),
  });
};

export const useArchiveTask = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (taskId: string) => archiveTask(workspaceId, taskId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'tasks') }),
  });
};

export const useCreateArtifact = (workspaceId: string, area: ArtifactArea) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ConsoleCreateArtifactRequest) => createArtifact(workspaceId, area, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, area) }),
  });
};

export const useReviseArtifact = (workspaceId: string, area: ArtifactArea) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ artifactId, request }: { artifactId: string; request: ConsoleReviseArtifactRequest }) =>
      reviseArtifact(workspaceId, area, artifactId, request),
    onSuccess: (_, { artifactId }) =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, area) }),
        queryClient.invalidateQueries({
          queryKey: [...workspaceQueryKeys.area(workspaceId, 'artifacts'), artifactId],
        }),
      ]),
  });
};

export const useSetBinding = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ConsoleSetBindingRequest) => setBinding(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'bindings') }),
  });
};

export const useRemoveBinding = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (bindingId: string) => removeBinding(workspaceId, bindingId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'bindings') }),
  });
};

export const useRememberMemory = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ConsoleRememberMemoryRequest) => rememberMemory(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'memories') }),
  });
};

export const useRegisterSource = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: RegisterSourceBody) => registerSource(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'sources') }),
  });
};

export const useRequestImport = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: RequestImportBody) => requestImport(workspaceId, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'imports') }),
  });
};

export const useCancelImport = (workspaceId: string) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (importId: string) => cancelImport(workspaceId, importId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: workspaceQueryKeys.area(workspaceId, 'imports') }),
  });
};

export const useBuildContext = (workspaceId: string) =>
  useMutation({
    mutationFn: (request: ConsoleBuildContextRequest) => buildContext(workspaceId, request),
  });

export const useLogoutConsole = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => logoutConsole(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: consoleSessionQueryOptions.queryKey }),
  });
};
