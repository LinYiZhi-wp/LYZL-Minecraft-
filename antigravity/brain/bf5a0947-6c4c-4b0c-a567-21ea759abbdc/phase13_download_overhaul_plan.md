# Phase 13: Download Manager Overhaul & Stability

This phase addresses critical download failures (404s, 99% hangs) and the "ugly" UI reported by the user. We will align the Download Manager with the iOS 26 glassmorphism style and harden the download engine.

## User Review Required

> [!IMPORTANT]
> - The Download Manager UI will be completely redesigned to use the glassmorphism theme (Dark + Translucency).
> - We are introducing a 30-second timeout per file chunk to prevent "99% hangs" caused by zombie connections.

## Proposed Changes

### 1. High-Fidelity Glassmorphism UI
#### [MODIFY] [DownloadManagerPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/DownloadManagerPage.xaml)
- Change background to `iOS26.Background`.
- Use `iOS26.SidebarLayer` for the left statistics panel.
- Redesign task cards to use translucent backgrounds (`#30FFFFFF`) and better typography.
- Add text prompts for each stage (e.g., "正在下载", "已完成", "准备中").

### 2. Download Engine Hardening
#### [MODIFY] [DownloadService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadService.cs)
- Add a per-try timeout in `DownloadInternalAsync` to catch hanging streams.
- Improve error reporting so that a 404 or connection failure is explicitly caught and surfaced to the `DownloadTask`.
- Ensure `File.Move` only happens if the download is strictly successful.

### 3. Mirror Logic Synchronization
#### [MODIFY] [DownloadManagerService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadManagerService.cs)
- Apply `ReplaceSource` to asset URLs in `DownloadGameFullAsync` to support MCMirror and FastMirror for assets.
- Fix a potential logic error where `weightedProgress` calculation might result in "99.99%" due to many small files.

### 4. Progress Reporting Refinement
#### [MODIFY] [DownloadTask.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/DownloadTask.cs)
- Add `JsonStatusText`, `LibrariesStatusText`, `AssetsStatusText` properties to provide the "text prompts" requested by the user.

## Verification Plan

### Automated Tests
- `dotnet build` to ensure no syntax errors.
- Simulate a 404 response to verify the error behavior in the UI.

### Manual Verification
- Start a download with `BMCLAPI` and `MCMirror` to check for 404s on assets.
- Monitor a full game download to ensure it reaches 100% and renames the `.jar` correctly.
- Verify the new UI matches the aesthetics of the rest of the application.
