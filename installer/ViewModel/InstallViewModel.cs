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
        public ObservableCollection<LogRecord> LogCollection { get => Log.List; }

        private Timer timer;
        protected ListLogger Log = new ListLogger();

        public InstallViewModel(IFolderPicker folderPicker, Downloader downloader)
        {
            Downloader = downloader;
            FolderPicker = folderPicker;

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
                    && Directory.Exists(value);
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
        private void CheckUpdBtnClicked()
        {
            // DebugAlert = "Check Button Clicked";
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;
            Downloader.CheckUpdateAsync().ContinueWith(r =>
            {
                var updated = r.Result;
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
                BrowseEnabled = true;
                CheckEnabled = true;
            });
        }
        public ICommand DownloadBtnClickedCommand { get; }
        private void DownloadBtnClicked()
        {
            // DebugAlert = "Download Button Clicked";
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;
            Task t;
            if (Installed)
            {
                t = Downloader.ResetInstallPathAsync(DownloadPath);
            }
            else
            {
                t = Downloader.InstallAsync(DownloadPath);
            }
            t.ContinueWith(_ =>
            {
                Installed = Downloader.Data.Installed;
                BrowseEnabled = true;
                CheckEnabled = true;
            });
        }
        public ICommand UpdateBtnClickedCommand { get; }
        private void UpdateBtnClicked()
        {
            // DebugAlert = "Update Button Clicked";
            BrowseEnabled = false;
            CheckEnabled = false;
            DownloadEnabled = false;
            UpdateEnabled = false;

            Downloader.UpdateAsync().ContinueWith(task =>
            {
                int result = task.Result;
                BrowseEnabled = true;
                
                if (result < 0)
                {
                    DebugAlert = "Update failed, please retry";
                    CheckEnabled = true;
                    UpdateEnabled = true;
                }
                else if ((result & 16) == 16)
                {
                    DebugAlert = "New installer downloaded, please exit and extract update";
                    CheckEnabled = true;
                    UpdateEnabled = false;
                }
                else if ((result & 8) == 8)
                {
                    DebugAlert = "Update completed";
                    CheckEnabled = true;
                    UpdateEnabled = false;
                    
                    Downloader.CheckUpdateAsync().ContinueWith(checkResult => {
                        UpdateEnabled = checkResult.Result;
                    });
                }
                else if (result > 0)
                {
                    DebugAlert = "Partial update completed, please check temp files";
                    CheckEnabled = true;
                    
                    Downloader.CheckUpdateAsync().ContinueWith(checkResult => {
                        UpdateEnabled = checkResult.Result;
                    });
                }
                else
                {
                    DebugAlert = "Already up to date";
                    CheckEnabled = true;
                    UpdateEnabled = false;
                }
            });
        }
        #endregion
    }
}