using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using installer.Model;
using installer.Services;
using System.Runtime.InteropServices;
using System.Threading;

namespace installer.ViewModel
{
    public class InstallViewModel : BaseViewModel
    {
        private readonly Downloader Downloader;
        private readonly IFolderPicker FolderPicker;
        private readonly DownloadService _downloadService;
        private bool _isDownloading;
        private bool _isPaused;
        public ObservableCollection<LogRecord> LogCollection { get => Log.List; }

        private Timer timer;
        protected ListLogger Log = new ListLogger();

        public InstallViewModel(IFolderPicker folderPicker, Downloader downloader)
        {
            Downloader = downloader;
            FolderPicker = folderPicker;
            _downloadService = new DownloadService();

            DownloadPath = Downloader.Data.Config.InstallPath;
            Installed = Downloader.Data.Installed;

            timer = new Timer((_) =>
            {
                ProgressReport(null, new EventArgs());
            }, null, 0, 500);

            Downloader.Cloud.Log.Partner.Add(Log);
            Downloader.Data.Log.Partner.Add(Log);

            Installed = Downloader.Data.Installed;
            DownloadPath = Downloader.Data.Config.InstallPath;
            BrowseBtnClickedCommand = new RelayCommand(BrowseBtnClicked);
            CheckUpdBtnClickedCommand = new RelayCommand(CheckUpdBtnClicked);
            DownloadBtnClickedCommand = new RelayCommand(DownloadBtnClicked);
            UpdateBtnClickedCommand = new RelayCommand(UpdateBtnClicked);
            PauseResumeCommand = new RelayCommand(PauseResumeDownload);
            CancelCommand = new RelayCommand(CancelDownload);

            Downloader.CloudReport.PropertyChanged += ProgressReport;
        }

        private string? debugAlert;
        public string? DebugAlert
        {
            get => debugAlert;
            set
            {
                debugAlert = value;
                OnPropertyChanged();
            }
        }

        private string downloadPath = string.Empty;
        public string DownloadPath
        {
            get => downloadPath;
            set
            {
                downloadPath = value;
                DownloadEnabled =
                       !value.EndsWith(':') && !value.EndsWith('\\')
                    && Directory.Exists(value) && Local_Data.CountFile(value) == 0;
                CheckEnabled =
                       !value.EndsWith(':') && !value.EndsWith('\\')
                    && Directory.Exists(value) && Local_Data.CountFile(value) > 0;
                OnPropertyChanged();
            }
        }

        private bool installed;
        public bool Installed
        {
            get => installed;
            set
            {
                installed = value;
                DownloadBtnText = value ? "移动" : "下载";
                OnPropertyChanged();
            }
        }


        #region 进度报告区
        private double numPro = 0;
        public double NumPro
        {
            get => numPro;
            set
            {
                numPro = value;
                OnPropertyChanged();
            }
        }

        private string numReport = string.Empty;
        public string NumReport
        {
            get => numReport;
            set
            {
                numReport = value;
                OnPropertyChanged();
            }
        }

        private double filePro = 0;
        public double FilePro
        {
            get => filePro;
            set
            {
                filePro = value;
                OnPropertyChanged();
            }
        }

        private string fileReport = string.Empty;
        public string FileReport
        {
            get => fileReport;
            set
            {
                fileReport = value;
                OnPropertyChanged();
            }
        }

        private bool bigFileProEnabled = false;
        public bool BigFileProEnabled
        {
            get => bigFileProEnabled;
            set
            {
                bigFileProEnabled = value;
                OnPropertyChanged();
            }
        }

        private DateTime ProgressReportTime = DateTime.Now;
        private void ProgressReport(object? sender, EventArgs e)
        {
            if ((DateTime.Now - ProgressReportTime).TotalMilliseconds <= 100)
                return;
            var r = Downloader.CloudReport;
            NumPro = r.Count == 0 ? 1 : (double)r.ComCount / r.Count;
            NumReport = $"{r.ComCount} / {r.Count}";
            FilePro = r.Total == 0 ? 1 : (double)r.Completed / r.Total;
            FileReport = $"{FileService.GetFileSizeReport(r.Completed)} / {FileService.GetFileSizeReport(r.Total)}";
            BigFileProEnabled = r.BigFileTraceEnabled;
            ProgressReportTime = DateTime.Now;
        }
        #endregion


