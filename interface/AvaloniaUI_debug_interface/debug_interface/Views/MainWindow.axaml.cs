using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace debug_interface.Views
{    
    
    //public partial class MainWindow : Window
    //{
    //    public MainWindow()
    //    {
    //        InitializeComComponent();
    //    }
    //}

    /// <summary>
    /// 主窗口的后置代码，负责初始化组件。
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools(); // 在调试模式下附加开发者工具
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

}