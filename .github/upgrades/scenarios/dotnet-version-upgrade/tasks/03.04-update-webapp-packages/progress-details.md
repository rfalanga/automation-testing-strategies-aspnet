Updated CarvedRock.WebApp.csproj to single-target net10.0 per user preference. Bumped Microsoft.AspNetCore.Authentication.OpenIdConnect to 10.0.12 and AspNetCore.HealthChecks.OpenIdConnectServer to 10.0.0. Ran restore/build; build still failing at CarvedRock.Api OpenAPI generation for net8 target. Committed CarvedRock.WebApp.csproj.

Files modified:
- CarvedRock.WebApp/CarvedRock.WebApp.csproj

Next steps: Fix CarvedRock.Api OpenAPI generation/build failure, then update test projects and tools.