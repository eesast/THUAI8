// ViewModels/LogConsoleViewModel.cs
using System;
using System.Collections.ObjectModel;
using Avalonia.Threading; // 需要引入 Avalonia.Threading
using CommunityToolkit.Mvvm.ComponentModel;

namespace debug_interface.ViewModels
{
    // (可选) 定义一个更丰富的日志条目类
    public partial class LogEntry : ObservableObject
    {
        [ObservableProperty]
        private DateTime timestamp;
        [ObservableProperty]
        private string message = "";
        [ObservableProperty]
        private string level = "INFO"; // 可以添加级别 INFO, WARN, ERROR 等
        [ObservableProperty]
        private Avalonia.Media.IBrush color = Avalonia.Media.Brushes.Black; // 根据级别设置颜色
    }

    public partial class LogConsoleViewModel : ViewModelBase // 或者直接继承 ObservableObject
    {
        // 使用 ObservableCollection<LogEntry> 提供更丰富的显示
        [ObservableProperty]
        private ObservableCollection<LogEntry> logEntries = new();

        // (可选) 限制最大日志条目数量，防止内存无限增长
        private const int MaxLogEntries = 200;

        // 方法：添加日志条目 (确保在 UI 线程上操作 ObservableCollection)
        public void AddLog(string message, string level = "INFO")
        {
            // 使用 Dispatcher.UIThread.InvokeAsync 来确保线程安全
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var newEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Level = level,
                    Color = GetLevelColor(level) // 根据级别获取颜色
                };

                LogEntries.Add(newEntry);

                // 如果超过最大数量，移除最早的条目
                if (LogEntries.Count > MaxLogEntries)
                {
                    LogEntries.RemoveAt(0);
                }
            });
        }

        // 辅助方法：根据级别获取颜色
        private Avalonia.Media.IBrush GetLevelColor(string level)
        {
            return level switch
            {
                "INFO" => Avalonia.Media.Brushes.Black,
                "WARN" => Avalonia.Media.Brushes.Orange,
                "ERROR" => Avalonia.Media.Brushes.Red,
                "SKILL" => Avalonia.Media.Brushes.Blue, // 给技能释放一个特殊颜色
                _ => Avalonia.Media.Brushes.Gray,
            };
        }
    }
}