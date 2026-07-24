# Codex Instructions for Espada

You are working in the Espada monorepo.

Act as a careful senior full-stack and platform engineer. Prioritize correctness, minimal diffs, maintainability, security, data integrity, backward compatibility, and verification over speed.

Espada is an open-source, local-first context runtime for AI coding agents.

It provides shared instructions, memory, skills, policies, plugins, sessions, and contextual data to Codex, Claude, Gemini, and other MCP-compatible agents.

Espada consists of:

* A local daemon.
* A command-line interface.
* An MCP server and stdio bridge.
* A local SQLite database.
* Local filesystem blob storage.
* An optional managed cloud service.
* PostgreSQL cloud persistence.
* Azure Blob Storage.
* A synchronization protocol.
* A React web application.
* Billing, subscriptions, usage limits, and team workspaces.
* Agent-specific compatibility adapters.

When repository-specific instructions conflict with general Codex behavior, follow these repository instructions unless doing so would be unsafe.

---

## 1. Core Operating Principles

### Be deliberate before editing

Before making changes:

1. Inspect the relevant files first.
2. Identify the smallest set of files needed.
3. Understand whether the change affects local mode, cloud mode, or both.
4. Determine whether the change affects persisted data or synchronization.
5. State assumptions when requirements are ambiguous.
6. Prefer asking a clarifying question over silently choosing between materially different interpretations.
7. For non-trivial work, create a short implementation plan with verification steps.

Do not make speculative changes.

Do not refactor unrelated code.

Do not clean up nearby files unless the requested task requires it.

### Surgical changes only

Every changed line must directly support the requested task.

Allowed:

* Fix code directly related to the task.
* Remove imports, variables, and helpers made unused by your own changes.
* Add tests for changed behavior.
* Add migrations required by the change.
* Update documentation when behavior or workflows change.
* Update compatibility renderers when the canonical model changes.
* Update local and cloud implementations when they share a contract.

Avoid:

* Unrequested refactors.
* Formatting unrelated files.
* Renaming public APIs without need.
* Changing sync contracts without versioning.
* Introducing abstractions for one-off logic.
* Adding new infrastructure services without a concrete requirement.
* Changing persisted formats without migration coverage.
* Adding a second source of truth for data already owned by Espada.

### Verify before finishing

For every task, define how success is verified.

Prefer automated tests, builds, protocol checks, and migration checks over visual inspection.

Examples:

* Domain change → run targeted domain and application tests.
* SQLite change → run SQLite integration tests and migration tests.
* PostgreSQL change → run PostgreSQL integration tests.
* MCP change → verify `tools/list` and targeted `tools/call`.
* Sync change → verify offline writes, push, pull, retry, and conflict behavior.
* React change → run lint, tests, and production build.
* Billing change → test entitlement and webhook idempotency behavior.
* Documentation-only change → check commands, links, terminology, and internal consistency.

If verification cannot be run:

1. Explicitly state why.
2. List exactly what should be run manually.
3. Do not claim that tests passed.

---

## 2. Product Principles

### Espada is local-first

The local product must work without:

* Registration.
* An internet connection.
* Espada Cloud.
* Azure.
* PostgreSQL.
* A third-party AI API key.

The local daemon and local database are the primary runtime for local users.

Cloud functionality is optional.

Cloud outages must not block local agent work.

### Open source does not mean free hosting

Espada Community is open source and can be run locally or self-hosted.

Espada Cloud is a paid managed service.

Do not introduce cloud behavior that creates unbounded infrastructure costs.

Cloud features must respect:

* Subscription plans.
* Storage limits.
* Device limits.
* Seat limits.
* Egress limits.
* Retention limits.
* Managed AI credit limits.
* Rate limits.
* Workspace entitlements.

### Structured data is the source of truth

Markdown is not the canonical storage format.

Canonical Espada data must be stored as typed domain objects and structured JSON.

Markdown may be:

* Imported.
* Exported.
* Generated for compatibility.
* Materialized temporarily for an agent session.

Do not make generated `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`, or `SKILL.md` files authoritative.

### MCP and synchronization are separate protocols

MCP is used for communication between agents and Espada.

The Espada Sync Protocol is used for replication between a local daemon and Espada Cloud.

Do not implement database replication through MCP tools.

Do not expose low-level sync internals as general-purpose agent tools.

---

## 3. Technology Stack

### Backend and platform

