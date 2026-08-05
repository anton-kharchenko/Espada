import { AppLayout } from 'app/layout';
import { Providers } from 'app/providers';

export const App = () => {
  return (
    <Providers>
      <AppLayout />
    </Providers>
  );
};
