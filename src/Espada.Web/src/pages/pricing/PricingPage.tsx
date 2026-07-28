import { Link } from 'react-router';
import { ROUTES } from 'shared/config';
import { MarketingLayout, repositoryUrl } from 'widgets';

const localFeatures = [
  'One local runtime for Codex, Claude, Gemini, and MCP clients',
  'Canonical instructions, memory, sources, bindings, and context',
  'Your PostgreSQL/pgvector database and local blob storage',
  'Offline operation with no Espada account required',
] as const;

const teamFeatures = [
  'Shared team workspaces and memberships',
  'Managed cloud persistence and synchronization',
  'Central policies, access controls, and audit-oriented context',
  'Commercial limits, support, and workspace administration',
] as const;

export const PricingPage = () => (
  <MarketingLayout>
    <section className="pricing-hero">
      <p className="eyebrow">
        <span /> Simple product boundary
      </p>
      <h1>
        Free on your machine.
        <br />
        Paid when the team shares.
      </h1>
      <p>
        Espada Community is open source and free for local or self-hosted use. Managed team collaboration is a paid
        service because shared infrastructure, storage, synchronization, and support have real operating costs.
      </p>
    </section>

    <section className="pricing-grid" aria-label="Espada product options">
      <article className="pricing-card pricing-card-local">
        <div>
          <p className="panel-label">COMMUNITY · LOCAL</p>
          <h2>Free</h2>
          <span>Use Espada on your own machine or self-host it.</span>
        </div>
        <ul>
          {localFeatures.map((feature) => (
            <li key={feature}>{feature}</li>
          ))}
        </ul>
        <a className="button button-primary" href={repositoryUrl} rel="noreferrer" target="_blank">
          Get the source
        </a>
      </article>

      <article className="pricing-card pricing-card-team">
        <div>
          <p className="panel-label">TEAM · MANAGED</p>
          <h2>Paid</h2>
          <span>Collaborate through Espada-managed cloud capabilities.</span>
        </div>
        <ul>
          {teamFeatures.map((feature) => (
            <li key={feature}>{feature}</li>
          ))}
        </ul>
        <Link className="button button-secondary" to={ROUTES.app}>
          Open Console
        </Link>
        <p className="pricing-note">Exact plans and prices will be published before managed team availability.</p>
      </article>
    </section>

    <section className="pricing-principles" aria-labelledby="pricing-principles-title">
      <div>
        <p className="eyebrow">
          <span /> What this means
        </p>
        <h2 id="pricing-principles-title">No cloud dependency hidden inside local use.</h2>
      </div>
      <dl>
        <div>
          <dt>Does local Espada require registration?</dt>
          <dd>No. The local runtime works offline against infrastructure you control.</dd>
        </div>
        <div>
          <dt>Can local agents share one memory?</dt>
          <dd>Yes. Codex, Claude, and Gemini can connect to the same local workspace through MCP.</dd>
        </div>
        <div>
          <dt>When does payment apply?</dt>
          <dd>When a team chooses managed shared workspaces, cloud synchronization, and hosted operations.</dd>
        </div>
      </dl>
    </section>
  </MarketingLayout>
);
