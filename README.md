# Espada

Espada is an open-source, local-first context runtime for AI coding agents. It provides Codex, Claude, Gemini, and other MCP-compatible agents with shared instructions, memory, skills, policies, plugins, tools, and session context.

The monorepo combines .NET backend services, a local daemon, CLI, MCP gateway, synchronization engine, React WebUI, cloud infrastructure, billing, deployment assets, and .NET Aspire orchestration.

## Purpose

Espada exists to replace fragmented agent context stored across `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`, `SKILL.md`, prompt files, and plugin configurations with one structured and explainable context runtime.

The local version works without an account, internet connection, or cloud dependency. Espada Cloud adds optional synchronization, shared workspaces, managed backups, team collaboration, and commercial organization features.

Core product capabilities:

* Store instructions, memory, skills, policies, prompts, plugins, and tool definitions as structured artifacts.
* Provide a shared context to Codex, Claude, Gemini, and other agents through MCP.
* Resolve context based on workspace, repository, branch, directory, task, user, and agent.
* Explain why every instruction, policy, memory, or skill was included or excluded.
* Run fully locally using an external PostgreSQL/pgvector database and filesystem blob storage.
* Synchronize local data with Espada Cloud when explicitly enabled.
* Support shared team workspaces, RBAC, audit history, usage limits, and subscription plans.
* Generate temporary Markdown compatibility files without treating them as the source of truth.
* Run the complete local development stack through Aspire.

## Product modes

Espada supports three primary operating modes.

### Espada Community

The open-source local and self-hosted edition.

* No registration required.
* No internet connection required.
* User-managed PostgreSQL/pgvector database.
* Local filesystem blob storage.
* Local MCP endpoint.
* CLI and React WebUI.
* Codex, Claude, and Gemini adapters.
* Import and export of existing Markdown-based agent instructions.
* Self-hosted cloud deployment.

### Espada Cloud

The optional managed commercial service.

* Cross-device synchronization.
* Shared team workspaces.
* Managed PostgreSQL.
* Azure Blob Storage.
* Backups and revision history.
* Device and session management.
* RBAC and audit logs.
* Billing, subscriptions, and usage limits.
* Private skills and plugin registries.
* Organization-level instructions and policies.

### Remote MCP

A cloud-hosted MCP endpoint for environments where the local daemon cannot be installed, including CI, temporary development environments, and remote agents.

## What is in this repo

