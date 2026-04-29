# Phase 6: UI Refactoring Implementation Plan

## Goal
Transform `SettingsPage.xaml` to match the "iOS 26" Grouped List design language, ensuring consistency with the rest of the application.

## User Review Required
None.

## Proposed Changes

### Styles
#### [MODIFY] [iOS26.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Styles/iOS26.xaml)
- Add `iOS26.SettingsGroup` style (Card container).
- Add `iOS26.SettingsItem` style (Base style for rows).
- Add `iOS26.SettingsItem.Icon` style.
- Add `iOS26.SettingsItem.Content` style.
- Add `iOS26.SettingsItem.Control` style (for right-side controls like Switch, Arrow, Text).

### Views
#### [MODIFY] [SettingsPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/SettingsPage.xaml)
- Replace manual Borders with `iOS26.SettingsGroup`.
- Refactor "Game Settings", "Java Settings", "Memory", "Accounts", "About" sections to use the new item structure.
- **Java Settings**:
  - Row 1: Java Path (Label left, Value right, Chevron/Button).
  - Row 2: Auto Search (Button style).
- **Accounts**:
  - List of accounts as rows.
  - "Add Account" button as a row or footer.
- **Memory**:
  - Integrate slider better.

#### [MODIFY] [SettingsPage.xaml.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/SettingsPage.xaml.cs)
- Update event handlers if UI structure changes significantly (unlikely, mostly XAML).
- Ensure "Open Path" buttons work.

## Verification
- Visual inspection of Settings Page.
- Verify all buttons and interactions still function.
