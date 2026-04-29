# 版本选择系统实现说明

## 功能概述

实现了一个类似HMCL的版本选择系统，支持：
- **自动检测**游戏目录（当前目录、官方启动器目录）
- **自动扫描**版本并解析version.json
- **智能识别**Mod加载器（Forge、Fabric、NeoForge、Quilt）
- **分类显示**版本（可装Mod、常规版本、错误版本）

## 实现的组件

### 1. 数据模型

#### [GameDirectory.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/GameDirectory.cs)
```csharp
public class GameDirectory
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDefault { get; set; }
    public DirectorySource Source { get; set; } // Auto/Manual/Current
}
```

#### [GameVersion.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/GameVersion.cs)
```csharp
public class GameVersion
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string MinecraftVersion { get; set; }
    public VersionType Type { get; set; } // Release/Snapshot
    public ModLoader? Loader { get; set; } // Forge/Fabric/NeoForge/Quilt
    public string LoaderVersion { get; set; }
    public VersionCategory Category { get; set; } // Moddable/Vanilla/Broken
    public string Icon { get; set; }
}
```

### 2. 版本检测服务

[VersionDetectionService.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Services/VersionDetectionService.cs)

**核心功能：**

1. **自动检测游戏目录**
   - 当前默认目录
   - 官方启动器目录（`%APPDATA%\.minecraft`）
   - 用户自定义目录

2. **版本扫描**
   - 遍历`versions`文件夹
   - 解析每个版本的`version.json`
   - 提取版本信息和依赖库

3. **Mod加载器识别**
   - 从版本ID识别（如`1.21.8-forge-58.0.3`）
   - 从libraries字段识别Maven依赖
   - 自动提取加载器版本号

4. **版本分类**
   - **可装Mod**：已安装Forge/Fabric等
   - **常规版本**：原版或OptiFine
   - **错误版本**：version.json缺失或损坏

### 3. 版本选择对话框UI

[VersionSelectorDialog.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/VersionSelectorDialog.xaml) | [.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/VersionSelectorDialog.xaml.cs)

**布局：**
- **左侧**：文件夹列表，支持自动检测和手动添加
- **右侧**：版本列表，按类别分组显示

**交互：**
- 点击文件夹 → 自动扫描并显示该目录下的版本
- 点击版本 → 绿色高亮
- 双击版本 → 直接确认选择
- 添加文件夹按钮 → 选择自定义.minecraft目录

### 4. 主页集成

[HomePage.xaml](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/HomePage.xaml#L125-L170) | [.cs](file:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Views/HomePage.xaml.cs#L43-L76)

**替换内容：**
- 移除了静态ComboBox
- 添加了版本选择按钮，显示图标、版本名称、详细信息
- 点击按钮打开对话框，选择后更新UI显示

## 使用流程

1. **打开应用** → HomePage显示"点击选择版本"按钮
2. **点击按钮** → 打开VersionSelectorDialog
3. **对话框初始化** → 自动检测游戏目录并选中第一个
4. **自动扫描** → 扫描选中目录的所有版本并分类显示
5. **选择版本** → 点击版本高亮，点击"确定"
6. **返回主页** → 显示选中的版本名称和详细信息

## 技术亮点

- **自动化检测**：无需手动配置，自动发现官方启动器目录
- **智能解析**：支持多种Mod加载器识别方式（ID解析 + Maven依赖分析）
- **容错处理**：损坏的版本标记为"错误版本"而非崩溃
- **iOS 26设计**：对话框采用浮动卡片 + 毛玻璃效果

## 待实现功能

- [ ] 保存选中版本到配置文件（持久化）
- [ ] 整合包导入功能（解析.zip/.mrpack）
- [ ] 版本排序和搜索
- [ ] 显示版本详细信息（Java版本要求、依赖库列表）
