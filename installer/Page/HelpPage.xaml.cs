using System;
using System.Diagnostics;
using installer.ViewModel;

namespace installer.Page;

public partial class HelpPage : ContentPage
{
    private int _tapCount = 0;
    private DateTime _lastTapTime = DateTime.MinValue;

    public HelpPage(HelpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // 初始化点击计数器
        _tapCount = 0;
        _lastTapTime = DateTime.MinValue;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 页面显示时重置计数器
        _tapCount = 0;
        _lastTapTime = DateTime.MinValue;

        Debug.WriteLine("帮助页面已显示，点击计数器已重置");
    }

    private async void OnHeaderTapped(object sender, EventArgs e)
    {
        try
        {
            DateTime now = DateTime.Now;

            // 检查是否在3秒内点击
            if ((now - _lastTapTime).TotalSeconds <= 3)
            {
                _tapCount++;
                Debug.WriteLine($"帮助页面标题被点击，当前点击次数: {_tapCount}");

                // 给用户一些视觉反馈，但不明显
                await HeaderLabel.ScaleTo(1.05, 50);
                await HeaderLabel.ScaleTo(1.0, 50);

                if (_tapCount >= 5)
                {
                    _tapCount = 0;
                    Debug.WriteLine("触发开发者模式");

                    // 导航到开发者页面
                    await Shell.Current.GoToAsync("//DeveloperPage");
                }
            }
            else
            {
                // 超过3秒，重置计数器
                _tapCount = 1;
                Debug.WriteLine("点击计数器已重置（超时）");
            }

            _lastTapTime = now;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"处理标题点击错误: {ex.Message}");
        }
    }
}