| Area               | Path                            | Purpose                                                                                                                                |
| ------------------ | ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Aspire host        | `src/Espada.AppHost`            | Primary local orchestration entry point for services, containers, databases, storage emulators, migrations, telemetry, and dashboards. |
| Service defaults   | `src/Espada.ServiceDefaults`    | Shared Aspire, OpenTelemetry, health-check, resilience, and service-discovery configuration.                                           |
| Domain             | `src/Espada.Domain`             | Core domain models, value objects, revision rules, context semantics, and business invariants.                                         |
| Application        | `src/Espada.Application`        | Use-case orchestration for context resolution, memory, skills, policies, sessions, synchronization, and billing.                       |
| Contracts          | `src/Espada.Contracts`          | Shared commands, queries, DTOs, events, sync contracts, and JSON schemas.                                                              |
| Infrastructure     | `src/Espada.Infrastructure`     | Shared external integrations and infrastructure implementations.                                                                       |
| PostgreSQL storage | `src/Espada.Db`, `src/Espada.Infrastructure` | Migrations, pgvector and full-text search, local persistence, and managed multi-tenant persistence.                         |
| Blob abstractions  | `src/Espada.Blobs.Abstractions` | Shared contracts for content-addressed blob storage.                                                                                   |
| Filesystem blobs   | `src/Espada.Blobs.FileSystem`   | Local blob storage implementation.                                                                                                     |
| Azure blobs        | `src/Espada.Blobs.Azure`        | Azure Blob Storage implementation for Espada Cloud.                                                                                    |
| MCP protocol       | `src/Espada.Protocol.Mcp`       | MCP tools, resources, prompts, stdio bridge, and Streamable HTTP integration.                                                          |
| Sync protocol      | `src/Espada.Protocol.Sync`      | Offline-first synchronization, event replication, cursors, blob negotiation, and conflict handling.                                    |
| HTTP protocol      | `src/Espada.Protocol.Http`      | REST and internal HTTP contracts for local and cloud services.                                                                         |
| Agent adapters     | `src/Espada.AgentAdapters`      | Codex, Claude, Gemini, and generic MCP client integration.                                                                             |
| Compatibility      | `src/Espada.Compatibility`      | Importers and generated compatibility output for `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`, and `SKILL.md`.                                |
| Plugins            | `src/Espada.Plugins`            | Plugin manifests, permissions, discovery, execution, and registry integration.                                                         |
| Billing            | `src/Espada.Billing`            | Plans, subscriptions, entitlements, usage accounting, trials, and payment webhooks.                                                    |
| Local daemon       | `src/Espada.Daemon`             | Persistent local runtime providing PostgreSQL-backed APIs, MCP, sync workers, and the local WebUI.                                    |
| CLI                | `src/Espada.Cli`                | `espada` command-line interface and native agent launcher.                                                                             |
| Cloud server       | `src/Espada.Server`             | Espada Cloud API, remote MCP, authentication, workspaces, billing, and synchronization endpoints.                                      |
| Workers            | `src/Espada.Workers`            | Cloud background jobs for synchronization, retention, billing, indexing, and cleanup.                                                  |
| WebUI              | `src/Espada.Web`                | React frontend used by the local daemon and Espada Cloud.                                                                              |
| Tests              | `tests/Espada.*.Tests`          | Domain, application, storage, MCP, synchronization, billing, integration, and end-to-end tests.                                        |
| Docs               | `docs` and `DESIGN.md`          | Onboarding, architecture, protocols, security, billing, and visual design guidance.                                                    |

## Technology stack

| Area                | Technology                           |
| ------------------- | ------------------------------------ |
| Runtime             | .NET 10 LTS                          |
| Language            | C# 14                                |
| Local daemon        | .NET Generic Host and ASP.NET Core   |
| MCP                 | Official ModelContextProtocol C# SDK |
| Database            | PostgreSQL, Npgsql, and pgvector     |
| Local blob storage  | Filesystem content-addressed storage |
| Cloud blob storage  | Azure Blob Storage                   |
| CLI                 | System.CommandLine                   |
| Background jobs     | BackgroundService                    |
| WebUI               | React, TypeScript, and Vite          |
| Serialization       | System.Text.Json                     |
| Observability       | OpenTelemetry                        |
| Local orchestration | .NET Aspire                          |

## Prerequisites

| Tool           | Version |
| -------------- | ------- |
| .NET SDK       | 10.0    |
| Docker Desktop | Latest  |
| Node.js        | 22+ LTS |

Install on Windows with winget:

```powershell
winget install Microsoft.DotNet.SDK.10
winget install Docker.DockerDesktop
winget install OpenJS.NodeJS.LTS
```

Optional tools:

```powershell
# Aspire CLI, optional alternative to dotnet run
irm https://aspire.dev/install.ps1 | iex

# Azure CLI, required only for Azure deployment and managed identity workflows
winget install Microsoft.AzureCLI

# PostgreSQL client tools, useful for database troubleshooting
winget install PostgreSQL.PostgreSQL
```

## Getting started

Clone the repository and start the complete development stack:

```powershell
git clone https://github.com/<organization>/espada.git
cd espada
dotnet run --project src/Espada.AppHost/Espada.AppHost.csproj
```

Aspire is the default development entry point.

On the first run it may:

