using System.Diagnostics;

namespace installer
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                
                Debug.WriteLine("正在初始化THUAI8安装程序...");
                MainPage = new AppShell();
                Debug.WriteLine("主页面已创建");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初始化错误: {ex.Message}");
                Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                
                // 尝试显示错误信息
                try
                {
                    MainPage = new ContentPage
                    {
                        Content = new VerticalStackLayout
                        {
                            Spacing = 10,
                            Padding = new Thickness(20),
                            Children =
                            {
                                new Label { Text = "应用程序初始化失败", FontSize = 24, HorizontalOptions = LayoutOptions.Center },
                                new Label { Text = ex.Message, FontSize = 16 },
                                new Label { Text = ex.StackTrace, FontSize = 12 }
                            }
                        }
                    };
                }
                catch
                {
                    // 无法显示错误页面
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            Debug.WriteLine("应用程序已启动");
        }
    }
}
