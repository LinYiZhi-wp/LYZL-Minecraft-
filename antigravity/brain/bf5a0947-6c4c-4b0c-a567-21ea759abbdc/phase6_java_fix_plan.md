# Phase 6: Java Environment & Launch Core Fix

## Goal
Fix the "Launch Failed" error caused by hardcoded Java path. Implement proper Java detection and selection.

## User Review Required
None. This is a critical bug fix and core feature implementation.

## Proposed Changes

### Services
#### [NEW] [JavaService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/JavaService.cs)
- `List<JavaInstallation> FindInstallations()`: Scans Registry and common paths for `javaw.exe`.
- `string? AutoDetectBestJava(string gameVersion)`: Heuristic to pick best Java (Java 8 for <1.17, Java 17/21 for newer).

#### [MODIFY] [ConfigService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/ConfigService.cs)
- Add `public string GlobalJavaPath { get; set; }` to `LauncherSettings` class.

#### [MODIFY] [GameService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/GameService.cs)
- Ensure `Parsing` handles java versions if stored in json (not strictly needed for this fix, but good for future).

### ViewModels
#### [MODIFY] [MainViewModel.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/ViewModels/MainViewModel.cs)
- Inject `JavaService`.
- In `LaunchGame`:
  1. Check `SelectedVersion.CustomJavaPath`.
  2. Check `ConfigService.Settings.GlobalJavaPath`.
  3. If invalid/empty, call `JavaService.FindInstallations()` and prompt user or auto-select.
  4. Use the resolved valid path.

### Views
#### [MODIFY] [SettingsPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/SettingsPage.xaml)
- Add Java Selector UI (ComboBox or Path Picker).

## Verification Plan
1. **Manual Verification**:
   - Open Settings Page, ensure it lists detected Java versions.
   - Select a valid Java path.
   - Launch a game version.
   - Verify success.
