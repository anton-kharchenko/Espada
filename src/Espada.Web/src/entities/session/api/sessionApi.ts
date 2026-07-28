import { bffGet } from 'shared/api';
import { parseConsoleSession, type ConsoleSession } from '../model/session';

export const getConsoleSession = (signal?: AbortSignal): Promise<ConsoleSession> =>
  bffGet('/bff/session', parseConsoleSession, signal);