* Restore .NET tools and NuGet packages.
* Install frontend packages.
* Pull required containers.
* Start PostgreSQL and Azurite.
* Build the local daemon and cloud services.
* Run database migrations.
* Configure local development secrets.
* Start the React WebUI.
* Open the Aspire dashboard.

Local development secrets are generated or loaded through supported secret-management mechanisms. Production credentials must not be stored in the repository.

## Run Espada locally

To run only the local daemon:

```powershell
dotnet run --project src/Espada.Daemon/Espada.Daemon.csproj
```

The local daemon uses:

```text
PostgreSQL:            ESPADA_CONNECTION_STRING
Blob storage:          ~/.espada/blobs
Generated artifacts:   ~/.espada/generated
Cache:                 ~/.espada/cache
Logs:                  ~/.espada/logs
```

The default local endpoints are:

```text
WebUI:       http://127.0.0.1:7432
Local API:   http://127.0.0.1:7432/api
MCP:         http://127.0.0.1:7432/mcp
Health:      http://127.0.0.1:7432/health
```

Exact ports may be overridden through configuration or Aspire.

## CLI

Run the CLI from source:

```powershell
dotnet run --project src/Espada.Cli/Espada.Cli.csproj -- status
```

Common commands:

```powershell
# Start and inspect the local daemon
espada start
espada stop
espada status

# Work with context
espada context resolve
espada context explain

# Inspect stored data
espada artifact list
espada memory search
espada skill list
espada policy list
espada session list

# Import existing agent files
espada import .

# Export a portable workspace
espada export --output workspace.espada

# Launch native coding agents with Espada context
espada run codex
espada run claude
espada run gemini

# Connect to Espada Cloud
espada login
espada workspace link my-workspace
espada sync enable
espada sync status
espada sync push
espada sync pull
```

## Common development commands

Backend validation from the repository root:

```powershell
# Run all .NET tests
dotnet test

# Prefer targeted tests for focused changes
dotnet test tests/Espada.Domain.Tests
dotnet test tests/Espada.Application.Tests
dotnet test tests/Espada.Tests.Integration
dotnet test tests/Espada.Protocol.Mcp.Tests
dotnet test tests/Espada.Protocol.Sync.Tests
dotnet test tests/Espada.Billing.Tests
```

Frontend commands must run from `src/Espada.Web`:

```powershell
cd src/Espada.Web

npm install
npm run dev
npm run lint
npm run test
npm run build
npm run format:check
```

Use `npm run lint:fix` or `npm run format` only when intentionally rewriting frontend files.

## MCP access

Espada exposes instructions, memory, skills, policies, sessions, and other agent-facing capabilities through MCP.

Start the complete stack through Aspire:

```powershell
dotnet run --project src/Espada.AppHost/Espada.AppHost.csproj
```

Or start only the local daemon:

```powershell
dotnet run --project src/Espada.Daemon/Espada.Daemon.csproj
```

Default local MCP endpoint:

```text
HTTP: http://127.0.0.1:7432/mcp
```

The CLI can also expose a stdio bridge:

```powershell
espada mcp stdio
```

Initial MCP tools may include:

```text
context.resolve
context.explain

artifact.get
artifact.search

memory.search
memory.record
memory.supersede

skill.search
skill.load

policy.check

session.append
session.commit
```

Espada does not expose unrestricted SQL or filesystem execution through MCP.

Agents interact with domain-level operations so that authorization, validation, synchronization, audit history, and usage limits remain enforceable.

Detailed setup and JSON-RPC examples:

[docs/mcp-server.md](docs/mcp-server.md)

## Agent integration

Espada can launch supported coding agents with the correct local context and MCP configuration.

```powershell
espada run codex
espada run claude
espada run gemini
```

A typical launch flow:

