using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using debug_interface.ViewModels;
using debug_interface.Views;
using System.Linq;

namespace debug_interface
{
    /// <summary>
    /// 应用程序的后置代码，负责初始化和启动主窗口。
    /// </summary>
    public partial class App : Application
    {
        //GPT GENERATED
        //public override void Initialize()
        //{
        //    AvaloniaXamlLoader.Load(this);
        //}

        //public override void OnFrameworkInitializationCompleted()
        //{
        //    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //    {
        //        // 创建主窗口的ViewModel实例
        //        var mainWindowViewModel = new MainWindowViewModel();

        //        // 设置主窗口及其数据上下文
        //        desktop.MainWindow = new MainWindow
        //        {
        //            DataContext = mainWindowViewModel
        //        };
        //    }

        //    base.OnFrameworkInitializationCompleted();
        //}


        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                var mainWindowViewModel = new MainWindowViewModel();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }


    }
}