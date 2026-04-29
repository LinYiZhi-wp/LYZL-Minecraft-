�	namespace GeminiLauncher.Models
{
    /// <summary>
    /// 版本设置配置
    /// </summary>
    public class VersionSettings
    {
        // 基本信息
        public string VersionId { get; set; } = "";
        public string CustomName { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string Category { get; set; } = "";

        // 启动选项
        public bool VersionIsolation { get; set; } = true;
        public string WindowTitle { get; set; } = "";
        public string CustomInfo { get; set; } = "";
        public string JavaPath { get; set; } = "";

        // 内存设置
        public MemoryAllocation MemoryMode { get; set; } = MemoryAllocation.FollowGlobal;
        public int MinMemoryMB { get; set; } = 512;
        public int MaxMemoryMB { get; set; } = 2048;

        // 命令
        public string PreLaunchCommand { get; set; } = "";
        public string PostExitCommand { get; set; } = "";
    }

    public enum MemoryAllocation
    {
        FollowGlobal,   // 跟随全局设置
        Auto,           // 自动配置
        Custom          // 自定义
    }
}
�	*cascade082Jfile:///C:/Users/Linyizhi/.gemini/GeminiLauncher/Models/VersionSettings.cs