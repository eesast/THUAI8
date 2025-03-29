using installer.ViewModel;
using installer.Model;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui.Storage;

namespace installer.Page;

public partial class PlaybackPage : ContentPage
{
    public PlaybackPage()
    {
        InitializeComponent();
        if (Application.Current?.Handler?.MauiContext != null)
        {
            var services = Application.Current.Handler.MauiContext.Services;
            var downloader = services.GetService<Downloader>();
            var filePicker = services.GetService<IFilePicker>() ?? FilePicker.Default;
            
            if (downloader != null)
            {
                this.BindingContext = new PlaybackViewModel(filePicker, downloader);
            }
            else
            {
                // 由于BaseViewModel是抽象类，我们使用一个空的ViewModel
                this.BindingContext = null;
                DisplayAlert("错误", "无法初始化回放服务", "确定");
            }
        }
    }
}