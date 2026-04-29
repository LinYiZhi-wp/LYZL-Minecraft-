# Dedicated Download Management UI Implementation Plan

Based on the provided design image, we will overhaul the Download Manager UI to be a dedicated, high-fidelity management interface.

## Proposed Changes

### 1. Data Model Enhancements
#### [MODIFY] [DownloadTask.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/DownloadTask.cs)
- Add sub-task progress properties:
    - `JsonStatus` (Completed/Pending)
    - `LibrariesProgress` (Percentage)
    - `AssetsProgress` (Percentage)
    - `InstallationStatus` (Pending/Running/Completed)
- Add `RemainingFilesCount` property.

### 2. UI Development
#### [NEW] [DownloadManagerPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/DownloadManagerPage.xaml)
- Implement the two-pane layout:
    - **Left Sidebar (Stats Area)**:
        - Total Progress (%)
        - Real-time Download Speed
        - Remaining Files count.
    - **Right Content Area (Task List)**:
        - Task Cards (e.g., "1.21.11 安装")
        - Sub-item list with status icons (Checkmark, Percentage, Loading dots).

#### [MODIFY] [MainWindow.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/MainWindow.xaml)
- Modify the floating download button logic to navigate to `DownloadManagerPage` (or open it as an overlay/dialog) instead of showing the small popup.

### 3. Logic Integration
#### [MODIFY] [DownloadManagerService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadManagerService.cs)
- Update `DownloadGameFullAsync` to report progress to the specific sub-task properties in `DownloadTask`.
- Calculate `RemainingFilesCount` dynamically.

## Verification Plan

### Automated Tests
- `dotnet build` to ensure UI and code-behind are valid.

### Manual Verification
- Start a download and check if the new "Download Management" page opens.
- Verify that the sub-tasks (JSON, Libraries, Assets) update their statuses/percentages in real-time.
- Verify the left sidebar statistics accurately reflect the global download state.
