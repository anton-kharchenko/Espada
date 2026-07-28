import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedConsoleRoute } from 'features/session';
import { AccountPage } from 'pages/account';
import { LandingPage } from 'pages/landing';
import { WorkspacePage } from 'pages/workspace';
import { APP_ROUTE_PATHS, DEFAULT_WORKSPACE_SECTION_PATH, ROUTES, workspaceSections } from 'shared/config';
import { ConsoleShell } from 'widgets';
import { WorkspaceRedirect } from './WorkspaceRedirect';

export const AppRoutes = () => {
  return (
    <Routes>
      <Route path={ROUTES.home} element={<LandingPage />} />
      <Route path={ROUTES.app} element={<ProtectedConsoleRoute />}>
        <Route index element={<WorkspaceRedirect />} />
        <Route element={<ConsoleShell />}>
          <Route path={APP_ROUTE_PATHS.workspace}>
            <Route index element={<Navigate to={DEFAULT_WORKSPACE_SECTION_PATH} replace />} />
            {workspaceSections.map((section) => (
              <Route key={section.key} path={section.path} element={<WorkspacePage section={section} />} />
            ))}
          </Route>
          <Route path={APP_ROUTE_PATHS.billing} element={<AccountPage title="Billing" />} />
          <Route path={APP_ROUTE_PATHS.settings} element={<AccountPage title="Settings" />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={ROUTES.home} replace />} />
    </Routes>
  );
};
