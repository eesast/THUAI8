//App.axaml.cs
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

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
                try
                {
                    // 加载配置
                    var config = new ConfigData();

                    // 设置日志目录
                    string logDir = Path.Combine(config.InstallPath, "Logs");
                    Directory.CreateDirectory(logDir);

                    // 创建日志记录器
                    var logFilePath = Path.Combine(logDir, $"debug_interface_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    var logger = LoggerProvider.FromFile(logFilePath);

                    // 创建主窗口视图模型
                    var mainWindowViewModel = new MainWindowViewModel();
                    mainWindowViewModel.myLogger = logger;

                    // 设置主窗口
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };

                    // 注意：连接服务器的逻辑已经移到了ViewModelBase的构造函数中
                    // 不需要手动调用ConnectToServer或StartPlaybackMode
                }
                catch (Exception ex)
                {
                    // 如果初始化过程中出现错误，至少尝试创建一个基本的窗口
                    Console.WriteLine($"初始化出错: {ex.Message}");

                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel()
                    };
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}