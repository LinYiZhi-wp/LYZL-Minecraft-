# LYZL Minecraft Launcher

一个基于 .NET 8 + WPF 的现代化 Minecraft 启动器，支持游戏版本管理、模组资源下载、微软账号登录等功能。

## 功能特性

- **游戏版本管理**：下载、安装、管理多个 Minecraft 版本，支持版本隔离
- **资源管理**：集成 Modrinth，支持浏览、搜索和下载模组、资源包、模组包
- **账号系统**：支持微软账号登录认证
- **下载管理**：多任务下载队列，支持断点续传和进度追踪
- **现代化 UI**：iOS26 风格毛玻璃效果，统一颜色系统，流畅过渡动画
- **多语言**：支持中文和英文切换
- **Java 管理**：自动检测和配置 Java 运行环境
- **游戏导出**：支持导出游戏实例配置

## 技术栈

| 技术 | 用途 |
|------|------|
| .NET 8 | 运行时框架 |
| WPF | 桌面 UI 框架 |
| WPF-UI | 现代化 UI 控件库 |
| CommunityToolkit.Mvvm | MVVM 架构 |
| Newtonsoft.Json | JSON 序列化 |
| SharpZipLib | 压缩文件处理 |
| WebView2 | 网页内嵌组件 |

## 项目结构

```
GeminiLauncher/
├── Assets/           # 静态资源
├── Controls/         # 自定义控件
├── Converters/       # 数据绑定转换器
├── Models/           # 数据模型
│   └── Ecosystem/    # 模组生态模型
├── Resources/        # 多语言资源
├── Services/         # 业务服务层
│   ├── Animation/    # 动画服务
│   ├── Ecosystem/    # 模组生态服务
│   └── Network/      # 网络服务
├── Styles/           # 样式资源
├── ViewModels/       # 视图模型
└── Views/            # 视图页面
    └── Dialogs/      # 对话框
```

## 构建运行

### 环境要求
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 或更高版本

### 构建
```bash
git clone https://github.com/LinYiZhi-wp/LYZL-Minecraft-.git
cd LYZL-Minecraft-/GeminiLauncher
dotnet build
```

### 运行
```bash
dotnet run
```

## 版本

- **v1.1.0** - UI 全面升级：统一颜色系统、毛玻璃效果、标准化圆角、修复 UI 崩溃问题

## License

MIT License