import type { ReactNode } from 'react';

export type PageStateKind =
  'loading' | 'empty' | 'offline' | 'forbidden' | 'read-only' | 'rate-limited' | 'unavailable';

interface PageStateProps {
  kind: PageStateKind;
  title: string;
  description: string;
  action?: ReactNode;
  compact?: boolean;
}

const stateSymbols: Record<PageStateKind, string> = {
  loading: '···',
  empty: '○',
  offline: '↯',
  forbidden: '×',
  'read-only': '◇',
  'rate-limited': '⌛',
  unavailable: '—',
};

export const PageState = ({ kind, title, description, action, compact = false }: PageStateProps) => {
  return (
    <section
      className={`page-state page-state-${kind}${compact ? ' page-state-compact' : ''}`}
      aria-live={kind === 'loading' ? 'polite' : undefined}
      aria-busy={kind === 'loading'}
    >
      <span className="page-state-symbol" aria-hidden="true">
        {stateSymbols[kind]}
      </span>
      <div>
        <h2>{title}</h2>
        <p>{description}</p>
        {action}
      </div>
    </section>
  );
};
