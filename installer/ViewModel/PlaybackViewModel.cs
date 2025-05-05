using CommunityToolkit.Mvvm.Input;
using installer.Model;
using installer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Storage;
using System.Windows.Input;
using System.Timers;
using System.IO;
using installer.Services;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;

namespace installer.ViewModel
{
    public class PlaybackViewModel : LaunchViewModel
    {
        private bool _isDownloading = false;
        private readonly Microsoft.Maui.Storage.IFilePicker FilePicker;

        public PlaybackViewModel(Microsoft.Maui.Storage.IFilePicker filePicker, Downloader downloader) : base(downloader)
        {
            FilePicker = filePicker;

            options = new Microsoft.Maui.Storage.PickOptions()
            {
                FileTypes = new Microsoft.Maui.Storage.FilePickerFileType(
                    new Dictionary<Microsoft.Maui.Devices.DevicePlatform, IEnumerable<string>>
                    {
                        { Microsoft.Maui.Devices.DevicePlatform.Android, new [] { "application/com.thueesast.thuaiplayback" } },
                        { Microsoft.Maui.Devices.DevicePlatform.iOS, new [] { "com.thueesast.thuaiplayback" } },
                        { Microsoft.Maui.Devices.DevicePlatform.WinUI, new [] { ".thuaipb" } },
                        { Microsoft.Maui.Devices.DevicePlatform.MacCatalyst, new [] { "thuaipb" } }
                    }
                ),
                PickerTitle = "请选择回放文件"
            };

            // 初始化命令
            BrowseBtnClickedCommand = new RelayCommand(BrowseBtnClicked);
            PlaybackStartBtnClickedCommand = new AsyncRelayCommand(PlaybackStartBtnClicked);
            SaveBtnClickedCommand = new RelayCommand(SaveBtnClicked);

            // 设置默认回放文件
            if (string.IsNullOrEmpty(PlaybackFile))
            {
                PlaybackFile = "114514.thuaipb";
            }
        }

        #region 命令
        public ICommand BrowseBtnClickedCommand { get; }
        public IAsyncRelayCommand PlaybackStartBtnClickedCommand { get; }
        public ICommand SaveBtnClickedCommand { get; }
        #endregion

        protected Microsoft.Maui.Storage.PickOptions options;

        #region 属性
        private bool browseEnabled = true;
        public bool BrowseEnabled
        {
            get => browseEnabled;
            set
            {
                browseEnabled = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 方法
        private async Task PlaybackStartBtnClicked()
        {
            if (string.IsNullOrEmpty(PlaybackFile))
            {
                await Application.Current.MainPage.DisplayAlert("错误", "请输入回放文件路径", "确定");
                return;
            }

            LogMessage($"开始回放: {PlaybackFile}");

            // 保存设置
            SaveSettings();

            await Task.Run(() => LaunchPlayback());
        }

        private void BrowseBtnClicked()
        {
            BrowseEnabled = false;
            FilePicker.PickAsync(options).ContinueWith(result =>
            {
                if (result is not null)
                {
                    var p = result?.Result?.FullPath;
                    PlaybackFile = string.IsNullOrEmpty(p) ? PlaybackFile : p;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BrowseEnabled = true;
                });
            });
        }

        private void SaveBtnClicked()
        {
            SaveSettings();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("成功", "设置已保存", "确定");
            });
        }

        private void SaveSettings()
        {
            try
            {
                // 保存回放文件路径
                Downloader.Data.Config.Commands.PlaybackFile = PlaybackFile;

                // 正确处理速度的转换
                if (double.TryParse(PlaybackSpeed, out double speed))
                {
                    Downloader.Data.Config.Commands.PlaybackSpeed = speed;
                }
                else
                {
                    // 如果无法转换，使用默认值1.0
                    Downloader.Data.Config.Commands.PlaybackSpeed = 1.0;
                    // 更新UI中显示的值
                    PlaybackSpeed = "1.0";
                }

                Downloader.Data.Config.SaveFile();
                LogMessage("设置已保存");
            }
            catch (Exception ex)
            {
                LogMessage($"保存设置失败: {ex.Message}");
            }
        }

        public bool LaunchPlayback()
        {
            try
            {
                LogMessage("启动回放客户端");

                // 构建启动参数
                var clientPath = Path.Combine(Downloader.Data.Config.InstallPath, "logic", "Client", "debug_interface.exe");

                // 检查客户端是否存在
                if (!File.Exists(clientPath))
                {
                    LogMessage($"回放客户端不存在: {clientPath}");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("错误", $"回放客户端不存在: {clientPath}", "确定");
                    });
                    return false;
                }

                // 检查回放文件是否已设置
                string playbackFilePath = "";
                if (!string.IsNullOrEmpty(PlaybackFile))
                {
                    if (Path.IsPathRooted(PlaybackFile))
                    {
                        playbackFilePath = PlaybackFile;
                    }
                    else
                    {
                        // 先尝试从Server目录下找
                        string serverPath = Path.Combine(Downloader.Data.Config.InstallPath, "logic", "Server", PlaybackFile);
                        if (File.Exists(serverPath))
                        {
                            playbackFilePath = serverPath;
                        }
                        else
                        {
                            // 再尝试直接从安装目录找
                            playbackFilePath = Path.Combine(Downloader.Data.Config.InstallPath, PlaybackFile);
                        }
                    }
                }

                // 构造启动参数
                string arguments = "-b"; // 开启回放模式

                // 如果回放文件存在，添加文件参数
                if (!string.IsNullOrEmpty(playbackFilePath) && File.Exists(playbackFilePath))
                {
                    arguments += $" -f \"{playbackFilePath}\"";
                    LogMessage($"使用回放文件: {playbackFilePath}");
                }
                else
                {
                    LogMessage("未找到有效回放文件，将使用默认文件");
                }

                // 添加速度参数
                if (double.TryParse(PlaybackSpeed, out double speed))
                {
                    // 确保速度在合理范围内
                    speed = Math.Max(0.25, Math.Min(4.0, speed));
                    arguments += $" --playbackSpeed {speed}";
                    LogMessage($"设置回放速度: {speed}x");
                }

                // 构造启动过程
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = clientPath,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                // 启动客户端
                LogMessage($"启动命令: {clientPath} {arguments}");
                Process.Start(startInfo);
                LogMessage("回放客户端已启动");

                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"启动回放客户端失败: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("错误", $"启动回放客户端失败: {ex.Message}", "确定");
                });
                return false;
            }
        }

        private void LogMessage(string message)
        {
            Debug.WriteLine($"[PlaybackControl] {message}");
            try
            {
                // 直接调用DebugTool.Log，DebugTool会自行处理异常
                DebugTool.Log($"回放: {message}");
            }
            catch
            {
                // 忽略日志错误
            }
        }
        #endregion
    }
}
