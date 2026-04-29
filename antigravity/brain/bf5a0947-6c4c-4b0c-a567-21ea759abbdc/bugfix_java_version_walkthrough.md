# Bug Fix: Launch Failure (Exit Code 1)

This walkthrough details the fix for the "Game exited with code 1" error, which was caused by using an incompatible Java version (e.g., Java 17 for Minecraft 1.8.9).

## 1. The Issue
-   **Symptom**: Game crashes immediately with "Exit Code 1".
-   **Cause**: The launcher was defaulting to the Global Java Path (often Java 17/21) even for older game versions that require Java 8.

## 2. The Fix: Smart Java Selection

We implemented a logic that automatically selects the correct Java version based on the game's requirements, **before** falling back to the global setting.

### Changes
1.  **Model (`GameInstance.cs`)**: Added `RequiredJavaVersion` property (defaulting to 8).
2.  **Parser (`GameService.cs`)**: Now reads `javaVersion.majorVersion` from the game's `version.json` file.
3.  **Service (`JavaService.cs`)**: Updated `AutoDetectBestJava` to accept a target version (e.g., `8`) and prioritize finding a matching installation.
4.  **ViewModel (`MainViewModel.cs`)**:
    -   **Old Logic**: Custom Path -> Global Path -> Auto-Detect.
    -   **New Logic**: Custom Path -> **Auto-Detect (Strict Match)** -> Global Path -> Auto-Detect (Any).

## 3. How to Verify

1.  **Prerequisites**: Ensure you have both **Java 8** and **Java 17/21** installed.
2.  **Scenario A: Launch 1.8.9 (Needs Java 8)**
    -   Set Global Java Path to Java 17 in Settings.
    -   Go to Home, select 1.8.9.
    -   Click Launch.
    -   **Result**: Game should launch successfully. (The launcher silently picks Java 8 despite Global being 17).
3.  **Scenario B: Launch 1.18+ (Needs Java 17)**
    -   Set Global Java Path to Java 8.
    -   Go to Home, select 1.18.
    -   Click Launch.
    -   **Result**: Game should launch successfully. (The launcher picks Java 17).

## 4. Troubleshooting
-   If you see "Could not find a valid Java installation for version X", you need to install that specific version of Java.
