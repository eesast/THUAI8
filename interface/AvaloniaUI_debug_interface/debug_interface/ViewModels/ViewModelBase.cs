using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using installer.Model; // 使用现有的Logger

namespace debug_interface.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        // 使用既有的Logger
        protected readonly Logger? logger;

        // UI更新定时器
        protected DispatcherTimer? dispatcherTimer;

        // 基本构造函数
        public ViewModelBase()
        {
            // 子类可以初始化logger
        }

        // 带logger的构造函数
        public ViewModelBase(Logger logger)
        {
            this.logger = logger;
        }

        // 启动UI更新定时器的方法
        protected void StartUiUpdateTimer(double intervalMs = 50)
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }

            dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(intervalMs)
            };

            dispatcherTimer.Tick += OnTimerTick;
            dispatcherTimer.Start();

            logger?.LogInfo($"UI更新定时器已启动，间隔：{intervalMs}毫秒");
        }

        // 停止定时器
        protected void StopUiUpdateTimer()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= OnTimerTick;
                logger?.LogInfo("UI更新定时器已停止");
            }
        }

        // 子类应覆盖此方法来处理定时器事件
        protected virtual void OnTimerTick(object? sender, EventArgs e)
        {
            // 默认实现为空
        }

        // 在UI线程上执行操作的便捷方法
        protected async void RunOnUiThread(Action action)
        {
            await Dispatcher.UIThread.InvokeAsync(action);
        }
    }
}