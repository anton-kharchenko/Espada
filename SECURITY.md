# Security Policy

## Supported versions

Security updates are provided for the latest release on the default branch (`master`).

| Version | Supported |
| ------- | --------- |
| Latest on `master` | Yes |
| Older releases | Best effort |

## Reporting a vulnerability

Please **do not** open a public GitHub issue for security vulnerabilities.

### Preferred: private vulnerability reporting

Use GitHub's private vulnerability reporting for this repository:

1. Open the [Security](https://github.com/anton-kharchenko/Espada/security) tab.
2. Choose **Report a vulnerability**.
3. Describe the issue, impact, and steps to reproduce.

### Alternative: email

If private reporting is unavailable, email:

**anton.kharchenko.job@gmail.com**

Please include:

- A clear description of the vulnerability
- Affected components or endpoints
- Steps to reproduce or a proof of concept
- Potential impact (data exposure, privilege escalation, etc.)
- Whether you plan to disclose publicly and any preferred timeline

## Response expectations

- We aim to acknowledge reports within **72 hours**.
- We aim to provide an initial assessment within **7 days**.
- Coordinated disclosure is preferred. Please give us a reasonable window to fix and release before public disclosure.

## Scope notes

Espada is a local-first context runtime. Reports are especially welcome for:

- Secret or credential leakage in logs, telemetry, or generated agent files
- Cross-tenant data access in cloud/API paths
- Unsafe plugin or process execution boundaries
- Injection or authorization flaws in MCP/HTTP surfaces
- Dependency vulnerabilities with a practical exploit path

Out of scope (unless they create a security impact):

- Denial of service that requires local attacker privileges on the same machine as the daemon
- Issues that only affect intentionally insecure local development configuration

## Thank you

Responsible disclosure helps keep Espada safe for local and cloud users. We appreciate your help.
