# Phase 13 Walkthrough: Download Manager Overhaul

针对你反馈的“下载不成功”、“卡 99%”以及“UI 丑且没文字”的问题，我进行了全面的重构和加固。现在下载管理器不仅更稳定，而且更符合 iOS 26 的高端审美。

## 1. 拒绝卡顿：下载引擎加固
- **读取超时 (Aggressive Timeouts)**: 针对“卡 99%”的问题，我为下载流增加了 30 秒的强制超时检查。如果服务器在 30 秒内没有发送任何数据，系统会自动断开并重试，避免死连接占用进度。
- **完整性校验**: 现在系统会严格对比 `Content-Length`，只有当文件字节数完全吻合且 SHA1 校验通过时，才会将 `.partial` 重命名为正式文件。

## 2. 告别 404：全路径镜像同步
- **万能替换**: 之前部分资源文件（Assets）和支持库（Libraries）可能跳过了镜像替换逻辑。现在所有下载请求都会强制经过 `ReplaceSource` 逻辑，确保 BMCLAPI、MCMirror 等源能覆盖到 100% 的文件。

## 3. 高端审美：iOS 26 玻璃拟态 UI
- **视觉升级**: 彻底重写了 `DownloadManagerPage.xaml`。
    - **侧边栏**: 使用了和主界面一致的磨砂玻璃材质（`SidebarLayer`）。
    - **任务卡片**: 采用了半透明悬浮卡片设计，配合精致的投影和渐变进度条。
- **文字提示 (Better Feedback)**: 
    - 增加了详细的文字提示，如“正在获取版本信息...”、“正在下载依赖库 (45/120)”、“资源文件已完成”。
    - 每个子任务现在有独立的进度条和状态标注，清清楚楚。

## 修改的文件
- [DownloadService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadService.cs) (超时与稳定性增强)
- [DownloadManagerService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Network/DownloadManagerService.cs) (进度逻辑与镜像修复)
- [DownloadManagerPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/DownloadManagerPage.xaml) (UI 彻底重构)
- [DownloadTask.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/DownloadTask.cs) (新增 UI 状态字段)

---
> [!TIP]
> 现在请再次尝试下载。如果遇到不稳定的网络，你会看到进度条在跳动几秒后自动重连并继续，而不会永久卡在 99%。
