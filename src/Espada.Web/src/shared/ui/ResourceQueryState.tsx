import { BffError, type BffErrorKind } from 'shared/api';
import { PageState, type PageStateKind } from './PageState';

interface ResourceQueryStateProps {
  isPending: boolean;
  error: unknown;
  resourceName: string;
  onRetry: () => void;
}

const stateByError: Record<BffErrorKind, PageStateKind> = {
  unauthorized: 'empty',
  forbidden: 'forbidden',
  'rate-limited': 'rate-limited',
  offline: 'offline',
  unavailable: 'unavailable',
};

export const ResourceQueryState = ({ isPending, error, resourceName, onRetry }: ResourceQueryStateProps) => {
  if (isPending) {
    return (
      <PageState
        kind="loading"
        title={`Loading ${resourceName}`}
        description="Reading canonical workspace data from the same-origin BFF."
      />
    );
  }

  if (!error) return null;

  const kind = error instanceof BffError ? stateByError[error.kind] : 'unavailable';
  const description = error instanceof Error ? error.message : `The ${resourceName} view is unavailable.`;

  return (
    <PageState
      kind={kind}
      title={`Could not load ${resourceName}`}
      description={description}
      action={
        <button className="button button-secondary page-state-action" type="button" onClick={onRetry}>
          Retry
        </button>
      }
    />
  );
};
