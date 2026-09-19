Updated CarvedRock.Domain.csproj to multi-target net8.0 and net10.0. Added conditional net10.0 PackageReference entries for AutoMapper and Microsoft.Extensions.Logging.Abstractions; bumped AutoMapper to 14.0.0 for net10 target. Ran restore; solution build still blocked by CarvedRock.Api OpenAPI generation error on net8 target. Committed CarvedRock.Domain.csproj.

Files modified:
- CarvedRock.Domain/CarvedRock.Domain.csproj

Next: Address CarvedRock.Api OpenAPI generation/build failure, then update CarvedRock.WebApp per user preference to single-target net10.0 and update test projects.