using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace installer.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                Debug.WriteLine("正在初始化WinUI应用...");
                this.InitializeComponent();
                this.UnhandledException += App_UnhandledException;
                Debug.WriteLine("WinUI初始化完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WinUI初始化错误: {ex.Message}");
                Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine($"未处理的异常: {e.Exception.Message}");
            Debug.WriteLine($"堆栈跟踪: {e.Exception.StackTrace}");
        }

        protected override MauiApp CreateMauiApp()
        {
            try
            {
                Debug.WriteLine("开始创建MAUI应用...");
                var app = MauiProgram.CreateMauiApp();
                Debug.WriteLine("MAUI应用创建成功");
                return app;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"创建MAUI应用失败: {ex.Message}");
                Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                throw; // 重新抛出以便能够看到崩溃信息
            }
        }
    }
}
