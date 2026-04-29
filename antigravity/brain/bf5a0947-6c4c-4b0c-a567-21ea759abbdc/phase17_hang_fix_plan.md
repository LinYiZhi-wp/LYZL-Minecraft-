# Phase 17: Fixing UI Thread Starvation (Download Manager Hang)

The user reported that the application hangs and becomes unresponsive when clicking the button to view download progress. This is likely due to "UI Thread Flooding"—the background download task is reporting progress for every single file (up to 1000+ files), causing the UI thread to spend all its time processing property change notifications instead of handling user interaction and navigation.

## Proposed Changes

### 1. Progress Throttling
#### [MODIFY] [DownloadManagerService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadManagerService.cs)
- Implement a simple throttling mechanism using a `Stopwatch` or timestamp.
- Update the UI properties (Progress, StatusText) only if at least 16ms (60fps) or 50ms have passed since the last update, or if it's the final update.

### 2. Batch Reporting Optimization
#### [MODIFY] [DownloadService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadService.cs)
- Ensure `DownloadBatchAsync` doesn't flood the `IProgress` reporter if files are finishing near-simultaneously.

### 3. Rendering Stability
#### [MODIFY] [DownloadManagerPage.xaml.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/DownloadManagerPage.xaml.cs)
- Set `this.DataContext` **before** `InitializeComponent` to avoid a massive binding update storm immediately after initialization.

## Verification Plan

### Manual Verification
- Start a large download (e.g., a version with many assets).
- Repeatedly navigate to and from the Download Manager page.
- Verify that the UI remains responsive and the navigation is instantaneous.
- Ensure the progress bars still update smoothly (throttled to ~20-60 FPS).
