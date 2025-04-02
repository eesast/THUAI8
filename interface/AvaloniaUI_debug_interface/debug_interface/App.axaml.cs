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
                // ��������
                var config = new ConfigData();

                // ������־Ŀ¼
                string logDir = Path.Combine(config.InstallPath, "Logs");
                Directory.CreateDirectory(logDir);

                // ������־��¼��
                var logFilePath = Path.Combine(logDir, $"debug_interface_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                var logger = new FileLogger(logFilePath);

                // ������������ͼģ��
                var mainWindowViewModel = new MainWindowViewModel(logger, config);

                // ����������ͨ�ŷ���
                var serverCommunicationService = new ServerCommunicationService(mainWindowViewModel, logger, config);

                // ����������
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                // �������ӷ������������ط�
                if (string.IsNullOrEmpty(config.Commands.PlaybackFile))
                {
                    // ���ӵ�������
                    _ = serverCommunicationService.ConnectToServer();
                }
                else
                {
                    // �����ط�ģʽ
                    serverCommunicationService.StartPlaybackMode();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}