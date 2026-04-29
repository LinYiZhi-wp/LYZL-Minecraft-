# ModLoader Installer Walkthrough (Fabric)

## Changes
- **New Service**: `ModLoaderService.cs`
    - Implemented `InstallFabricAsync` to fetch Fabric metadata from `meta.fabricmc.net`.
    - Downloads all required libraries to `.minecraft/libraries`.
    - Returns the Fabric Profile JSON for further processing.
    - Uses `DownloadBatchAsync` for efficient parallel downloading.
- **Integration**: `ModpackService.cs`
    - Detected `fabric-loader` dependency in `.mrpack` metadata.
    - Calls `ModLoaderService.InstallFabricAsync` during import.
    - Uses the returned Fabric JSON as the base for the Modpack's version definition, ensuring the instance has the correct main class and libraries.

## Verification
- [x] **Code Compilation**: `dotnet build` successful.
- [x] **Logic Check**: 
    - `ModpackService` correctly identifies Fabric.
    - `ModLoaderService` fetches the correct JSON structure.
    - Libraries are added to the download queue.
    - JSON is saved with the correct ID.

## Next Steps
- Implement **NeoForge** support (Future Task).
- Add specific UI for standalone ModLoader installation (outside of Modpacks), if requested.
