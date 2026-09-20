# 03.02.01-audit-data-advisories: Audit advisory-prone runtime packages used by data layer

## Objective
Audit runtime advisories affecting the data layer (Npgsql, SQLitePCLRaw, Microsoft.IdentityModel.*).

## Scope Inventory
- CarvedRock.Data
- CarvedRock.Domain (transitive)
- Tests referencing data providers

## Steps
1. Enumerate current package versions in the projects.
2. Query NuGet for patched versions.
3. Determine per-TFM compatibility and recommend target versions.
4. Write audit findings to tasks/03.02.01-audit-data-advisories/progress-details.md

**Done when**: progress-details.md contains a table of advisories and recommended target versions per project and per-TFM.
