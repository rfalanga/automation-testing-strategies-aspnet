Npgsql Upgrade Plan

Goal
- Remediate Npgsql advisory reported against Npgsql 8.0.2 while preserving net8 compatibility.

Current state
- CarvedRock.Data currently references Npgsql.EntityFrameworkCore.PostgreSQL 8.0.2 for net8 and 10.0.3 for net10.
- Build warnings show advisory GHSA-x9vc-6hfv-hg8c affecting Npgsql 8.0.2.

Options
1. Patch net8 packages (preferred if a patched 8.x exists)
   - Find an 8.x release that fixes GHSA-x9vc-6hfv-hg8c and update CarvedRock.Data net8 PackageReference to that version.
   - Validate restore/build/tests for net8 and net10 TFMs.
2. If no 8.x patch exists
   - Keep current net8 package but document risk and schedule migration; or
   - Multi-target or single-target projects that need the patched provider, or
   - Replace provider with alternative that supports net8 (if viable).
3. Upgrade net10 provider to 10.0.3 (already present) to ensure net10 builds use the patched provider.

Proposed small-scope plan (this branch)
- Research: confirm whether an 8.x patch for Npgsql exists and its version number.
- If patch exists: apply the patch to net8 PackageReference, run restore/build/tests, open PR with changes.
- If no patch: open a draft PR documenting the advisory, impact, and recommended migration path (multi-targeting or schedule).

Validation
- Restore and build for both TFMs.
- Run unit & integration tests (testcontainers if available) on CI.

Notes
- Upgrading runtime packages can require code or TFM changes; changes will be isolated to an upgrade-npgsql branch and validated by CI before merge.

Files to review
- CarvedRock.Data/CarvedRock.Data.csproj
- Any project that transitive-depends on Npgsql (search for Npgsql references)

Links
- Advisory: https://github.com/advisories/GHSA-x9vc-6hfv-hg8c

Prepared by: GitHub Copilot
