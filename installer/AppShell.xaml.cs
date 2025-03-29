using System.Diagnostics;

namespace installer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            try
            {
                InitializeComponent();
                Debug.WriteLine("AppShell组件已初始化");

                // 注册路由
                RegisterRoutes();
                Debug.WriteLine("路由已注册");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AppShell初始化错误: {ex.Message}");
                Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        private void RegisterRoutes()
        {
            // 注册所有页面路由
            Routing.RegisterRoute("InstallPage", typeof(Page.InstallPage));
            Routing.RegisterRoute("DebugPage", typeof(Page.DebugPage));
            Routing.RegisterRoute("PlaybackPage", typeof(Page.PlaybackPage));
            Routing.RegisterRoute("LoginPage", typeof(Page.LoginPage));
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            Debug.WriteLine($"正在导航到: {args.Target.Location}");
        }
    }
}
