# 02.01-audit-advisories: Progress details

## Summary
Collected runtime advisory findings and candidate patched versions. Recommendations below prioritize safe upgrades that preserve multi-targeting for libraries and single-target for CarvedRock.WebApp (per scenario preferences). No code changes were applied in this task — only audit evidence and recommendations are recorded.

## Remediation table
| Package | Current version(s) found | Advisory | Latest candidate version (NuGet) | Projects affected | Recommendation |
|---|---:|---|---:|---|---|
| System.IdentityModel.Tokens.Jwt | 6.24.0 / 7.0.3 (transitive) | GHSA-59j7-ghrg-fj52 https://github.com/advisories/GHSA-59j7-ghrg-fj52 | 8.23.0 | CarvedRock.Api, CarvedRock.Data, CarvedRock.Domain, tests | Update to >=8.x on net10.0. Evaluate net8 compatibility; multi-target libraries can keep older TFM or use binding redirect strategies for apps.
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.6 | GHSA-2m69-gcr7-jv3q https://github.com/advisories/GHSA-2m69-gcr7-jv3q | 3.53.3 (or latest 3.53.3) | CarvedRock.Data, CarvedRock.Domain, tests | Update to 3.53.3. This is a native/packing change; test locally on platforms used by CI. Consider pinning per-TFM if needed.
| Azure.Identity | 1.10.3 | GHSA-m5vv-6r4h-3vj9 and GHSA-wvxc-855f-jvrv https://github.com/advisories/GHSA-m5vv-6r4h-3vj9 | 1.21.0 | CarvedRock.Api (direct or transitive) | Upgrade to latest stable (1.21.0). Verify API compatibility; prefer single upgrade for apps, conservative approach for libraries.
| Microsoft.Identity.Client | 4.56.0 | GHSA-m5vv-6r4h-3vj9 https://github.com/advisories/GHSA-m5vv-6r4h-3vj9 | 4.90.0 | CarvedRock.Api (direct or transitive) | Upgrade to latest stable (4.90.0). Run integration tests that exercise auth flows.
| OpenTelemetry.Api | 1.5.0 | GHSA-g94r-2vxg-569j https://github.com/advisories/GHSA-g94r-2vxg-569j | 1.19.0 | CarvedRock.Api, instrumentation packages | Update to latest stable (1.19.0). Verify instrumentation packages compatibility with collector and SDKs.
| NuGet.Packaging | 6.12.1 | GHSA-g4vj-cjjj-v7hg https://github.com/advisories/GHSA-g4vj-cjjj-v7hg | 7.9.0 | CarvedRock.Api (build-time/tooling) | Update to 7.x if used directly; evaluate whether PackageReference can be removed if not required (NU1510 note for Logging.Abstractions applies similarly).
| NuGet.Protocol | 6.12.1 | GHSA-g4vj-cjjj-v7hg https://github.com/advisories/GHSA-g4vj-cjjj-v7hg | 7.9.0 | CarvedRock.Api (tooling) | Update to 7.x; verify build/restore in CI.

## Commands run (evidence)
- dotnet list CarvedRock.Api\CarvedRock.Api.csproj package --vulnerable
  - Output: warnings for Azure.Identity 1.10.3, Microsoft.Identity.Client 4.56.0, NuGet.Packaging 6.12.1, NuGet.Protocol 6.12.1, OpenTelemetry.Api 1.5.0, SQLitePCLRaw.lib.e_sqlite3 2.1.6, System.IdentityModel.Tokens.Jwt 7.0.3. See terminal logs in task run history.
- NuGet version queries (nuget.org) used to find latest stable candidates: System.IdentityModel.Tokens.Jwt (8.23.0), SQLitePCLRaw.lib.e_sqlite3 (3.53.3), Azure.Identity (1.21.0), Microsoft.Identity.Client (4.90.0), OpenTelemetry.Api (1.19.0), NuGet.Packaging (7.9.0).

## Impact analysis / Next steps
1. For each package above, create per-project PRs that: update the PackageReference to the candidate version in the net10.0 TFM group (where present), and for libraries consider multi-targeting strategy:
   - If a safe 8.x -> 10.x upgrade exists and is API-compatible, apply to both TFMs in multi-targeted projects.
   - If the upgrade is incompatible for net8.0, apply TFM-specific PackageReference (keep net8.0 on existing version) and document the rationale.
2. Run full CI and unit/integration tests for each PR. Pay attention to native packages (SQLitePCLRaw) — verify platform runners in CI.
3. Remove unnecessary explicit PackageReferences for automatically provided packages (e.g., Microsoft.Extensions.Logging.Abstractions) to reduce NU1510 noise.
4. For packages used only at build time (NuGet.* tooling), consider updating or removing direct references if they are not required at runtime.

## Decision record
- Priority: Upgrade SQLite and JWT packages first (high/medium severity respectively) and validate native behavior and auth flows before bulk upgrading telemetry and tooling packages.
- Backward compatibility: Preserve net8.0 behavior where possible via multi-targeted PackageReference or per-TFM versioning. CarvedRock.WebApp remains single-target net10.0 (scenario preference) and can be upgraded directly to latest versions.


---
Generated on: 2026-09-19 (UTC). Audit performed by automation agent. Actions taken: evidence collected and recommendations recorded. No code changes applied in this task.
