# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0 (.NET 10, LTS)
- **Compatibility Strategy**: Maintain backward compatibility (user requested)

## Source Control
- **Source Branch**: start-upgrade-over
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Notes
- User requested keeping backward compatibility. Will prefer multi-targeting or compatibility-preserving changes and record decisions per-project during planning.

## User Preferences
### Technical Preferences
- **CarvedRock.WebApp**: Force single-target to net10.0 (user request)

## Key Decisions Log
- **2026-09-19**: User requested forcing CarvedRock.WebApp to single-target net10.0 to simplify app migration while maintaining backward compatibility for libraries via multi-targeting.
