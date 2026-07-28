import { useOutletContext } from 'react-router-dom';
import type { WorkspaceOutletContext } from 'entities/workspace';
import { PageState } from 'shared/ui';

interface AccountPageProps {
  title: 'Billing' | 'Settings';
}

export const AccountPage = ({ title }: AccountPageProps) => {
  const { session } = useOutletContext<WorkspaceOutletContext>();

  return (
    <div className="console-page">
      <header className="console-page-header">
        <p>{session.mode === 'local' ? 'Local runtime' : 'Espada Cloud'}</p>
        <h1>{title}</h1>
        <span>
          {title === 'Billing'
            ? 'Plan, entitlement, and usage information.'
            : 'Console session and runtime preferences.'}
        </span>
      </header>
      <PageState
        kind="empty"
        title={`${title} data is not available`}
        description="The BFF contract for this view has not been connected. No placeholder account data is shown."
      />
    </div>
  );
};
