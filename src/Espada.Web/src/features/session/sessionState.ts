import { BffError, type BffErrorKind } from 'shared/api';
import type { PageStateKind } from 'shared/ui';

export const getSessionStateKind = (error: unknown): PageStateKind => {
  if (!(error instanceof BffError)) return 'unavailable';

  const states: Record<BffErrorKind, PageStateKind> = {
    unauthorized: 'empty',
    forbidden: 'forbidden',
    'rate-limited': 'rate-limited',
    offline: 'offline',
    unavailable: 'unavailable',
  };

  return states[error.kind];
};
