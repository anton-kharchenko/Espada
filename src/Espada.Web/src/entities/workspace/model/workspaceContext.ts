import type { ConsoleSession, ConsoleWorkspace } from 'entities/session';

export interface WorkspaceOutletContext {
  session: ConsoleSession;
  workspace: ConsoleWorkspace | null;
}
