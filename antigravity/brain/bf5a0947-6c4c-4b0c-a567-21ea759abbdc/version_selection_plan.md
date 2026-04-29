# 版本选择和文件夹管理系统

## 目标

实现类似HMCL的版本选择系统，用户可以：
1. 管理多个游戏文件夹（`.minecraft`目录）
2. 自动检测文件夹中已安装的版本
3. 查看版本详细信息（版本号、Mod加载器类型）
4. 分类显示版本（可装Mod、常规版本、错误版本）

## UI设计

### 版本选择对话框 (VersionSelectorDialog)

```
┌─────────────────────────────────────────────────────────────┐
│  版本选择                                    [_] [□] [×]     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐  ┌────────────────────────────────────┐  │
│  │ 文件支列表    │  │  可装 Mod (4)                ▼     │  │
│  │              │  │  ┌──────────────────────────────┐  │  │
│  │ ★ 当前文件夹 │  │  │ 🧊 1.21.8-Forge 58.0.3      │  │  │
│  │   D:\pc\..   │  │  │    正式版 1.21.8, Forge ... │  │  │
│  │              │  │  └──────────────────────────────┘  │  │
│  │ 官方启动器   │  │  │ 🔧 1.21.4-Forge 54.0.34     │  │  │
│  │   C:\Users.. │  │  │    正式版 1.21.4, Forge ... │  │  │
│  │              │  │  └──────────────────────────────┘  │  │
│  │ mc          │  │  │ 🧵 undertale2               │  │  │
│  │   C:\Users.. │  │  │    正式版 1.21, Fabric ...  │  │  │
│  │              │  │  └──────────────────────────────┘  │  │
│  ├──────────────┤  │                                      │  │
│  │ + 添加文件夹 │  │  常规版本 (2)                ▼     │  │
│  │ 📦 导入整合包│  │  │ 📦 1.21.10-OptiFine J7_pre2│  │  │
│  └──────────────┘  │  │ 🟢 Smoke                    │  │  │
│                     │                                      │  │
│                     │  错误的版本 (2)              ▼     │  │
│                     │  │ ⚠️ broken_version          │  │  │
│                     └────────────────────────────────────┘  │
│                                                               │
│                                    [取消]  [确定(Enter)]     │
└─────────────────────────────────────────────────────────────┘
```

## 实现方案

### 1. 数据模型

#### GameDirectory (游戏目录)
```csharp
public class GameDirectory
{
    public string Name { get; set; }          // 显示名称
    public string Path { get; set; }          // 完整路径
    public bool IsDefault { get; set; }       // 是否默认
    public DirectorySource Source { get; set; } // Auto/Manual/Official
}
```

#### GameVersion (游戏版本)
```csharp
public class GameVersion
{
    public string Id { get; set; }            // 版本ID（文件夹名）
    public string DisplayName { get; set; }   // 显示名称
    public string MinecraftVersion { get; set; } // MC版本号
    public VersionType Type { get; set; }     // Release/Snapshot
    public ModLoader? Loader { get; set; }    // Forge/Fabric/NeoForge/null
    public string LoaderVersion { get; set; } // 加载器版本
    public VersionCategory Category { get; set; } // Moddable/Vanilla/Broken
    public string Icon { get; set; }          // 图标（🧊/🔧/🧵/📦/⚠️）
}

public enum VersionCategory
{
    Moddable,   // 可装Mod（已安装Forge/Fabric）
    Vanilla,    // 常规版本（原版或OptiFine）
    Broken      // 错误的版本（检测失败）
}
```

### 2. 版本检测服务 (VersionDetectionService)

核心功能：
1. **扫描目录**：读取`{gameDir}/versions/`下的所有子文件夹
2. **解析version.json**：
   - 读取`inheritsFrom`判断是否基于其他版本
   - 检测`id`中是否包含`forge`/`fabric`/`neoforge`
   - 解析`libraries`确认Mod加载器类型和版本
3. **分类**：
   - 有Forge/Fabric/NeoForge → Moddable
   - 纯原版或OptiFine → Vanilla
   - version.json缺失或解析失败 → Broken

### 3. UI组件

#### VersionSelectorDialog.xaml
- 左侧：`ListView<GameDirectory>`
- 右侧：`ItemsControl<VersionGroup>`，每组是一个可折叠的分类

#### HomePage.xaml改造
替换当前的ComboBox：
```xml
<Button Content="选择版本：1.20.4 (Forge 49.0.3)" 
        Click="OpenVersionSelector"
        Style="{StaticResource iOS26VersionButton}"/>
```

### 4. 文件夹管理

- **自动检测**：
  - 当前目录：`D:\pc\2\.minecraft\`（从配置读取）
  - 官方启动器：`%AppData%\.minecraft\`
  - 自定义：用户添加的路径（保存到配置文件）

- **添加文件夹**：打开FolderBrowserDialog，验证是否包含`versions`目录

- **导入整合包**：后续Phase实现，暂时显示入口

## 技术细节

### version.json解析示例

**Forge版本**：
```json
{
  "id": "1.21.8-forge-58.0.3",
  "inheritsFrom": "1.21.8",
  "libraries": [
    { "name": "net.minecraftforge:forge:1.21.8-58.0.3" }
  ]
}
```

**Fabric版本**：
```json
{
  "id": "fabric-loader-0.16.10-1.21",
  "libraries": [
    { "name": "net.fabricmc:fabric-loader:0.16.10" }
  ]
}
```

### 配置文件存储

`config.json`新增字段：
```json
{
  "gamePaths": [
    { "name": "当前文件夹", "path": "D:\\pc\\2\\.minecraft", "isDefault": true },
    { "name": "mc", "path": "C:\\Users\\...\\Desktop\\mc\\.minecraft", "isDefault": false }
  ],
  "selectedVersion": "1.21.8-forge-58.0.3",
  "selectedGamePath": "D:\\pc\\2\\.minecraft"
}
```

## 验证计划

1. 创建测试目录结构
2. 测试版本检测（Forge/Fabric/原版）
3. 验证UI交互（选择版本、切换文件夹）
4. 确认配置持久化

## 后续优化

- 版本图标优化（使用真实的Forge/Fabric图标）
- 版本排序（按时间/版本号）
- 搜索过滤功能
- 版本右键菜单（删除、重命名、打开文件夹）
