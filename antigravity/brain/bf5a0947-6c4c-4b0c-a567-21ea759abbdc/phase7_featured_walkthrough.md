# Phase 7 完成 + 通用组件 Walkthrough

## 完成的功能

### 1. 资源页 🔥热门 / ✨最新 板块

**改动文件:**
- [ModrinthService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/Ecosystem/ModrinthService.cs) — 新增 `GetTrendingAsync()` (按下载量排序) 和 `GetNewestAsync()` (按最新排序)
- [ResourcesViewModel.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/ViewModels/ResourcesViewModel.cs) — 新增 `TrendingMods`/`NewestMods` 集合 + `HasSearchResults` 标志，构造时自动加载
- [ResourcesPage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/ResourcesPage.xaml) — 重写 ScrollViewer: 无搜索时显示横向滚动的热门/最新卡片，搜索后切换为 WrapPanel 网格
- [InverseBooleanToVisibilityConverter.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Converters/InverseBooleanToVisibilityConverter.cs) — [NEW] 反向布尔转换器

**交互逻辑:**
- 页面加载 → 自动请求 Modrinth API，填充热门/最新两行横滑卡片
- 用户搜索 → `HasSearchResults=true` → 隐藏推荐板块，显示搜索结果网格
- 清空搜索 → 恢复推荐板块

---

### 2. iOS26 样式资源字典扩展

**改动文件:** [iOS26.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Styles/iOS26.xaml)

新增样式:

| Key | 用途 |
|-----|------|
| `iOS26.GlassCard` | 玻璃拟态卡片 (半透明+边框+阴影) |
| `iOS26.HoverCard` | 带 hover 放大效果的卡片 (预设 TransformGroup) |
| `iOS26.SecondaryButton` | 半透明次要按钮 (hover 变亮, press 缩放) |
| `iOS26.JellyButton` | 🎯 果冻弹性按钮 (ElasticEase 回弹) |
| `iOS26.PageSlideIn` | 页面淡入+上滑转场 Storyboard |
| `iOS26.PageZoomIn` | 页面淡入+缩放转场 Storyboard (BackEase) |
| `iOS26.Text.Title/Subtitle/Body/Caption` | 文本层级样式 |
| `iOS26.Badge` | 绿色徽章 |

---

### 验证结果

```
dotnet build → 116 个警告, 0 个错误 ✅
```
