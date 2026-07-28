import type { ReactNode } from 'react';
import { Link } from 'react-router';
import { ROUTES } from 'shared/config';
import { LogoMark } from 'shared/ui';

export const repositoryUrl = 'https://github.com/anton-kharchenko/Espada';

interface MarketingLayoutProps {
  children: ReactNode;
}

export const MarketingLayout = ({ children }: MarketingLayoutProps) => (
  <div className="site-shell">
    <a className="skip-link" href="#content">
      Skip to content
    </a>
    <header className="site-header">
      <Link className="brand" to={ROUTES.home} aria-label="Espada home">
        <LogoMark />
        <span>Espada</span>
      </Link>
      <nav aria-label="Primary navigation">
        <a href="/#capabilities">Context</a>
        <a href="/#workflow">How it works</a>
        <Link to={ROUTES.pricing}>Pricing</Link>
        <Link className="nav-console" to={ROUTES.app}>
          Console
        </Link>
        <a className="nav-github" href={repositoryUrl} rel="noreferrer" target="_blank">
          GitHub <span aria-hidden="true">↗</span>
        </a>
      </nav>
    </header>
    <main id="content">{children}</main>
    <footer>
      <Link className="brand" to={ROUTES.home}>
        <LogoMark />
        <span>Espada</span>
      </Link>
      <p>Free local context runtime. Paid managed collaboration for teams.</p>
      <div>
        <Link to={ROUTES.pricing}>Pricing</Link>
        <a href={repositoryUrl} rel="noreferrer" target="_blank">
          GitHub
        </a>
        <a href={`${repositoryUrl}/blob/master/LICENSE`} rel="noreferrer" target="_blank">
          MIT License
        </a>
        <a href={`${repositoryUrl}/security/policy`} rel="noreferrer" target="_blank">
          Security
        </a>
      </div>
    </footer>
  </div>
);
