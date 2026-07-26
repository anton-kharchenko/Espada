# Azure Deployment Plan

> **Status:** Deployment in progress

Generated: 2026-07-26T12:47:15+03:00

## 1. Goal and boundaries

Create and publish an English-only Espada landing page at `https://espada.website`, with GitHub as the primary call to action. Reuse TripRadar's React/Vite engineering quality, not its travel-domain content or branding.

This is a website-only deployment. It must not provision or modify the pending Espada API, PostgreSQL, Container Apps, ACR, Key Vault, migrations, or other cloud workloads already being developed in the current dirty branch.

## 2. Confirmed requirements

| Attribute | Value |
|---|---|
| Path | Add a new public frontend and isolated website deployment target |
| Classification | Production-facing landing page, no SLA requirement for v1 |
| Scale | Small, public static traffic |
| Budget | Cost-optimized |
| Data/compliance | Public static content only; no forms, cookies, analytics, authentication, or personal data |
| Subscription | Azure subscription 1 (`71ed459a-9eba-41b5-9afa-163fd37b5b47`) |
| Tenant | `3a7d1785-e650-4c95-9845-5781e0f479d9` |
| Location | `eastus2` |
| Canonical URL | `https://espada.website` |
| Alternate URL | `https://www.espada.website` |
| Registrar | Namecheap |
| Authoritative DNS | Move from Namecheap BasicDNS to Azure DNS; Namecheap remains registrar |

## 3. Components and recipe

| Component | Technology | Path / target |
|---|---|---|
| Landing page | React + TypeScript + Vite, focused components and plain CSS | `src/Espada.Web` |
| Infrastructure | Standalone .NET Pulumi program, isolated website project/stack | `src/Espada.Deployment.Website` |
| Continuous delivery | GitHub Actions with a scoped Static Web Apps deployment token | `.github/workflows/deploy-website.yml` |

**Recipe:** Pulumi Azure Native with a standalone .NET program. The separate project and stack avoid coupling to the pending API/data deployment work. Pulumi is explicitly required by the user, so no `azure.yaml`, Bicep, Terraform, or azd project is introduced.

## 4. Architecture

| Resource | Choice | Purpose |
|---|---|---|
| Resource group | `espada-website-prod` | Isolate the low-risk website from API/data infrastructure |
| Azure Static Web Apps | Free SKU | Global static hosting, managed HTTPS, two custom domains |
| Azure DNS public zone | `espada.website` | Pulumi-managed apex and `www` records |
| Pulumi project/stack | `espada-website` / `production` | Independent state and blast radius |

The Pulumi program provisions and updates only website resources and returns the Azure hostname plus DNS nameservers. The Vite build is uploaded with the latest pinned Static Web Apps CLI by passing the deployment token through an environment variable; the token is neither logged nor persisted.

DNS rollout is intentionally staged:

1. One Pulumi update provisions Static Web Apps, Azure DNS, apex/`www` routing, and asynchronous TXT-token custom-domain validation.
2. Replace Namecheap nameservers with the Pulumi outputs. Once Azure DNS is authoritative, Static Web Apps completes validation and managed TLS automatically; then verify both URLs and set the GitHub repository homepage.

## 5. Landing-page behavior

- Header: Espada wordmark, concise navigation anchors, GitHub button.
- Hero: the verified README positioning — open-source, local-first context runtime for AI coding agents.
- Value section: shared structured instructions, memory, skills, policies, plugins, and explainable context across Codex, Claude, Gemini, and MCP-compatible agents.
- Workflow section: store structured context, resolve it deterministically, deliver it through MCP.
- Quick start: only commands already present in the repository documentation; no unsupported download or release claims.
- Footer: GitHub, MIT license, security link, current year.
- Accessibility/quality: semantic landmarks, skip link, keyboard-visible focus, reduced-motion support, responsive layouts, SEO/Open Graph metadata, favicon, and no remote runtime dependencies.

## 6. Provisioning limits and cost guardrail

