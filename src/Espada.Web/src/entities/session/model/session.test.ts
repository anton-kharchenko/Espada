import { describe, expect, it } from 'vitest';
import { parseConsoleSession } from './session';

describe('parseConsoleSession', () => {
  it('accepts the same-origin BFF session contract', () => {
    expect(
      parseConsoleSession({
        authenticated: true,
        mode: 'local',
        user: { displayName: 'Anton', email: null },
        workspaces: [{ id: 'workspace-one', name: 'Espada' }],
        readOnly: false,
      }),
    ).toMatchObject({ authenticated: true, mode: 'local', readOnly: false });
  });

  it('rejects malformed session payloads', () => {
    expect(() => parseConsoleSession({ authenticated: true })).toThrow('Invalid console session response.');
  });
});
