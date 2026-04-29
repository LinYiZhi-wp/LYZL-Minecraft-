# Microsoft Authentication Walkthrough

This document guides you through verifying the new Microsoft Authentication integration in GeminiLauncher.

## 1. Feature Overview
We have implemented a native-like login flow using an embedded `WebView2` browser. This allows you to log in with your Microsoft account securely without leaving the launcher.

## 2. Changes Implemented
- **WebView2 Integration**: Added `Microsoft.Web.WebView2` dependency.
- **Login Dialog**: Created `MicrosoftLoginDialog.xaml` to host the Microsoft OAuth pages.
- **Service Logic**: Updated `AuthenticationService` to launch the dialog and capture the authorization code.
- **UI Connection**: Connected the "Add Microsoft Account" button in Settings to the new flow.

## 3. How to Verify

1.  **Open Settings**: Navigate to the Settings page.
2.  **Click "Add Microsoft Account"**:
    -   Verify that a dialog window opens.
    -   Verify that the Microsoft Login page loads inside the dialog.
3.  **Perform Login**:
    -   Enter your Microsoft credentials.
    -   Approve the permissions (if asked).
4.  **Success State**:
    -   Verify the dialog closes automatically after a successful login.
    -   **Check Accounts List**: Verify your Microsoft account (with your Minecraft username and skin) appears in the Accounts list.
    -   **Check Home Page**: Verify your player name and head are updated in the top-right corner capsule.
5.  **Game Launch**:
    -   Select a game version.
    -   Click Launch.
    -   Verify the game starts and you are logged in (e.g., check multiplayer servers).

## 4. Troubleshooting
-   **"Browser interaction required"**: If you see this error, ensure the `Microsoft.Web.WebView2` runtime is installed on your system (it usually is on Windows 10/11).
-   **Blank Dialog**: It might take a moment to load the login page. Check your internet connection.

## 5. Next Steps
-   Implement Modloader installation.
-   Implement Modpack import UI.
