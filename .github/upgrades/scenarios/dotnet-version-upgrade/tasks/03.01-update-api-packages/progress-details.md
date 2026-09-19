Updated CarvedRock.Api.csproj to multi-target net8.0 and net10.0. Added conditional PackageReference entries for net10.0 for the packages that require newer versions (JwtBearer, EF.Design, EF.Sqlite, EF.SqlServer, HealthChecks.EntityFrameworkCore, Logging.Abstractions, CodeGeneration.Design). Ran restore and attempted solution build; build reports errors in CarvedRock.Api net8.0 GenerateOpenApiDocuments target due to Microsoft.Extensions.ApiDescription.Server tool failing to read DOTNET_STARTUP_HOOKS (external tool issue). Committed CarvedRock.Api.csproj.

Files modified:
- CarvedRock.Api/CarvedRock.Api.csproj

Next steps: adjust project to avoid GenerateOpenApiDocuments failure (tool config) or scope builds per-target; then update other projects' packages per plan.