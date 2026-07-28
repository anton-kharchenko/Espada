export interface ConsoleUser {
  displayName: string;
  email?: string;
}

export interface ConsoleWorkspace {
  id: string;
  name: string;
}

export interface ConsoleSession {
  authenticated: boolean;
  mode: 'local' | 'cloud';
  user: ConsoleUser | null;
  workspaces: readonly ConsoleWorkspace[];
  readOnly: boolean;
}

const isRecord = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null;

const isWorkspace = (value: unknown): value is ConsoleWorkspace =>
  isRecord(value) && typeof value.id === 'string' && typeof value.name === 'string';

export const parseConsoleSession = (value: unknown): ConsoleSession => {
  if (
    !isRecord(value) ||
    typeof value.authenticated !== 'boolean' ||
    (value.mode !== 'local' && value.mode !== 'cloud') ||
    !Array.isArray(value.workspaces) ||
    !value.workspaces.every(isWorkspace) ||
    typeof value.readOnly !== 'boolean'
  ) {
    throw new TypeError('Invalid console session response.');
  }

  const user = value.user;
  if (
    user !== null &&
    (!isRecord(user) ||
      typeof user.displayName !== 'string' ||
      (user.email !== undefined && typeof user.email !== 'string'))
  ) {
    throw new TypeError('Invalid console user response.');
  }

  return {
    authenticated: value.authenticated,
    mode: value.mode,
    user: user as ConsoleUser | null,
    workspaces: value.workspaces,
    readOnly: value.readOnly,
  };
};
