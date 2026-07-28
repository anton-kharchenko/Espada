import { Link } from 'react-router';
import { ROUTES } from 'shared/config';
import { LogoMark } from 'shared/ui';
import { MarketingLayout, repositoryUrl } from 'widgets';

const capabilities = [
  {
    index: '01',
    title: 'Instructions',
    body: 'Keep repository, workspace, path, branch, and task guidance structured and deterministic.',
  },
  {
    index: '02',
    title: 'Memory',
    body: 'Carry typed facts, decisions, preferences, and provenance between agents and sessions.',
  },
  {
    index: '03',
    title: 'Skills & plugins',
    body: 'Discover reusable capabilities once, then expose them in the format each agent understands.',
  },
  {
    index: '04',
    title: 'Policies',
    body: 'Resolve hard rules by scope and priority — never by a semantic-search guess.',
  },
] as const;

const steps = [
  ['Store', 'Model context as typed artifacts instead of scattered Markdown files.'],
  ['Resolve', 'Select the right context for the repository, branch, path, task, and agent.'],
  ['Deliver', 'Serve one canonical result through MCP and disposable compatibility adapters.'],
] as const;

const Arrow = () => (
  <svg aria-hidden="true" className="arrow" viewBox="0 0 16 16">
    <path
      d="M3 8h9M8.5 4.5 12 8l-3.5 3.5"
      fill="none"
      stroke="currentColor"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

export const LandingPage = () => (
  <MarketingLayout>
    <section className="hero" id="top" aria-labelledby="hero-title">
      <div className="hero-copy">
        <p className="eyebrow">
          <span /> Open source · Local first
        </p>
        <h1 id="hero-title">
          One context.
          <br />
          Every coding agent.
        </h1>
        <p className="hero-lede">
          Run Espada free on your own machine and give Codex, Claude, Gemini, and other MCP clients one canonical source
          of instructions, memory, policies, and task context. Choose paid managed collaboration only when a team needs
          to share it.
        </p>
        <div className="hero-actions">
          <a className="button button-primary" href={repositoryUrl} rel="noreferrer" target="_blank">
            Explore on GitHub <Arrow />
          </a>
          <Link className="button button-secondary" to={ROUTES.pricing}>
            Local vs team
          </Link>
        </div>
        <p className="hero-note">Local use is free. Managed team collaboration is paid.</p>
      </div>

      <div className="resolver-card" aria-label="Espada context resolution example">
        <div className="resolver-header">
          <span className="window-dots">
            <i />
            <i />
            <i />
          </span>
          <span>context.resolve</span>
          <span className="status">
            <i /> deterministic
          </span>
        </div>
        <div className="resolver-body">
          <div className="resolver-sources">
            <p className="panel-label">INPUT SCOPE</p>
            <div>
              <span>workspace</span>
              <b>espada</b>
            </div>
            <div>
              <span>repository</span>
              <b>anton-kharchenko/Espada</b>
            </div>
            <div>
              <span>branch</span>
              <b>feature/*</b>
            </div>
            <div>
              <span>agent</span>
              <b>codex</b>
            </div>
          </div>
          <div className="resolver-line">
            <span>resolve</span>
          </div>
          <div className="resolver-output">
            <p className="panel-label">RESOLVED CONTEXT</p>
            <div className="artifact-row">
              <span className="artifact-dot amber" />
              Instructions <b>12</b>
            </div>
            <div className="artifact-row">
              <span className="artifact-dot cyan" />
              Memories <b>8</b>
            </div>
            <div className="artifact-row">
              <span className="artifact-dot pink" />
              Skills <b>4</b>
            </div>
            <div className="artifact-row">
              <span className="artifact-dot green" />
              Policies <b>6</b>
            </div>
            <p className="explain-note">
              <span>✓</span> Every inclusion is explainable
            </p>
          </div>
        </div>
      </div>
    </section>

    <section className="agent-bar" aria-label="Supported agent ecosystem">
      <span>ONE RUNTIME FOR</span>
      <div>
        <b>CODEX</b>
        <b>CLAUDE</b>
        <b>GEMINI</b>
        <b>MCP</b>
      </div>
    </section>

    <section className="section capabilities" id="capabilities" aria-labelledby="capabilities-title">
      <div className="section-heading">
        <p className="eyebrow">
          <span /> Structured context
        </p>
        <h2 id="capabilities-title">
          Stop rebuilding context
          <br />
          for every agent.
        </h2>
        <p>
          Espada keeps the canonical model typed and portable in one workspace. Agent-specific projections are generated
          on request, never stored as a second source of truth.
        </p>
      </div>
      <div className="capability-grid">
        {capabilities.map((capability) => (
          <article key={capability.title}>
            <span>{capability.index}</span>
            <h3>{capability.title}</h3>
            <p>{capability.body}</p>
          </article>
        ))}
      </div>
    </section>

    <section className="section workflow" id="workflow" aria-labelledby="workflow-title">
      <div className="section-heading compact">
        <p className="eyebrow">
          <span /> How it works
        </p>
        <h2 id="workflow-title">A simple path from rules to runtime.</h2>
      </div>
      <ol className="workflow-list">
        {steps.map(([title, body], index) => (
          <li key={title}>
            <span>0{index + 1}</span>
            <div>
              <h3>{title}</h3>
              <p>{body}</p>
            </div>
          </li>
        ))}
      </ol>
      <div className="quick-start">
        <div>
          <p className="panel-label">DEVELOP LOCALLY</p>
          <h3>Run the complete local stack for free.</h3>
          <p>No Espada account or managed cloud dependency is required.</p>
        </div>
        <pre aria-label="PowerShell quick start">
          <code>
            <span>git clone</span> https://github.com/anton-kharchenko/Espada.git{`\n`}
            <span>cd</span> Espada{`\n`}
            <span>dotnet run</span> --project src/Aspire/Aspire.csproj
          </code>
        </pre>
      </div>
    </section>

    <section className="closing" aria-labelledby="closing-title">
      <LogoMark />
      <h2 id="closing-title">
        Context belongs to you.
        <br />
        Not to one agent.
      </h2>
      <p>Build the shared, explainable runtime for the tools you already use.</p>
      <div className="hero-actions closing-actions">
        <a className="button button-primary" href={repositoryUrl} rel="noreferrer" target="_blank">
          Start locally <Arrow />
        </a>
        <Link className="button button-secondary" to={ROUTES.pricing}>
          Compare options
        </Link>
      </div>
    </section>
  </MarketingLayout>
);
