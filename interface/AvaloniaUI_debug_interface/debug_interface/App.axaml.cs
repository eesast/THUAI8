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
    /// Ӧ�ó���ĺ��ô��룬�����ʼ�������������ڡ�
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
        //        // ���������ڵ�ViewModelʵ��
        //        var mainWindowViewModel = new MainWindowViewModel();

        //        // ���������ڼ�������������
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