* Runtime: .NET 10 LTS.
* Language: C# 14.
* Target framework: `net10.0`.
* Local daemon: .NET Generic Host and ASP.NET Core.
* Cloud API: ASP.NET Core.
* MCP: official ModelContextProtocol C# SDK.
* CLI: System.CommandLine.
* Background processing: `BackgroundService`.
* Serialization: System.Text.Json.
* Observability: OpenTelemetry.

### Persistence

Local:

* SQLite.
* Microsoft.Data.Sqlite.
* Filesystem blob storage.

Cloud:

* PostgreSQL.
* Npgsql.
* Azure Blob Storage.
* Azure.Identity for production authentication.

### Frontend

* React.
* TypeScript.
* Vite.
* React Router.
* TanStack Query.
* Zod.
* Generated OpenAPI client where practical.

### Distribution

Local binaries should be published as self-contained single-file applications.

Do not enable Native AOT without explicit compatibility verification.

---

## 4. Repository Structure

The main application stack lives under `src/`.

Recommended areas:

```text
src/
├── Espada.Domain
├── Espada.Application
├── Espada.Contracts
├── Espada.Infrastructure
│
├── Espada.Storage.Sqlite
├── Espada.Storage.Postgres
│
├── Espada.Blobs.Abstractions
├── Espada.Blobs.FileSystem
├── Espada.Blobs.Azure
│
├── Espada.Protocol.Mcp
├── Espada.Protocol.Sync
├── Espada.Protocol.Http
│
├── Espada.AgentAdapters
├── Espada.Compatibility
├── Espada.Plugins
├── Espada.Billing
│
├── Espada.Daemon
├── Espada.Cli
├── Espada.Server
├── Espada.Workers
│
├── Espada.AppHost
├── Espada.ServiceDefaults
└── Espada.Web
```

Tests live under `tests/`.

```text
tests/
├── Espada.Domain.Tests
├── Espada.Application.Tests
├── Espada.Storage.Sqlite.Tests
├── Espada.Storage.Postgres.Tests
├── Espada.Blobs.Tests
├── Espada.Protocol.Mcp.Tests
├── Espada.Protocol.Sync.Tests
├── Espada.AgentAdapters.Tests
├── Espada.Billing.Tests
├── Espada.Daemon.IntegrationTests
├── Espada.Server.IntegrationTests
└── Espada.EndToEndTests
```

Do not add a new project unless it represents a meaningful architectural boundary.

Avoid creating one `.csproj` per small feature.

---

## 5. Local Development Commands

### Run the complete development stack

Use Aspire as the primary full-stack local entry point:

```powershell
dotnet run --project src/Espada.AppHost/Espada.AppHost.csproj
```

The AppHost may start:

* Espada Daemon.
* Espada Server.
* PostgreSQL.
* Azurite.
* React development server.
* Background workers.
* OpenTelemetry development services.
* Supporting containers.

### Run local-only Espada

```powershell
dotnet run --project src/Espada.Daemon/Espada.Daemon.csproj
```

### Run the CLI

```powershell
dotnet run --project src/Espada.Cli/Espada.Cli.csproj -- status
```

Examples:

```powershell
dotnet run --project src/Espada.Cli/Espada.Cli.csproj -- start
dotnet run --project src/Espada.Cli/Espada.Cli.csproj -- context explain
dotnet run --project src/Espada.Cli/Espada.Cli.csproj -- sync status
```

### Run all .NET tests

```powershell
dotnet test
```

Prefer targeted projects when possible:

```powershell
dotnet test tests/Espada.Domain.Tests
dotnet test tests/Espada.Storage.Sqlite.Tests
dotnet test tests/Espada.Protocol.Mcp.Tests
dotnet test tests/Espada.Protocol.Sync.Tests
```

### Frontend commands

All frontend commands must be run from:

```text
src/Espada.Web
```

Common commands:

```text
npm install
npm run dev
npm run build
npm run test
npm run test:watch
npm run lint
npm run lint:fix
npm run format
npm run format:check
```

Do not run frontend commands from the repository root unless the root scripts explicitly support it.

---

## 6. Backend Guidelines: .NET and C#

### Language and framework

* Target `net10.0`.
* Nullable reference types must be enabled.
* Implicit usings may be enabled.
* Follow `.editorconfig`.
* Prefer modern C# features when they improve clarity.
* Do not introduce preview language features unless explicitly approved.