1. Detect the current Git repository.
2. Determine the branch, remote, and current directory.
3. Start or connect to the local daemon.
4. Create an Espada session.
5. Resolve applicable instructions, policies, memories, and skills.
6. Configure the local MCP bridge.
7. Generate temporary compatibility files when required by the agent.
8. Launch the native agent CLI.
9. Record session events.
10. Remove temporary generated files.
11. Queue cloud synchronization when enabled.

Generated Markdown files are compatibility artifacts and are not the canonical Espada source of truth.

## Synchronization

Synchronization is optional and disabled by default.

MCP and synchronization are separate protocols:

```text
MCP          Agent ↔ Espada
Sync API     Local Espada ↔ Espada Cloud
```

Enable cloud synchronization:

```powershell
espada login
espada workspace link my-workspace
espada sync enable
```

Inspect synchronization status:

```powershell
espada sync status
```

Run synchronization explicitly:

```powershell
espada sync push
espada sync pull
```

Espada synchronization is designed to be:

* Offline-first.
* Idempotent.
* Retry-safe.
* Resumable.
* Conflict-aware.
* Versioned.

Local agent work must continue when Espada Cloud is unavailable.

## Cloud plans

Espada Community is free and open source.

Espada Cloud is a paid managed service because storage, compute, traffic, backups, telemetry, support, and managed infrastructure have ongoing costs.

Expected plan families:

| Plan       | Intended use                                                        |
| ---------- | ------------------------------------------------------------------- |
| Community  | Local and self-hosted usage                                         |
| Solo       | Individual developers requiring basic synchronization               |
| Pro        | Power users with larger storage and history                         |
| Team       | Shared workspaces, RBAC, and team policies                          |
| Business   | SSO, SCIM, enforcement, audit retention, and SLA                    |
| Enterprise | Dedicated infrastructure, private networking, and custom agreements |

Managed AI usage is not unlimited.

Supported models may use:

* Local inference.
* Bring your own API key.
* Prepaid Espada AI credits.

Plan limits and prices must be defined through versioned billing configuration rather than hard-coded across the application.

## Local and cloud storage

### Local

```text
External PostgreSQL with pgvector
Filesystem blob storage
PostgreSQL full-text search
Local event journal
```

### Cloud

```text
PostgreSQL
Azure Blob Storage
Multi-tenant workspace isolation
Cloud synchronization journal
Managed backups
```

Local mode must never require Azure Blob Storage or PostgreSQL.

For local cloud development, use Azurite and a local PostgreSQL container through Aspire.

## Context resolution

Espada resolves context using selectors such as:

```text
Workspace
Repository
Branch
Directory
Agent
Task
User
Device
Token budget
```

Resolved context may include:

* Security policies.
* Organization policies.
* Workspace instructions.
* Repository instructions.
* Path-specific instructions.
* Agent-specific overrides.
* Skills.
* Relevant memory.
* Allowed tools.

Use `Context Explain` to inspect why an item was included, excluded, overridden, or removed because of the token budget.

```powershell
espada context explain
```

Context resolution must be deterministic for policies and scope precedence.

Semantic search must not decide whether mandatory security policies apply.

## Structured artifacts

Espada stores canonical data as structured artifacts.

Artifact kinds may include:

```text
instruction
policy
memory
skill
plugin
tool
prompt
template
document
configuration
```

Example instruction:

```json
{
  "kind": "instruction",
  "title": "Database migrations",
  "rules": [
    {
      "level": "must",
      "text": "Every migration must include a rollback strategy."
    }
  ]
}
```

Markdown files may be imported and generated, but they are not the primary storage format.

## Optional design tooling

External MCP servers may be used for design and browser workflows when relevant.

Keep optional development tools separate from the core Espada runtime.

Do not require external design tooling to:

* Build Espada.
* Run the local daemon.
* Use the CLI.
* Connect coding agents.
* Run backend or frontend tests.

Document optional integrations under `docs/integrations`.

## Style and engineering guidelines

Use these files as the source of truth before changing code:

