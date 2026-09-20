# 02.01-audit-advisories: Audit runtime advisories

## Objective
Confirm runtime/package advisories, affected projects, and available patched versions.

## Scope Inventory
- Projects: CarvedRock.Api, CarvedRock.Data, CarvedRock.Domain, tests
- Packages to audit: Azure.Identity, Microsoft.Identity.Client, System.IdentityModel.Tokens.Jwt, SQLitePCLRaw.lib.e_sqlite3, OpenTelemetry.Api, NuGet.Packaging, NuGet.Protocol, Npgsql (already handled)

## Done when
- A remediation table exists listing current version, advisory link, candidate patched version, and per-project impact.
- tasks/02.01-audit-advisories/progress-details.md created with findings.