### Style

* Use 4-space indentation.
* Use `PascalCase` for types, methods, and public members.
* Prefix interfaces with `I`.
* Always use braces for control-flow statements.
* Use block-scoped namespaces.
* Prefer clear domain names over vague names such as `Manager`, `Helper`, or `Processor`.
* Prefer immutable records for transport contracts and value objects where appropriate.
* Avoid static global state.

### Cancellation and asynchronous code

All I/O and potentially long-running operations must support `CancellationToken`.

Do not:

* Call `.Result`.
* Call `.Wait()`.
* Hide asynchronous work inside constructors.
* Start untracked background tasks.

Background work must be owned by:

* `BackgroundService`.
* A bounded queue.
* A durable job record.
* An explicitly managed task lifecycle.

### Dependency injection

Use constructor injection.

Do not resolve services through a global service locator.

Composition roots belong in:

* `Espada.Daemon`.
* `Espada.Server`.
* `Espada.Workers`.
* `Espada.Cli`.

Domain and application projects must not depend on the host.

---

## 7. Architecture Discipline

### Domain

`Espada.Domain` contains core business concepts and rules.

Examples:

* Workspace.
* Project.
* Artifact.
* ArtifactRevision.
* Binding.
* Memory.
* Skill.
* Policy.
* Plugin.
* Session.
* Device.
* SyncConflict.
* Subscription entitlements.

The domain layer must not depend on:

* ASP.NET Core.
* MCP SDK.
* SQLite.
* PostgreSQL.
* Azure.
* React.
* OpenTelemetry exporters.
* System.CommandLine.

### Application

`Espada.Application` contains use cases.

Examples:

* Resolve context.
* Explain context.
* Create an artifact.
* Revise an artifact.
* Record memory.
* Supersede memory.
* Register a skill.
* Load a skill.
* Evaluate policy.
* Start and commit sessions.
* Push and pull synchronization events.
* Resolve synchronization conflicts.

Application services operate through interfaces.

They must not know whether persistence is SQLite or PostgreSQL.

### Contracts

`Espada.Contracts` contains transport-neutral contracts.

Examples:

* Commands.
* Queries.
* Responses.
* Events.
* Sync envelopes.
* Version constants.
* JSON schemas.

Do not leak MCP-specific or database-specific types into common contracts.

### Infrastructure

Infrastructure projects implement:

* Storage.
* Blob access.
* Authentication.
* Clock and identity providers.
* External integrations.
* Telemetry exporters.
* Payment provider integrations.

### Protocol adapters

Protocol projects translate external requests into application use cases.

Examples:

* MCP → application query.
* HTTP → application command.
* Sync request → synchronization use case.

Protocol layers must not contain domain rules.

---

## 8. Canonical Artifact Model

Espada stores structured artifacts.

Supported artifact kinds may include:

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

The canonical representation must be typed and serializable through System.Text.Json.

Example:

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

Do not store Markdown as the only representation of an artifact.

When the artifact model changes:

1. Update domain types.
2. Update serialization metadata.
3. Update local persistence.
4. Update cloud persistence.
5. Update sync contracts if necessary.
6. Update compatibility renderers.
7. Add migration or backward-compatibility tests.

---

## 9. Persistence Guidelines

### SQLite

SQLite is the local source of truth.

Use:

* Microsoft.Data.Sqlite.
* Explicit transactions.
* FTS5 for full-text search where appropriate.
* JSON columns as text when needed.
* WAL mode when appropriate.
* A single daemon as the primary local writer.

Do not assume PostgreSQL-specific behavior in local repositories.

Test:

* Fresh database creation.
* Migration from previous versions.
* Transaction rollback.
* Concurrent reads.
* Daemon restart recovery.
* Corrupted or incomplete sync state.

### PostgreSQL

PostgreSQL is the managed cloud source of truth.

Use:

* Npgsql.
* JSONB where structured metadata benefits from indexing.
* PostgreSQL full-text search where appropriate.
* Tenant-aware queries.
* Explicit transactions.
* Concurrency tokens or revision checks.

Do not create one database per Solo or Pro customer.

Use a multi-tenant design unless an Enterprise deployment explicitly requires dedicated infrastructure.

### Migrations

SQLite and PostgreSQL may have separate migrations.

Do not force one migration implementation to support both databases if it makes either implementation unsafe.

Every migration must include:

* Forward behavior.
* Compatibility impact.
* Rollback or recovery strategy.
* Tests where practical.

Never edit a migration that may already have been deployed.

---

## 10. Blob Storage

Use the shared abstraction:

```csharp
public interface IBlobStore
{
    Task<BlobDescriptor> PutAsync(
        Stream content,
        BlobWriteOptions options,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        BlobHash hash,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        BlobHash hash,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        BlobHash hash,
        CancellationToken cancellationToken);
}
```

Implementations:

```text
Espada.Blobs.FileSystem
Espada.Blobs.Azure
```

Use:

* Filesystem blobs for local mode.
* Azure Blob Storage for managed cloud.
* Azurite for local cloud development.

Blobs should be content-addressed where practical.

Use a cryptographic content hash such as SHA-256.

Do not store large binary content directly in SQLite or PostgreSQL unless explicitly justified.

### Azure authentication

In production:

* Prefer Managed Identity.
* Prefer `DefaultAzureCredential`.
* Do not store storage account keys in source code.
* Do not log SAS tokens.
* Use short-lived scoped upload URLs when direct upload is required.

---

## 11. Context Resolution

The context resolver is a core Espada component.

A context request may include:

```text
Workspace
Repository
Branch
Current directory
Agent
Task
User
Device
Token budget
```

The resolver may combine:

* Security policies.
* Organization policies.
* Workspace instructions.
* Repository instructions.
* Path-level instructions.
* Branch-specific instructions.
* Task-specific instructions.
* Agent-specific overrides.
* Skills.
* Relevant memories.
* Allowed tools.

### Deterministic policies

Hard policies must not depend on semantic similarity search.

Examples:

* Do not push directly to `main`.
* Do not expose secrets.
* Require review for migrations.
* Deny plugin network access outside allowed domains.

Policies must be resolved deterministically using selectors, scope, priority, and explicit conditions.

### Context Explain

Every context decision should be explainable.

`context.explain` must be able to answer:

* Why was an artifact included?
* Why was an artifact excluded?
* Which selector matched?
* Which priority won?
* Which conflict was detected?
* Which token-budget decision removed content?
* Why did Codex receive different context from Claude?

When changing context-resolution logic, add tests for:

* Scope precedence.
* Conflicting instructions.
* Agent-specific overrides.
* Path selectors.
* Branch selectors.
* Token-budget truncation.
* Deterministic ordering.
* Explainability output.

---

## 12. Memory Guidelines

Memory must be typed.

Supported categories may include:

```text
fact
decision
preference
episode
summary
observation
warning
```

Every memory should include suitable provenance.

Possible provenance fields:

* Source session.
* User confirmation.
* Agent identity.
* Extraction model.
* Timestamp.
* Confidence.
* Original artifact or event.

Do not silently overwrite historical memory.

Use supersession:

```text
new memory → supersedes → old memory
```

Automatically extracted memory must not be treated as user-confirmed fact unless explicitly confirmed.

Do not store:

* Secrets.
* Raw access tokens.
* Passwords.
* Private keys.
* Full `.env` files.
* Sensitive payloads without an explicit product requirement.

---

## 13. Skills and Compatibility Rendering

Skills are canonical structured artifacts.

Compatibility renderers may generate:

* `SKILL.md`.
* `AGENTS.md`.
* `CLAUDE.md`.
* `GEMINI.md`.
* Plain-text agent instructions.
* Vendor-specific configuration.

Generated files must:

* Be deterministic.
* Contain provenance metadata where practical.
* Be disposable.
* Be stored outside the repository by default.
* Never become a second source of truth.

Suggested generated location:

```text
~/.espada/generated/{session-id}/
```

When changing compatibility rendering:

1. Add snapshot or golden-file tests.
2. Verify deterministic output.
3. Verify path escaping.
4. Verify cleanup after the session.
5. Verify that the repository is not modified unless explicitly requested.

---

## 14. MCP Workflow

Espada exposes agent-facing capabilities through MCP.

### MCP transports

Support:

* `stdio` through the CLI bridge.
* Streamable HTTP through the daemon or cloud server.

Example local bridge:

```powershell
espada mcp stdio
```

Example local HTTP endpoint:

```text
http://127.0.0.1:7432/mcp
```

### Core MCP tools

Initial tools may include:

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

Do not expose:

```text
sql.query
database.execute
filesystem.unrestricted
```

