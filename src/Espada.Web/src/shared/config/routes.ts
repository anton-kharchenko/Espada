const routeSegments = {
  app: 'app',
  workspaces: 'workspaces',
  workspaceId: ':workspaceId',
  billing: 'billing',
  settings: 'settings',
} as const;

export const DEFAULT_WORKSPACE_SECTION_PATH = 'overview';

export const ROUTES = {
  home: '/',
  pricing: '/pricing',
  app: `/${routeSegments.app}`,
  setup: '/setup',
  billing: `/${routeSegments.app}/${routeSegments.billing}`,
  settings: `/${routeSegments.app}/${routeSegments.settings}`,
} as const;

export const APP_ROUTE_PATHS = {
  workspace: `${routeSegments.workspaces}/${routeSegments.workspaceId}`,
  billing: routeSegments.billing,
  settings: routeSegments.settings,
} as const;

export type WorkspaceSectionKey =
  | 'overview'
  | 'projects'
  | 'tasks'
  | 'instructions'
  | 'policies'
  | 'bindings'
  | 'context-preview'
  | 'context-explain'
  | 'memories'
  | 'sources'
  | 'imports'
  | 'artifacts';

export interface WorkspaceSection {
  key: WorkspaceSectionKey;
  path: string;
  title: string;
  description: string;
  group: 'Workspace' | 'Context' | 'Knowledge';
}

export const workspaceSections: readonly WorkspaceSection[] = [
  {
    key: 'overview',
    path: DEFAULT_WORKSPACE_SECTION_PATH,
    title: 'Overview',
    description: 'Workspace status and canonical context entry points.',
    group: 'Workspace',
  },
  {
    key: 'projects',
    path: 'projects',
    title: 'Projects',
    description: 'Repositories and local aliases registered with this workspace.',
    group: 'Workspace',
  },
  {
    key: 'tasks',
    path: 'tasks',
    title: 'Tasks',
    description: 'Active, completed, and archived task context.',
    group: 'Workspace',
  },
  {
    key: 'instructions',
    path: 'instructions',
    title: 'Instructions',
    description: 'Structured instructions resolved for coding agents.',
    group: 'Context',
  },
  {
    key: 'policies',
    path: 'policies',
    title: 'Policies',
    description: 'Administrative hard policies and scoped soft rules.',
    group: 'Context',
  },
  {
    key: 'bindings',
    path: 'bindings',
    title: 'Bindings',
    description: 'Selectors that bind artifact revisions to context scopes.',
    group: 'Context',
  },
  {
    key: 'context-preview',
    path: 'context/preview',
    title: 'Context preview',
    description: 'Preview canonical context and agent projections.',
    group: 'Context',
  },
  {
    key: 'context-explain',
    path: 'context/explain',
    title: 'Context explain',
    description: 'Inspect matches, conflicts, exclusions, and budget decisions.',
    group: 'Context',
  },
  {
    key: 'memories',
    path: 'memories',
    title: 'Memories',
    description: 'Typed shared memory with provenance and confidence.',
    group: 'Knowledge',
  },
  {
    key: 'sources',
    path: 'sources',
    title: 'Sources',
    description: 'Registered source definitions for this workspace.',
    group: 'Knowledge',
  },
  {
    key: 'imports',
    path: 'imports',
    title: 'Imports',
    description: 'Source import jobs and their current state.',
    group: 'Knowledge',
  },
  {
    key: 'artifacts',
    path: 'artifacts',
    title: 'Artifacts',
    description: 'Canonical artifacts and immutable revisions.',
    group: 'Knowledge',
  },
] as const;

export const getWorkspaceRoute = (workspaceId: string, sectionPath = DEFAULT_WORKSPACE_SECTION_PATH): string =>
  `${ROUTES.app}/${routeSegments.workspaces}/${encodeURIComponent(workspaceId)}/${sectionPath}`;

export const getWorkspaceQueryKey = (workspaceId: string, resource: string): readonly string[] =>
  ['workspaces', workspaceId, resource] as const;