The selected subscription currently has zero resource groups/resources. Azure quota tooling reports these providers as quota-API unsupported/no-limit, so official Azure limits are used as the required fallback.

| Resource / limit | Planned | Total after | Official limit | Result |
|---|---:|---:|---:|---|
| Static Web Apps Free apps | 1 | 1 | 10 per subscription | Within limit |
| Static Web Apps custom domains | 2 | 2 | 2 per Free app | At intended v1 limit |
| Azure public DNS zones | 1 | 1 | 250 per subscription | Within limit |
| DNS record sets | 10 | 10 | 10,000 per zone | Within limit |

Expected Azure service cost for this website is limited to Azure DNS zone/query charges; Static Web Apps Free has no hosting charge. No paid monitoring, database, compute, storage, Front Door, or analytics resource is in scope.

## 7. Execution and validation

### Preparation

- [x] Inspect Espada and TripRadar frontend/deployment seams
- [x] Confirm subscription, tenant, region, domain, language, CTA, and DNS ownership model
- [x] Check regional/service limits and current subscription state
- [x] Select an isolated Pulumi website stack
- [x] User approved this plan

### Implementation

- [x] Add the minimal React/Vite landing page and deterministic production build
- [x] Add Pulumi Static Web Apps and Azure DNS resources without coupling to API deployment
- [x] Add a dedicated website deployment workflow with pinned Actions
- [x] Update this plan to `Ready for Validation`

### Validation proof

| Check | Command | Result | Timestamp |
|---|---|---|---|
| Frontend | `npm run lint`; `npm test`; `npm run build`; `npm audit` | Pass: 1 test, production build, 0 vulnerabilities | 2026-07-26T14:49:00+03:00 |
| Responsive UI | Playwright snapshots at 1440x1000 and 390x844 | Pass: semantic landmarks present; no horizontal overflow | 2026-07-26T14:38:00+03:00 |
| Pulumi program | `dotnet build Espada.Deployment.Website.csproj -c Release` | Pass: 0 warnings, 0 errors | 2026-07-26T14:49:32+03:00 |
| Infrastructure preview | `pulumi preview --diff --non-interactive` | Pass: 8 creates, 0 updates, 0 deletes; preview `618c28ea-1af0-4f96-927c-7d1367dc15e6` | 2026-07-26T15:12:00+03:00 |
| Pulumi deployment | `pulumi up --yes --skip-preview --non-interactive` | Pass: 8 created, 6 unchanged; update 3 | 2026-07-26T15:14:00+03:00 |
| Repository diff | `git diff --check` | Pass | 2026-07-26T14:49:32+03:00 |
| Azure-hostname smoke | HTTP 200, title, CSP at `orange-stone-06a09310f.7.azurestaticapps.net` | Pass | 2026-07-26T15:02:00+03:00 |
| Public smoke | HTTP, DNS, custom-domain status, managed TLS | Pending Namecheap nameserver switch | Pending |

### Deployment

- [x] Validate all generated artifacts and record proof
- [x] Run and inspect Pulumi preview; reject unexpected deletes/replacements
- [x] Deploy the bootstrap website stack and site content
- [ ] Switch Namecheap nameservers to Azure DNS
- [ ] Complete domain binding after DNS propagation
- [ ] Verify `https://espada.website` and `https://www.espada.website`
- [ ] Set GitHub About homepage URL
- [ ] Update this plan to `Deployed`

## 8. Files to generate or change

| Area | Purpose |
|---|---|
| `.azure/plan.md` | Approved execution and validation record |
| `src/Espada.Web` | Landing page, build config, tests, static hosting config |
| `src/Espada.Deployment.Website` | Isolated .NET Pulumi website project and production stack |
| `.github/workflows/deploy-website.yml` | Repeatable website-only production deployment |

## 9. Approval boundary

Approval authorizes creation/update of the new website resources, switching `espada.website` nameservers to Azure DNS, publishing the landing page, and setting the GitHub homepage. It does not authorize deploying the pending API/database stack, deleting resources, changing billing plans, or creating paid SKUs.