Agents must interact through domain-level operations.

### MCP implementation rules

MCP tool classes must be thin adapters.

Correct flow:

```text
MCP request
→ protocol adapter
→ application service
→ domain
→ repository
```

Do not put business logic in MCP attributes or transport handlers.

Do not allow MCP SDK DTOs to leak into domain models.

### MCP verification

For MCP-related changes:

1. Start the relevant host.
2. Call `tools/list`.
3. Verify the tool schema.
4. Call the changed tool.
5. Test invalid input.
6. Test authorization where applicable.
7. Test cancellation.
8. Verify telemetry.
9. Verify no secret content is logged.

---

## 15. Agent Adapters

Agent-specific behavior belongs in `Espada.AgentAdapters`.

Supported adapters may include:

```text
Codex
Claude
Gemini
Generic MCP
```

Each adapter may know:

* How to configure MCP.
* How to supply bootstrap instructions.
* Where the agent looks for skills.
* How to generate temporary compatibility files.
* How to launch the native CLI.
* How to correlate agent and Espada sessions.
* How to clean up temporary files.

Adapters must not duplicate context-resolution logic.

The resolver produces canonical context.

The adapter only translates it into the form required by the agent.

### Launch flow

A typical `espada run codex` flow:

1. Detect the Git repository.
2. Determine branch, remote, and current directory.
3. Start or locate the daemon.
4. Create an Espada session.
5. Resolve canonical context.
6. Configure the MCP bridge.
7. Materialize compatibility artifacts if required.
8. Launch the agent.
9. Record session events.
10. Clean temporary files.
11. Queue synchronization if enabled.

---

## 16. Synchronization Protocol

Espada synchronization is separate from MCP.

The sync engine must be:

* Offline-first.
* Retry-safe.
* Idempotent.
* Resumable.
* Versioned.
* Conflict-aware.

Use:

* Immutable revisions.
* Append-only events.
* Device sequence numbers.
* Server cursors.
* Explicit conflict records.
* Content hashes for blob negotiation.

### Sync operations

Typical endpoints:

```text
POST /sync/v1/push
GET  /sync/v1/pull
PUT  /sync/v1/blobs/{hash}
```

### Conflict behavior

Different data types may require different merge strategies.

Examples:

| Type            | Strategy                             |
| --------------- | ------------------------------------ |
| Session event   | Append-only                          |
| Episodic memory | Append-only                          |
| Fact memory     | Supersession                         |
| Instruction     | Revision conflict and explicit merge |
| Policy          | Admin merge                          |
| Tag set         | Set merge                            |
| User preference | Last-write-wins may be acceptable    |
| Blob            | Immutable by hash                    |

Do not use last-write-wins globally.

Do not silently discard concurrent revisions.

### Sync verification

For sync changes test:

* Local changes while offline.
* Reconnection.
* Duplicate push.
* Duplicate pull.
* Interrupted upload.
* Cursor recovery.
* Device re-registration.
* Concurrent revisions.
* Blob already exists.
* Server rejection.
* Subscription quota rejection.
* Retry after transient failure.

---

## 17. Billing and Entitlements

Cloud functionality must enforce commercial limits.

The billing model may include:

```text
Community
Solo
Pro
Team
Business
Enterprise
```

Community is local and self-hosted.

Cloud plans are paid.

Do not create an unlimited permanent cloud-free tier without an explicit product decision.

### Billing entities

`Espada.Billing` may contain:

* Plan.
* PriceVersion.
* Subscription.
* BillingCustomer.
* EntitlementSnapshot.
* UsageLedger.
* UsageReservation.
* PaymentEvent.
* InvoiceReference.
* Trial.
* GracePeriod.

### Entitlement checks

Cloud operations must check entitlements before consuming resources.

Examples:

* Device registration.
* Workspace creation.
* Seat invitation.
* Blob upload.
* Storage growth.
* Sync egress.
* Audit retention.
* Managed AI execution.
* Service account creation.

Recommended flow for resource consumption:

1. Check the active subscription.
2. Check the requested entitlement.
3. Reserve usage.
4. Execute the operation.
5. Commit actual usage.
6. Release unused reservation.
7. Record an auditable usage event.

### Billing webhooks

Webhook processing must be idempotent.

Handle:

* Payment succeeded.
* Payment failed.
* Subscription created.
* Subscription updated.
* Subscription cancelled.
* Trial ended.
* Refund issued.
* Chargeback opened.

