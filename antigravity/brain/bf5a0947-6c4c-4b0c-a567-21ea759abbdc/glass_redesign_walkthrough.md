# Phase 9: iOS 26 磨砂玻璃大重构 — Walkthrough

**0 errors · 4批次全部完成 · 13个文件修改**

---

## 批次A: 视觉基础层

### 玻璃材质层级
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Styles/iOS26.xaml)

| 层级 | 旧值 | 新值 | 用途 |
|------|------|------|------|
| Background | `#99000000` (60%) | `#40000000` (25%) | 最底层，透出壁纸 |
| SidebarLayer | — | `#4D101014` (30%) | 侧边栏，极通透 |
| ContentLayer | `#CC1A1A1A` (80%) | `#80181820` (50%) | 内容区，半透明 |
| FloatingLayer | `#F21A1A1A` (95%) | `#B0202028` (69%) | 弹出层 |
| CardLayer | — | `#30FFFFFF` | 白色微光卡片 |

### 自定义滚动条
- [GlassScrollBar.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Styles/GlassScrollBar.xaml) — 6px thin, `#40FFFFFF` thumb, hover→10px + brighter
- 全局注册在 [App.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/App.xaml)

### HeroButton 宝石渐变
`LinearGradientBrush`: `#2ECC71` → `#00E676` → `#00C853`，发光 BlurRadius 60

### 主窗口悬浮感
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/MainWindow.xaml)

---

## 批次B: 主页重构

### 版本中心化
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/HomePage.xaml)

- 中央 `VersionHeroText` 56px bold（默认 "LinLaunch"，选版后显示版本号）
- 副标题显示 `MinecraftVersion · Loader`（如 "1.20.1 · Fabric"）

### 🧩 Mod 快捷入口
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/HomePage.xaml.cs)

- 底部三按钮布局：版本选择 / 🧩 / 版本设置
- 点击 🧩 → 直接打开 `VersionSettingsDialog` 的 Mods tab

---

## 批次C: 功能交互

### 下载 Action Sheet
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/ViewModels/DownloadViewModel.cs)

- ☁️ 仅原版 / 🧵 原版+Fabric（推荐标记）/ ⚒️ 原版+NeoForge
- 选Fabric后自动调用 `ModLoaderService.InstallFabricAsync`

### 内存智能警告
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/SettingsPage.xaml.cs)

- `<1024MB` → 🔴 "分配过低"
- `>80% 物理RAM` → 🔴 "可能卡死"
- 正常 → 🟢 "推荐范围"

### 版本选择 folder 折叠
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/VersionSelectorDialog.xaml)

- 文件夹面板默认 `Collapsed`，📁 按钮切换显示/隐藏

---

## 批次D: 隐私与账号

### 隐私模式 + Token 状态
render_diffs(file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/SettingsPage.xaml)

- 🔒 隐私模式 ToggleSwitch（网吧模式）
- 🔑 Token 状态显示

---

## 验证结果

```
dotnet build → 0 个错误, 116 个警告（全为已有 null 引用警告）
```
