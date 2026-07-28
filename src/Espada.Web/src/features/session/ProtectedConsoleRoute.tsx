import { Outlet } from 'react-router';
import { useConsoleSession } from 'entities/session';
import { PageState, type PageStateKind } from 'shared/ui';
import { getSessionStateKind } from './sessionState';

const copyByState: Record<PageStateKind, { title: string; description: string }> = {
  loading: {
    title: 'Loading console',
    description: 'Checking the local Espada session.',
  },
  empty: {
    title: 'Console session required',
    description: 'Open the one-time console link created by the Espada CLI or daemon.',
  },
  offline: {
    title: 'Espada is offline',
    description: 'Start the local runtime, then reload this page.',
  },
  forbidden: {
    title: 'Access denied',
    description: 'This browser session cannot access the requested Espada workspace.',
  },
  'read-only': {
    title: 'Read-only session',
    description: 'Changes are disabled for this console session.',
  },
  'rate-limited': {
    title: 'Too many requests',
    description: 'Wait a moment before reloading the console.',
  },
  unavailable: {
    title: 'Console unavailable',
    description: 'The same-origin Espada BFF did not return a valid session.',
  },
};

export const ProtectedConsoleRoute = () => {
  const sessionQuery = useConsoleSession();

  if (sessionQuery.isPending) {
    const copy = copyByState.loading;
    return <PageState kind="loading" title={copy.title} description={copy.description} />;
  }

  if (sessionQuery.isError) {
    const kind = getSessionStateKind(sessionQuery.error);
    const copy = copyByState[kind];
    return <PageState kind={kind} title={copy.title} description={copy.description} />;
  }

  if (!sessionQuery.data.authenticated) {
    const copy = copyByState.empty;
    return <PageState kind="empty" title={copy.title} description={copy.description} />;
  }

  return <Outlet context={sessionQuery.data} />;
};