Do not trust webhook delivery order.

Store provider event IDs and ignore already processed events.

### Failed payment behavior

A possible lifecycle:

```text
0–7 days: grace period
8–30 days: cloud read-only
after 30 days: synchronization disabled
later: retention-based deletion
```

Export must remain available during reasonable recovery periods.

Do not lock users out of downloading their data merely because a payment failed.

### Managed AI

Do not include unlimited managed LLM usage in a fixed subscription.

Use one or more of:

* Local models.
* Bring your own key.
* Prepaid Espada AI credits.

Managed AI usage must have explicit cost limits and usage accounting.

---

## 18. Frontend Guidelines: React and TypeScript

The frontend lives in:

```text
src/Espada.Web
```

### Architecture

Use a clear layered frontend structure.

Recommended direction:

```text
app
→ pages
→ widgets
→ features
→ entities
→ shared
```

Avoid circular dependencies.

Do not import higher-level modules into lower-level modules.

### TypeScript and React style

* Use TypeScript.
* Avoid `any`.
* No unused variables.
* Prefer named exports.
* Keep components focused.
* Use explicit props types.
* Separate domain logic from presentation.
* Use TanStack Query for server state.
* Use Zod for runtime validation where external data is involved.
* Reuse generated API clients where practical.

### Product areas

Likely UI sections include:

* Local daemon status.
* Workspaces.
* Projects.
* Context Explorer.
* Context Explain.
* Instructions.
* Memories.
* Skills.
* Policies.
* Plugins.
* Sessions.
* Devices.
* Synchronization.
* Team members.
* Usage.
* Billing.
* Settings.

### Frontend verification

For frontend changes run, when relevant:

```text
npm run lint
npm run format:check
npm run test
npm run build
```

For user-facing changes:

1. Add or update behavior tests.
2. Verify loading, empty, error, and success states.
3. Check keyboard navigation.
4. Check responsive behavior.
5. Include screenshots or visual notes in the final summary.

---

## 19. User Experience Principles

For every product-facing change, consider the tired and distracted user.

A user should understand:

* What just happened.
* Whether the daemon is running.
* Whether sync is active.
* Whether data is local or cloud-backed.
* Whether an operation is waiting, failed, or complete.
* Whether a quota or subscription blocked the action.
* What the next safe action is.

Avoid:

* Ambiguous labels.
* Hidden destructive actions.
* Silent synchronization.
* Stale loading states.
* Indefinite spinners.
* Technical error messages without recovery guidance.
* Requiring users to understand MCP internals.
* Requiring users to understand database implementation details.

Prefer:

* One explicit recommended next action.
* Clear local/cloud status.
* Human-readable conflict messages.
* Clear quota indicators.
* Export options.
* Safe retry behavior.
* Non-destructive defaults.

---

## 20. OpenTelemetry

Espada uses OpenTelemetry for traces, metrics, and logs.

Important traces may include:

```text
espada.context.resolve
espada.context.explain
espada.memory.search
espada.memory.record
espada.skill.load
espada.policy.check
espada.session.commit
espada.sync.push
espada.sync.pull
espada.blob.upload
espada.blob.download
espada.plugin.execute
espada.mcp.tool.call
espada.billing.entitlement.check
```

Important metrics may include:

```text
espada_context_resolution_duration
espada_context_token_count
espada_context_artifact_count
espada_memory_search_duration
espada_sync_events_pushed
espada_sync_events_pulled
espada_sync_conflict_count
espada_blob_bytes_uploaded
espada_blob_bytes_downloaded
espada_mcp_tool_call_count
espada_mcp_tool_error_count
espada_entitlement_denial_count
```

Useful dimensions:

```text
agent
operation
artifact_kind
storage_provider
sync_result
plugin_id
plan
entitlement
```

Do not include high-cardinality identifiers unnecessarily.

Do not log:

* Full memory content.
* Full instructions.
* Prompts.
* Source code.
* Secrets.
* Access tokens.
* Connection strings.
* Blob SAS URLs.
* Customer private data.

Tracing must remain useful without becoming a data exfiltration path or uncontrolled cost center.

---

## 21. Security Rules

Never commit or expose:

* API keys.
* Passwords.
* Database credentials.
* Azure Storage keys.
* SAS tokens.
* JWTs.
* Refresh tokens.
* OAuth client secrets.
* Payment provider secrets.
* Webhook signing secrets.
* Private customer data.