| Guide                                              | Use for                                                                                                                                                           |
| -------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [AGENTS.md](AGENTS.md)                             | Repository operating rules, local-first constraints, architecture boundaries, MCP and sync workflows, billing rules, verification requirements, and PR standards. |
| [.editorconfig](.editorconfig)                     | C# formatting, nullable behavior, indentation, and editor settings.                                                                                               |
| [DESIGN.md](DESIGN.md)                             | Visual design system, component guidance, responsive behavior, and UX standards.                                                                                  |
| [docs/onboarding_en.html](docs/onboarding_en.html) | Detailed contributor onboarding in English.                                                                                                                       |
| [docs/onboarding_ru.html](docs/onboarding_ru.html) | Detailed contributor onboarding in Russian.                                                                                                                       |
| [docs/mcp-server.md](docs/mcp-server.md)           | MCP transports, client configuration, tools, schemas, and testing.                                                                                                |
| [docs/sync-protocol.html](docs/sync-protocol.html) | Synchronization events, cursors, retries, conflicts, and compatibility.                                                                                           |
| [docs/billing.html](docs/billing.html)             | Plans, entitlements, usage accounting, trials, and payment lifecycle.                                                                                             |

Backend code follows explicit domain and application boundaries:

* Domain logic stays in `Espada.Domain`.
* Use-case orchestration stays in `Espada.Application`.
* Persistence and external services remain infrastructure details.
* MCP and HTTP handlers remain thin protocol adapters.
* The PostgreSQL adapter serves both external local deployments and managed cloud deployments.
* Sync contracts are versioned.
* Public contracts remain explicit.

Frontend code follows the project structure under `src/Espada.Web/src`.

## Security

Never commit or expose:

* API keys.
* Passwords.
* Database credentials.
* Azure Storage account keys.
* SAS tokens.
* JWTs.
* Refresh tokens.
* OAuth client secrets.
* Payment provider secrets.
* Webhook signing secrets.
* Production connection strings.
* Private customer data.

Use:

* Environment variables.
* .NET user secrets.
* Managed Identity in Azure.
* Existing secret managers.
* Placeholder values in documentation.

Do not log:

* Full prompts.
* Full memory contents.
* Full instructions.
* Source code.
* Access tokens.
* Connection strings.
* Signed Blob Storage URLs.
* Sensitive MCP payloads.

## Observability

Espada uses OpenTelemetry for traces, metrics, and logs.

Important operations include:

```text
espada.context.resolve
espada.context.explain
espada.memory.search
espada.skill.load
espada.policy.check
espada.session.commit
espada.sync.push
espada.sync.pull
espada.blob.upload
espada.mcp.tool.call
espada.billing.entitlement.check
```

Telemetry must help diagnose failures without becoming:

* A source of sensitive-data leakage.
* An uncontrolled cloud-cost center.
* A high-cardinality metrics problem.

## Maintenance

```powershell
# Update Aspire CLI
aspire update --self

# Restore .NET tools
dotnet tool restore

# Restore and verify .NET dependencies
dotnet restore
dotnet build

# Check outdated NuGet packages when performing dependency maintenance
dotnet list package --outdated

# Install frontend dependencies
cd src/Espada.Web
npm install
```

Dependency upgrades should be scoped and verified.

Do not upgrade all dependencies during unrelated feature work.

## Contributing checklist

Before opening a pull request:

* Keep the diff scoped to the requested behavior.
* Add or update tests for changed behavior.
* Run the smallest meaningful validation first.
* Widen validation only when necessary.
* Confirm whether local mode, cloud mode, or both were affected.
* Document changes to public contracts.
* Add migrations for persisted-data changes.
* Check synchronization compatibility.
* Check billing and entitlement implications.
* Verify generated compatibility output when artifact models change.
* Do not commit secrets, tokens, generated credentials, local databases, or private customer data.
* Do not claim tests passed unless they were actually run.
