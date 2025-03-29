using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace installer
{
    public static class DebugTool
    {
        private static string? _logFilePath;
        private static readonly object _lock = new object();
        private static bool _initialized = false;
        private static TextWriter? _logWriter;
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            try
            {
                _logFilePath = Path.Combine(AppContext.BaseDirectory, "installer_debug.log");
                _logWriter = new StreamWriter(_logFilePath, append: false) { AutoFlush = true };
                File.WriteAllText(_logFilePath, $"[{DateTime.Now}] 调试日志初始化\r\n");
                _initialized = true;
                
                LogSystemInfo();
                Log("调试工具初始化完成");
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "debug_error.log"), 
                        $"初始化调试工具失败: {ex.Message}\r\n{ex.StackTrace}");
                }
                catch
                {
                    // 无法写入日志
                }
            }
        }
        
        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                    Debug.WriteLine(logMessage);
                    
                    if (_logWriter != null)
                    {
                        _logWriter.WriteLine(logMessage);
                        _logWriter.Flush();
                    }
                    else if (!string.IsNullOrEmpty(_logFilePath) && File.Exists(_logFilePath))
                    {
                        File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                    }
                }
            }
            catch
            {
                // 记录日志出错，忽略
            }
        }
        
        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                string message = string.IsNullOrEmpty(context) 
                    ? $"异常: {ex.Message}" 
                    : $"异常({context}): {ex.Message}";
                    
                Log(message);
                Log($"堆栈跟踪: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Log($"内部异常: {ex.InnerException.Message}");
                    Log($"内部异常堆栈: {ex.InnerException.StackTrace}");
                }
            }
            catch
            {
                // 记录异常出错，忽略
            }
        }
        
        private static void LogSystemInfo()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("系统信息:");
                sb.AppendLine($"操作系统: {Environment.OSVersion}");
                sb.AppendLine($".NET 版本: {Environment.Version}");
                sb.AppendLine($"进程架构: {(Environment.Is64BitProcess ? "64位" : "32位")}");
                sb.AppendLine($"系统架构: {(Environment.Is64BitOperatingSystem ? "64位" : "32位")}");
                sb.AppendLine($"处理器数量: {Environment.ProcessorCount}");
                sb.AppendLine($"应用程序基目录: {AppContext.BaseDirectory}");
                
                Log(sb.ToString());
            }
            catch (Exception ex)
            {
                Log($"获取系统信息失败: {ex.Message}");
            }
        }
    }
} 