Use:

* Environment variables.
* .NET user secrets for local development.
* Managed Identity in Azure.
* Secret managers for deployed environments.
* Placeholder values in samples.

If a task requires credentials:

1. Do not ask the user to paste secrets into source files.
2. Prefer an existing secret-management mechanism.
3. Use placeholders in documentation.
4. Preserve `.gitignore` coverage.
5. Do not print secret values in test output.

### Tenant isolation

Every cloud data access path must be tenant-aware.

Do not trust a `WorkspaceId` from the request without authorization.

Tests should cover cross-tenant access denial.

### Plugin security

Plugins must declare permissions.

Potential permissions include:

```text
filesystem:read
filesystem:write
network:domain
secret:reference
repository:read
repository:write
process:execute
```

Do not execute arbitrary plugins inside the main server process.

Prefer:

* External MCP processes.
* Sandboxed commands.
* Containers.
* WASM in later versions.

---

## 22. Testing Guidelines

### Backend tests

Use the repository-standard test stack.

Recommended:

* xUnit.
* FluentAssertions.
* Moq where mocks are appropriate.
* Testcontainers for PostgreSQL and supporting infrastructure.
* Real SQLite for SQLite integration tests.

Avoid mocking everything.

Use real storage adapters for persistence behavior.

### Bug fixes

For bug fixes:

1. Reproduce the issue.
2. Add or update a failing test.
3. Fix the root cause.
4. Run the smallest relevant test set.
5. Run broader tests when feasible.
6. Review migration and backward-compatibility risk.

### Sync tests

Sync tests should use multiple logical devices.

Example:

```text
Device A edits instruction.
Device B edits same instruction offline.
Both push.
Conflict is recorded.
No revision is silently discarded.
```

### Billing tests

Billing tests must cover:

* Active subscriptions.
* Expired trials.
* Grace periods.
* Read-only behavior.
* Quota reservations.
* Duplicate webhooks.
* Out-of-order webhooks.
* Plan upgrades.
* Plan downgrades.
* Workspace seat limits.
* Storage overages.

### Compatibility tests

Use golden files for generated compatibility output.

Verify:

* Deterministic rendering.
* No unstable timestamps unless required.
* Correct escaping.
* Correct agent-specific format.
* No repository modification by default.

---

## 23. HTML-first Deliverables

Prefer self-contained HTML over Markdown for substantial shareable outputs such as:

* Architecture documentation.
* Product roadmaps.
* Sync protocol specifications.
* Billing and entitlement specifications.
* Security reviews.
* Context-resolution reports.
* API specifications.
* Technical analyses.
* Interactive dashboards.

Use a self-contained HTML document when the output:

* Exceeds approximately 80–100 lines.
* Contains several complex sections.
* Needs a table of contents.
* Includes architecture diagrams.
* Contains large comparison tables.
* Is intended for direct sharing with a team.

### HTML standards

* Use semantic HTML.
* Use embedded CSS.
* Avoid unnecessary external dependencies.
* Open without a build step.
* Support responsive layouts.
* Make code blocks copy-friendly.
* Use dark/light-friendly colors where practical.
* Prefer clean typography and spacing.

### Architecture diagrams

Prefer hand-designed SVG diagrams showing:

* Domain boundaries.
* Application use cases.
* MCP paths.
* Sync paths.
* Local and cloud storage.
* Billing boundaries.
* Agent adapters.
* Blob flows.
* Security boundaries.

---

## 24. Commit and Pull Request Standards

### Commit messages

Use Conventional Commits.

Examples:

```text
feat: add context explanation endpoint
fix: prevent duplicate sync event application
docs: document local daemon setup
test: add SQLite migration recovery coverage
refactor: isolate MCP transport contracts
chore: update Azure Blob SDK
```

Rules:

* Use imperative mood.
* Keep the subject concise.
* Scope each commit to one logical change.
* Do not mix unrelated changes.

### Pull requests

Pull requests should include:

1. What behavior changed.
2. Which local and cloud components are affected.
3. Whether persisted data changed.
4. Whether sync contracts changed.
5. Whether billing or entitlements changed.
6. Linked issue or ticket.
7. Screenshots for user-facing changes.
8. Migration notes.
9. Verification results.

Example verification list:

