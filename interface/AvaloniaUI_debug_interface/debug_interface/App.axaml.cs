using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using debug_interface.Services;
using debug_interface.ViewModels;
using debug_interface.Views;
using System;
using System.IO;
using installer.Model;
using installer.Data;

namespace debug_interface
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 加载配置
                var config = new ConfigData();

                // 设置日志目录
                string logDir = Path.Combine(config.InstallPath, "Logs");
                Directory.CreateDirectory(logDir);

                // 创建日志记录器
                var logFilePath = Path.Combine(logDir, $"debug_interface_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                var logger = new FileLogger(logFilePath);

                // 创建主窗口视图模型
                var mainWindowViewModel = new MainWindowViewModel(logger, config);

                // 创建服务器通信服务
                var serverCommunicationService = new ServerCommunicationService(mainWindowViewModel, logger, config);

                // 设置主窗口
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                // 尝试连接服务器或启动回放
                if (string.IsNullOrEmpty(config.Commands.PlaybackFile))
                {
                    // 连接到服务器
                    _ = serverCommunicationService.ConnectToServer();
                }
                else
                {
                    // 启动回放模式
                    serverCommunicationService.StartPlaybackMode();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}