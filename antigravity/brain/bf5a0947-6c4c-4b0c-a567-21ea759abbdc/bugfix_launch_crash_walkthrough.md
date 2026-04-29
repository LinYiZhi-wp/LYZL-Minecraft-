# Bug Fix: Launch Failure (Java Exception / Exit Code 1)

This walkthrough details the fixes for "Java Exception has occurred" and "Exit Code 1" errors.

## 1. The Issues
-   **Exit Code 1**: Caused by Java Version Mismatch (Fixed in previous step).
-   **Java Exception has occurred**: Likely caused by **missing Native Libraries** (DLLs) required by Minecraft (LWJGL), or incorrect JVM arguments.

## 2. The Fixes

### A. Smart Java Selection (Previously Applied)
-   Automatically selects Java 8 for Minecraft 1.8.9 and Java 17 for 1.18+.

### B. Natives Extraction (New!)
-   **Problem**: Minecraft 1.8.9 requires native libraries (`.dll` files on Windows) to be extracted to a specific folder.
-   **Fix**: Modified `LaunchService.cs` to automatically extract `natives-windows` jars to `bin/natives` before launch.
-   **Flag**: Enabled `-Djava.library.path=".../bin/natives"` argument.

### C. Debug Logging (New!)
-   The launcher now writes the **exact command line** used to launch the game to a file named `debug_launch_cmd.txt` in the game version folder.
-   This allows you to verify exactly what arguments are being passed to Java.

## 3. How to Verify

1.  **Launch 1.8.9**:
    -   Click Launch.
    -   **Expected**: The "Java Exception" popup should be GONE, and the game should start.

2.  **Verify Natives**:
    -   Go to your game specific folder (e.g. `.minecraft/versions/1.8.9`).
    -   Check if a `bin/natives` folder exists.
    -   Inside usage should be `.dll` files (e.g. `lwjgl64.dll`).

3.  **If Issues Persist**:
    -   Check the `debug_launch_cmd.txt` file in the game folder.
    -   Share the content of this file to debug further.

## 4. Troubleshooting
-   **"Java Exception has occurred" persists**: This usually means the Classpath is still wrong, or the Main Class is invalid. Please check `debug_launch_cmd.txt`.
