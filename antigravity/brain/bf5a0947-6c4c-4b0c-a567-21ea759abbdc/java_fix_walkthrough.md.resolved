# Java环境修复与设置页更新 (Phase 6)

## 问题描述
用户尝试启动游戏时遇到 "Launch Failed" 错误，提示找不到 `javaw.exe`。
原因：代码中存在硬编码的调试路径 `C:\Program Files\Java\jdk-17\bin\javaw.exe`。

## 解决方案
已移除硬编码路径，并实现了完整的 **Java环境检测与配置系统**。

### 1. 自动检测 (Auto-Detect)
*   启动器现在会在启动时自动扫描系统中已安装的 Java 版本。
*   扫描范围包括：注册表、Program Files (Java/Adoptium/Microsoft/Azul/BellSoft) 以及用户目录。
*   如果未配置 Java 路径，会自动选择最新的可用版本。

### 2. 手动配置 (Settings Page)
*   **设置页面** 新增了 Java 路径配置区域。
*   **自动搜索按钮**：点击可一键重新扫描并应用最佳 Java 版本。
*   **浏览按钮**：允许用户手动选择特定的 `javaw.exe`。
*   **版本检测**：选择路径后，会自动识别并显示 Java 版本号（如 "Java 17 detected"）。

### 3. 持久化
*   选择的 Java 路径会自动保存到 `config.json`，下次启动无需重新配置。

## 如何验证
1.  进入 **Settings** 页面。
2.  查看 "Java Settings" 区域，应显示已检测到的 Java 路径。
3.  点击 "Auto Search" 测试自动检测功能。
4.  回到主页，点击启动游戏，应能正常启动。
