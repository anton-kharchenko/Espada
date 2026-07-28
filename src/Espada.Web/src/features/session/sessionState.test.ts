import { describe, expect, it } from 'vitest';
import { BffError } from 'shared/api';
import { getSessionStateKind } from './sessionState';

describe('getSessionStateKind', () => {
  it.each([
    ['unauthorized', 'empty'],
    ['forbidden', 'forbidden'],
    ['rate-limited', 'rate-limited'],
    ['offline', 'offline'],
    ['unavailable', 'unavailable'],
  ] as const)('maps %s BFF errors to %s states', (errorKind, stateKind) => {
    expect(getSessionStateKind(new BffError(errorKind, 'failure'))).toBe(stateKind);
  });
});
