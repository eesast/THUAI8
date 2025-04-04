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
                    // ��������
                    var config = new ConfigData();

                    // ������־Ŀ¼
                    string logDir = Path.Combine(config.InstallPath, "Logs");
                    Directory.CreateDirectory(logDir);

                    // ������־��¼��
                    var logFilePath = Path.Combine(logDir, $"debug_interface_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    var logger = LoggerProvider.FromFile(logFilePath);

                    // ������������ͼģ��
                    var mainWindowViewModel = new MainWindowViewModel();
                    mainWindowViewModel.myLogger = logger;

                    // ����������
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = mainWindowViewModel
                    };

                    // ע�⣺���ӷ��������߼��Ѿ��Ƶ���ViewModelBase�Ĺ��캯����
                    // ����Ҫ�ֶ�����ConnectToServer��StartPlaybackMode
                }
                catch (Exception ex)
                {
                    // �����ʼ�������г��ִ������ٳ��Դ���һ�������Ĵ���
                    Console.WriteLine($"��ʼ������: {ex.Message}");

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