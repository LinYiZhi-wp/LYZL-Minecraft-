# 版本设置对话框实现计划

## 功能概述

实现类似HMCL的版本设置对话框，允许用户对选中的游戏版本进行详细配置。

## UI设计

### 布局结构
```
┌─────────────────────────────────────────────┐
│  版本设置 - [版本名称]              [×]     │
├──────┬──────────────────────────────────────┤
│ 概览 │  版本信息卡片                        │
│      │  - 图标、版本名称、MC版本、加载器    │
│ 设置 │  - 快捷操作（重命名、删除）          │
│      │                                      │
│ Mod  │  个性化设置                          │
│ 管理 │  - 图标选择、分类                    │
│      │                                      │
│ 导出 │  启动选项                            │
│      │  - 版本隔离、游戏窗口标题            │
│      │  - 自定义信息、Java路径              │
│      │                                      │
│      │  游戏内存                            │
│      │  - 最小/最大内存滑块                 │
│      │                                      │
│      │  快捷方式                            │
│      │  - 打开文件夹快捷按钮                │
└──────┴──────────────────────────────────────┘
```

## Tab页内容

### 1. 概览 Tab
- **版本信息卡片**
  - 显示版本图标、名称
  - MC版本号、加载器类型和版本
- **个性化**
  - 图标选择（ComboBox）
  - 分类选择
  - 修改版本名按钮
  - 修改版本描述按钮
  - 加入收藏夹
- **快捷方式**
  - 版本文件夹按钮
  - 存档文件夹按钮
  - Mod文件夹按钮
- **高级管理**
  - 导出启动脚本
  - 补全文件
  - 删除版本（红色）

### 2. 设置 Tab
- **启动选项**
  - 版本隔离开关（开启/关闭）
  - 游戏窗口标题输入框
  - 自定义信息输入框
  - 游戏Java选择（ComboBox）
- **游戏内存**
  - 跟随全局设置 / 自动配置 / 自定义
  - 最小内存滑块
  - 最大内存滑块
  - 已使用/游戏分配显示条
- **启动前/后命令**（可选）
  - 执行命令输入框

### 3. Mod管理 Tab
- **顶部搜索栏**
  - 搜索Mod名称/描述/标签
- **操作按钮**
  - 打开文件夹
  - 从文件安装
  - 下载新Mod
  - 全选
- **Mod列表**
  - 显示已安装的Mod
  - 勾选框（启用/禁用）
  - Mod图标、名称、版本
  - 描述文字
- **分组显示**
  - 全部（数量）

### 4. 导出 Tab
- **导出内容选择**
  - 游戏本体
  - 游戏本体设置
  - Mod
  - Mod设置
  - 截图
  - 单人游戏存档
  - PCL启动器个性化配置
- **导出按钮**
  - "开始导出"蓝色按钮

## 数据模型

### VersionSettings
```csharp
public class VersionSettings
{
    // 基本信息
    public string VersionId { get; set; }
    public string CustomName { get; set; }
    public string Description { get; set; }
    public string IconPath { get; set; }
    public string Category { get; set; }
    
    // 启动选项
    public bool VersionIsolation { get; set; }
    public string WindowTitle { get; set; }
    public string CustomInfo { get; set; }
    public string JavaPath { get; set; }
    
    // 内存设置
    public MemoryAllocation MemoryMode { get; set; }
    public int MinMemoryMB { get; set; }
    public int MaxMemoryMB { get; set; }
    
    // 命令
    public string PreLaunchCommand { get; set; }
    public string PostExitCommand { get; set; }
}

public enum MemoryAllocation
{
    FollowGlobal,
    Auto,
    Custom
}
```

## 技术实现

### 文件结构
```
Views/
  VersionSettingsDialog.xaml      // 主对话框
  VersionSettingsDialog.xaml.cs   // 代码后台

ViewModels/
  VersionSettingsViewModel.cs     // ViewModel

Services/
  VersionConfigService.cs          // 版本配置读写服务
```

### 集成方式

在VersionSelectorDialog右键版本项 → 打开版本设置，或者在主页版本按钮旁添加"设置"图标按钮。

## 阶段实现

### Phase 1: 基础框架
- [ ] 创建VersionSettingsDialog XAML
- [ ] 实现左侧Tab导航
- [ ] 基础布局和样式

### Phase 2: 概览Tab
- [ ] 版本信息显示
- [ ] 个性化选项
- [ ] 快捷方式按钮

### Phase 3: 设置Tab
- [ ] 启动选项
- [ ] 内存设置UI
- [ ] 保存配置逻辑

### Phase 4: Mod管理Tab（简化版）
- [ ] Mod列表显示
- [ ] 启用/禁用功能
- [ ] 打开Mod文件夹

### Phase 5: 导出Tab
- [ ] 导出选项勾选
- [ ] 导出功能实现

## 优先级

**MVP（最小可用产品）：**
1. 概览Tab - 版本信息展示
2. 设置Tab - 启动选项和内存设置
3. 快捷方式 - 打开文件夹

**后续优化：**
4. Mod管理Tab
5. 导出功能
6. 高级管理功能
