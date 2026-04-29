# 下载页 (Download Page) 实现计划

## 目标
创建一个功能完善的游戏版本下载页面，允许用户浏览、筛选和下载 Minecraft 官方版本、Modloader (Forge/Fabric/NeoForge/Quilt) 以及主流整合包。

## 核心功能
1.  **多标签导航**：
    *   **官方版本**：正式版、快照版、远古版。
    *   **Modloader**：Forge, Fabric, NeoForge, Quilt 安装器。
    *   **整合包**：CurseForge, Modrinth (后续阶段)。
2.  **版本列表**：
    *   显示版本号、发布时间、类型。
    *   支持搜索和筛选（如：只看Release）。
3.  **下载管理**：
    *   一键下载安装。
    *   下载进度显示。
    *   依赖库补全（Inherit from version json）。
4.  **数据源**：
    *   优先使用 **BMCLAPI** (国内加速)。
    *   备用 Mojang 官方 API。

## UI 设计 (参考 PCL2/HMCL)
- **左侧/顶部筛选栏**：选择分类（正式/快照）。
- **主要内容区**：卡片式或列表式版本展示。
- **右侧详情/操作栏**（可选）：选中版本后的详细信息和安装按钮。

## 技术实现
### 1. 数据模型
- `DownloadableVersion`: 从API获取的版本元数据。
- `DownloadTask`: 追踪下载状态。

### 2. 服务层
- `VersionManifestService`: 获取 version_manifest.json。
- `DownloaderService`: 处理文件下载、校验、解压。

### 3. ViewModel
- `DownloadViewModel`: 管理列表状态、筛选逻辑、下载命令。

## 开发步骤
1.  **基础架构**：创建 `DownloadPage.xaml` 和 `DownloadViewModel`，配置导航。
2.  **数据层**：实现 `VersionManifestService` (BMCLAPI集成)。
3.  **UI实现**：
    *   版本列表视图。
    *   分类筛选器。
4.  **下载逻辑**（核心难点）：
    *   解析版本JSON。
    *   下载Client Jar, Libraries, Assets。
    *   (MVP阶段先实现核心Jar下载，完整资源补全后续完善)。

## MVP 范围
- 仅支持 **官方版本** 下载。
- 解析并下载 version.json 和 client.jar。
- 简单的列表展示。
