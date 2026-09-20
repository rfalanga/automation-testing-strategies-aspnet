# 03.02.01-audit-advisories: Audit runtime advisories affecting data projects

## Objective
Confirm advisory details for runtime packages that affect data and domain projects (Npgsql, SQLitePCLRaw, System.IdentityModel.Tokens.Jwt, Microsoft.Identity.Client).

## Scope Inventory
- Projects: CarvedRock.Data, CarvedRock.Domain, CarvedRock.Api
- Packages to check: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, SQLitePCLRaw.lib.e_sqlite3, System.IdentityModel.Tokens.Jwt, Microsoft.Identity.Client

## Done when
- A remediation table exists listing current version, advisory id, patched versions available, and recommended action for each package.

