Updated CarvedRock.Data.csproj to multi-target net8.0 and net10.0. Added conditional net10.0 PackageReference entries for EF Design, Sqlite, SqlServer, and Logging.Abstractions. Ran restore and build; build still failing at CarvedRock.Api GenerateOpenApiDocuments for net8.0 target. Committed CarvedRock.Data.csproj.

Files modified:
- CarvedRock.Data/CarvedRock.Data.csproj

Next steps: Address CarvedRock.Api OpenAPI generation failure or scope build targets; then continue updating CarvedRock.Domain and WebApp packages per plan.