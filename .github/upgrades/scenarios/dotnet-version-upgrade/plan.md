# .NET 10 Upgrade Plan

## Overview

Target: CarvedRock solution (upgrade projects to .NET 10)
Scope: 9 projects; prioritize Razor Pages WebApp and API; libraries multi-target to preserve backward compatibility unless explicitly forced.

## Tasks

### 01-assessment: Run assessment
Documented in assessment.md.

### 02-planning: Create upgrade plan
Produce per-project strategies, package lists, and task breakdown.

### 03.01-update-api-packages: Update NuGet packages for CarvedRock.Api
Update recommended packages to versions compatible with .NET 10. Build and validate.

**Done when**: CarvedRock.Api.csproj updated, solution builds, and no new package-related errors remain.

---

### 03.02-update-data-packages: Update NuGet packages for CarvedRock.Data
Update EF packages and logging abstractions to .NET 10-compatible versions.

**Done when**: CarvedRock.Data.csproj updated and solution builds.

---

### 03.03-update-domain-packages: Update NuGet packages for CarvedRock.Domain
Update logging and security-sensitive packages (e.g., AutoMapper) to safe versions.

**Done when**: CarvedRock.Domain.csproj updated and unit tests pass.

---

### 03.04-update-webapp-packages: Update NuGet packages for CarvedRock.WebApp (Razor Pages, single-target net10.0)
Update authentication and logging packages, validate Razor Pages behavior and routing.

**Done when**: CarvedRock.WebApp targets net10.0, packages updated, and web app builds and runs locally.

---

### 03.05-update-tests-core-packages: Update test packages (CarvedRock.InnerLoop.Tests)
Update Microsoft.AspNetCore.Mvc.Testing and other test-related packages. Run tests.

**Done when**: Test project packages updated and tests execute.

---

### 03.06-update-tests-webapp-packages: Update test packages (CarvedRock.InnerLoop.WebApp.Tests)
Update test and helper packages (AngleSharp, xUnit updates) and run web app tests.

**Done when**: Tests updated and pass.

---

### 03.07-update-tools-packages: Update tools (WireMockRecorder)
Update user-secrets package and any flagged packages.

**Done when**: Tools project updated and builds.

---

### 04-execution: Apply code fixes and TFM changes
Apply source/binary compatibility fixes, update TFMs (multi-target libraries; WebApp single-target net10.0), build, and run tests.

**Done when**: All projects target net10.0 or multi-target as planned, solution builds without errors, and tests pass.


