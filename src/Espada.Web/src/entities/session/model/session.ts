import { z } from 'zod';

export interface ConsoleUser {
  displayName: string;
  email: string | null;
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

const consoleSessionSchema = z.object({
  authenticated: z.boolean(),
  mode: z.enum(['local', 'cloud']),
  user: z
    .object({
      displayName: z.string(),
      email: z.string().nullable(),
    })
    .nullable(),
  workspaces: z.array(
    z.object({
      id: z.string(),
      name: z.string(),
    }),
  ),
  readOnly: z.boolean(),
});

export const parseConsoleSession = (value: unknown): ConsoleSession => {
  const result = consoleSessionSchema.safeParse(value);
  if (!result.success) {
    throw new TypeError('Invalid console session response.', {
      cause: result.error,
    });
  }

  return result.data;
};