        #region 按钮
        private string downloadBtnText = string.Empty;
        public string DownloadBtnText
        {
            get => downloadBtnText;
            set
            {
                downloadBtnText = value;
                OnPropertyChanged();
            }
        }

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
        private bool checkEnabled = false;
        public bool CheckEnabled
        {
            get => checkEnabled;
            set
            {
                checkEnabled = value && Installed
                    && !DownloadPath.EndsWith(':') && !DownloadPath.EndsWith('\\')
                    && Directory.Exists(DownloadPath) && Local_Data.CountFile(DownloadPath) > 0; ;
                OnPropertyChanged();
            }
        }
        private bool downloadEnabled = false;
        public bool DownloadEnabled
        {
            get => downloadEnabled;
            set
            {
                downloadEnabled = value
                    && !DownloadPath.EndsWith(':') && !DownloadPath.EndsWith('\\')
                    && Directory.Exists(DownloadPath) && Local_Data.CountFile(DownloadPath) == 0;
                OnPropertyChanged();
            }
        }
        private bool updateEnabled = false;
        public bool UpdateEnabled
        {
            get => updateEnabled;
            set
            {
                updateEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseBtnClickedCommand { get; }
        private void BrowseBtnClicked()
        {
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;

            if ((OperatingSystem.IsWindows() && !string.IsNullOrEmpty(DownloadPath)) ||
                (OperatingSystem.IsMacCatalyst() && RuntimeInformation.OSDescription.Contains("14")))
            {
                FolderPicker.PickAsync(DownloadPath, CancellationToken.None).ContinueWith(result =>
                {
                    if (result.Result.IsSuccessful)
                    {
                        DownloadPath = result.Result.Folder.Path;
                    }
                    else
                    {
                        DownloadEnabled = true;
                        CheckEnabled = true;
                    }
                    BrowseEnabled = true;
                });
            }
            else
            {
                Log.LogWarning("当前平台不支持文件夹选择器");
                BrowseEnabled = true;
            }
        }
        public ICommand CheckUpdBtnClickedCommand { get; }
        private async void CheckUpdBtnClicked()
        {
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;

            try
            {
                var updated = await Downloader.CheckUpdateAsync();
                if (updated)
                {
                    DebugAlert = "Need to update.";
                    UpdateEnabled = true;
                }
                else
                {
                    DebugAlert = "Nothing to update.";
                    UpdateEnabled = false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"检查更新失败: {ex.Message}");
                DebugAlert = "检查更新失败";
                UpdateEnabled = false;
            }
            finally
            {
                BrowseEnabled = true;
                CheckEnabled = true;
            }
        }
        public ICommand DownloadBtnClickedCommand { get; }
        private async void DownloadBtnClicked()
        {
            if (_isDownloading)
            {
                return;
            }

            try
            {
                _isDownloading = true;
                CanPauseResume = true;
                CanCancel = true;
                DownloadEnabled = false;
                BrowseEnabled = false;

                var progress = new Progress<double>(value =>
                {
                    NumPro = value;
                    NumReport = $"下载进度: {value:P2}";
                });

                await _downloadService.DownloadFileAsync(
                    "https://thuai7-1319625962.cos.ap-beijing.myqcloud.com/THUAI8.tar.gz",
                    Path.Combine(DownloadPath, "THUAI8.zip"),
                    progress
                );

                Installed = true;
                Log.LogInfo("下载完成");
            }
            catch (Exception ex)
            {
                Log.LogError($"下载失败: {ex.Message}");
            }
            finally
            {
                _isDownloading = false;
                CanPauseResume = false;
                CanCancel = false;
                DownloadEnabled = true;
                BrowseEnabled = true;
            }
        }
        public ICommand UpdateBtnClickedCommand { get; }
        private async void UpdateBtnClicked()
        {
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;

            try
            {
                await Downloader.UpdateAsync();
            }
            catch (Exception ex)
            {
                Log.LogError($"更新失败: {ex.Message}");
            }
            finally
            {
                BrowseEnabled = true;
                CheckEnabled = true;
            }
        }
        public ICommand PauseResumeCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _canPauseResume;
        public bool CanPauseResume
        {
            get => _canPauseResume;
            set
            {
                _canPauseResume = value;
                OnPropertyChanged();
            }
        }

        private bool _canCancel;
        public bool CanCancel
        {
            get => _canCancel;
            set
            {
                _canCancel = value;
                OnPropertyChanged();
            }
        }

        private string _pauseResumeText = "暂停";
        public string PauseResumeText
        {
            get => _pauseResumeText;
            set
            {
                _pauseResumeText = value;
                OnPropertyChanged();
            }
        }

        private void PauseResumeDownload()
        {
            if (_isPaused)
            {
                _downloadService.Resume();
                PauseResumeText = "暂停";
            }
            else
            {
                _downloadService.Pause();
                PauseResumeText = "继续";
            }
            _isPaused = !_isPaused;
        }

        private void CancelDownload()
        {
            _downloadService.Cancel();
            _isDownloading = false;
            CanPauseResume = false;
            CanCancel = false;
            DownloadEnabled = true;
            BrowseEnabled = true;
            Log.LogInfo("下载已取消");
        }
        #endregion
    }
}