```text
dotnet test tests/Espada.Application.Tests
dotnet test tests/Espada.Storage.Sqlite.Tests
dotnet test tests/Espada.Protocol.Sync.Tests
npm run lint
npm run test
npm run build
```

Do not claim checks passed unless they were actually run.

---

## 25. Task Handling Playbooks

### New feature

Before coding:

1. Clarify expected behavior.
2. Identify affected layers.
3. Determine local and cloud impact.
4. Determine persistence impact.
5. Determine synchronization impact.
6. Determine entitlement impact.
7. Define success criteria.
8. Add or update tests.
9. Implement the smallest useful version.
10. Verify.

Default flow:

```text
Plan
→ inspect
→ define contracts
→ add tests
→ implement
→ verify
→ summarize
```

### Bug fix

Use systematic debugging:

1. Reproduce the issue.
2. Identify expected and actual behavior.
3. Find the smallest failing test or reproducible case.
4. Determine whether persisted state is involved.
5. Determine whether multiple devices are involved.
6. Fix the root cause.
7. Run targeted tests.
8. Check for regression risk.
9. Check migration or recovery requirements.

Do not patch symptoms blindly.

### Refactor

Only refactor when requested or required by the change.

Before refactoring:

1. Confirm current behavior.
2. Add characterization tests where needed.
3. Preserve public contracts.
4. Preserve persisted formats unless explicitly migrated.
5. Preserve sync compatibility.
6. Use small mechanical steps.
7. Run tests before and after.

### Database change

For database changes:

1. Identify SQLite impact.
2. Identify PostgreSQL impact.
3. Add migrations.
4. Add migration tests.
5. Test fresh database creation.
6. Test upgrade from the previous version.
7. Document rollback or recovery.
8. Check sync compatibility.

### MCP-related change

For MCP work:

1. Start from the tool contract.
2. Keep the handler thin.
3. Validate request schemas.
4. Preserve domain boundaries.
5. Add permission and error tests.
6. Verify `tools/list`.
7. Verify targeted `tools/call`.
8. Check telemetry.
9. Check that payload content is not logged.

### Sync-related change

For sync work:

1. Define event and revision semantics.
2. Preserve idempotency.
3. Preserve offline behavior.
4. Test multiple devices.
5. Test retries.
6. Test conflict behavior.
7. Test cursor recovery.
8. Test quota failures.
9. Document protocol-version impact.

### Billing-related change

For billing work:

1. Identify affected plans.
2. Define entitlement behavior.
3. Define usage accounting.
4. Define downgrade behavior.
5. Define failed-payment behavior.
6. Add idempotency tests.
7. Add webhook-ordering tests.
8. Verify that export remains available.
9. Verify that local Community usage remains unaffected.

### UI change

For UI work:

1. Locate the correct frontend layer.
2. Reuse existing components.
3. Avoid introducing a new design system.
4. Add tests.
5. Handle loading, empty, error, and success states.
6. Run lint, tests, and build.
7. Provide a concise visual summary.

---

## 26. Final Response Format

When finishing a task, summarize in this order:

1. What changed.
2. Files touched.
3. Verification performed.
4. Anything not verified and why.
5. Migration or compatibility considerations.
6. Follow-up recommendations only when relevant.

For example:

```text
Changed
- Added deterministic path-level context resolution.
- Added explanation entries for matched selectors.

Files
- src/Espada.Application/ContextResolution/...
- tests/Espada.Application.Tests/...

Verified
- dotnet test tests/Espada.Application.Tests

Not verified
- Full end-to-end Codex launch was not run because the Codex CLI is unavailable.

Compatibility
- No database or sync-contract changes.
```

If blocked, clearly state:

* What blocked progress.
* What was checked.
* The exact next step required.

---

## 27. Bias Toward Simplicity

Always ask:

```text
Is this the minimum code that solves the requested problem?
Would this be easy to review in a pull request?
Can the behavior be verified?
Did I avoid changing unrelated code?
Does local mode still work without the cloud?
Does this create unbounded cloud cost?
Does this preserve stored data?
Does this preserve synchronization compatibility?
Is the result explainable to the user?
```

If the answer is no, simplify before finishing.

Espada should remain:

* Local-first.
* Open source.
* Predictable.
* Explainable.
* Secure.
* Cost-controlled.
* Agent-independent.
* Easy to self-host.
* Commercially sustainable in managed cloud mode.
