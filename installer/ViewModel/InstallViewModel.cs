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
                    && Directory.Exists(DownloadPath);
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
                Log.LogInfo("开始检查更新...");
                
                string targetDir = DownloadPath;
                
                // 1. 下载最新的hash.json获取版本信息
                string tempHashPath = Path.Combine(targetDir, "temp_hash.json");
                await Task.Run(() => Downloader.Cloud.DownloadFile(tempHashPath, "hash.json"));
                
                // 读取本地和远程的hash.json
                string localHashPath = Path.Combine(targetDir, "hash.json");
                string localHashContent = File.Exists(localHashPath) ? File.ReadAllText(localHashPath) : "{}";
                string remoteHashContent = File.ReadAllText(tempHashPath);
                
                // 提取版本信息
                string localVersion = ExtractVersion(localHashContent, "version");
                string remoteVersion = ExtractVersion(remoteHashContent, "version");
                
                string localCppVersion = ExtractVersion(localHashContent, "cpp_version");
                string remoteCppVersion = ExtractVersion(remoteHashContent, "cpp_version");
                
                string localPyVersion = ExtractVersion(localHashContent, "py_version");
                string remotePyVersion = ExtractVersion(remoteHashContent, "py_version");
                
                bool hasUpdates = false;
                List<string> updatesAvailable = new List<string>();
                
                // 检查各组件更新
                if (localVersion != remoteVersion)
                {
                    updatesAvailable.Add($"安装器: {localVersion} → {remoteVersion}");
                    hasUpdates = true;
                }
                
                if (localCppVersion != remoteCppVersion)
                {
                    updatesAvailable.Add($"C++模板: {localCppVersion} → {remoteCppVersion}");
                    hasUpdates = true;
                }
                
                if (localPyVersion != remotePyVersion)
                {
                    updatesAvailable.Add($"Python模板: {localPyVersion} → {remotePyVersion}");
                    hasUpdates = true;
                }
                
                // 输出更新信息
                if (hasUpdates)
                {
                    Log.LogInfo("发现可用更新:");
                    foreach (var update in updatesAvailable)
                    {
                        Log.LogInfo($"  - {update}");
                    }
                    Log.LogInfo("请点击\"更新\"按钮开始更新");
                    UpdateEnabled = true;
                }
                else
                {
                    Log.LogInfo("已是最新版本，无需更新");
                    UpdateEnabled = false;
                }
                
                // 删除临时文件
                if (File.Exists(tempHashPath))
                {
                    File.Delete(tempHashPath);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"检查更新失败: {ex.Message}");
                UpdateEnabled = false;
            }
            finally
            {
                BrowseEnabled = true;
                CheckEnabled = true;
                DownloadEnabled = true;
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
                DownloadEnabled = false;
                BrowseEnabled = false;

                // 创建下载目录结构
                string targetDir = DownloadPath;
                string templatesDir = Path.Combine(targetDir, "Templates");
                string setupDir = Path.Combine(targetDir, "Setup");
                string protoDir = Path.Combine(setupDir, "proto");
                
                // 确保目录存在
                Directory.CreateDirectory(templatesDir);
                Directory.CreateDirectory(protoDir);
                
                Log.LogInfo("开始下载文件...");
                
                // 1. 下载核心文件
                string hashFilePath = Path.Combine(targetDir, "hash.json");
                string mainPackagePath = Path.Combine(targetDir, "THUAI8.tar.gz");
                
                // 重置下载报告计数器
                Downloader.CloudReport.Total = 0;
                Downloader.CloudReport.Completed = 0;
                Downloader.CloudReport.ComCount = 0;
                Downloader.CloudReport.Count = 4; // 预计下载4个主要文件
                Downloader.CloudReport.BigFileTraceEnabled = true;
                
                // 1.1 下载哈希配置文件
                Log.LogInfo("下载配置文件 hash.json");
                try 
                {
                    await Task.Run(() => Downloader.Cloud.DownloadFile(hashFilePath, "hash.json"));
                    Downloader.CloudReport.ComCount++;
                    Log.LogInfo("配置文件下载完成");
                } 
                catch (Exception ex) 
                {
                    Log.LogError($"下载配置文件时出错: {ex.Message}");
                }
                
                // 1.2 下载主安装包
                Log.LogInfo("下载主安装包 THUAI8.tar.gz");
                try
                {
                    await Task.Run(() => Downloader.Cloud.DownloadFile(mainPackagePath, "THUAI8.tar.gz"));
                    Downloader.CloudReport.ComCount++;
                    Log.LogInfo("主安装包下载完成");
                }
                catch (Exception ex)
                {
                    Log.LogError($"下载主安装包时出错: {ex.Message}");
                }
                
                // 2. 下载最新的C++和Python模板文件
                string templateVersion = "1.0.0"; // 默认版本为1.0.0
                
                // 尝试从FileHashData获取版本
                if (Downloader.Data.FileHashData != null && 
                    Downloader.Data.FileHashData.TVersion != null)
                {
                    var version = Downloader.Data.FileHashData.TVersion.TemplateVersion;
                    templateVersion = $"{version.Major}.{version.Minor}.{version.Build}";
                }
                
                string cppTemplatePath = Path.Combine(templatesDir, $"t.{templateVersion}.cpp");
                string pyTemplatePath = Path.Combine(templatesDir, $"t.{templateVersion}.py");
                
                Log.LogInfo($"下载C++模板文件(版本:{templateVersion})");
                bool cppDownloadSuccess = false;
                try
                {
                    await Task.Run(() => Downloader.Cloud.DownloadFile(cppTemplatePath, $"Templates/t.{templateVersion}.cpp"));
                    Downloader.CloudReport.ComCount++;
                    cppDownloadSuccess = File.Exists(cppTemplatePath) && new FileInfo(cppTemplatePath).Length > 0;
                    if (cppDownloadSuccess)
                    {
                        Log.LogInfo("C++模板文件下载完成");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"下载C++模板文件时出错: {ex.Message}");
                }
                
                Log.LogInfo($"下载Python模板文件(版本:{templateVersion})");
                bool pyDownloadSuccess = false;
                try
                {
                    await Task.Run(() => Downloader.Cloud.DownloadFile(pyTemplatePath, $"Templates/t.{templateVersion}.py"));
                    Downloader.CloudReport.ComCount++;
                    pyDownloadSuccess = File.Exists(pyTemplatePath) && new FileInfo(pyTemplatePath).Length > 0;
                    if (pyDownloadSuccess)
                    {
                        Log.LogInfo("Python模板文件下载完成");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"下载Python模板文件时出错: {ex.Message}");
                }
                
                // 如果C++模板文件下载失败，尝试下载其他版本
                if (!cppDownloadSuccess)
                {
                    // 尝试备用版本格式
                    string[] backupVersions = new[] { "1.0.0", "1", "1.0", "1.0.0.0" };
                    
                    foreach (var ver in backupVersions)
                    {
                        if (ver == templateVersion) continue; // 跳过已尝试过的版本
                        
                        string backupPath = Path.Combine(templatesDir, $"t.{ver}.cpp");
                        Log.LogInfo($"尝试下载备用C++模板文件(版本:{ver})");
                        try
                        {
                            await Task.Run(() => Downloader.Cloud.DownloadFile(backupPath, $"Templates/t.{ver}.cpp"));
                            bool success = File.Exists(backupPath) && new FileInfo(backupPath).Length > 0;
                            if (success)
                            {
                                Log.LogInfo($"备用C++模板文件(版本:{ver})下载成功");
                                File.Copy(backupPath, cppTemplatePath, true);
                                cppDownloadSuccess = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"下载备用C++模板文件(版本:{ver})时出错: {ex.Message}");
                        }
                    }
                    
                    // 最后尝试不带版本号的模板
                    if (!cppDownloadSuccess)
                    {
                        string noVersionPath = Path.Combine(templatesDir, "t.cpp");
                        Log.LogInfo("尝试下载无版本号C++模板文件");
                        try
                        {
                            await Task.Run(() => Downloader.Cloud.DownloadFile(noVersionPath, "Templates/t.cpp"));
                            bool success = File.Exists(noVersionPath) && new FileInfo(noVersionPath).Length > 0;
                            if (success)
                            {
                                Log.LogInfo("无版本号C++模板文件下载成功");
                                File.Copy(noVersionPath, cppTemplatePath, true);
                                cppDownloadSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"下载无版本号C++模板文件时出错: {ex.Message}");
                        }
                    }
                }
                
                // 如果Python模板文件下载失败，尝试下载其他版本
                if (!pyDownloadSuccess)
                {
                    // 尝试备用版本格式
                    string[] backupVersions = new[] { "1.0.0", "1", "1.0", "1.0.0.0" };
                    
                    foreach (var ver in backupVersions)
                    {
                        if (ver == templateVersion) continue; // 跳过已尝试过的版本
                        
                        string backupPath = Path.Combine(templatesDir, $"t.{ver}.py");
                        Log.LogInfo($"尝试下载备用Python模板文件(版本:{ver})");
                        try
                        {
                            await Task.Run(() => Downloader.Cloud.DownloadFile(backupPath, $"Templates/t.{ver}.py"));
                            bool success = File.Exists(backupPath) && new FileInfo(backupPath).Length > 0;
                            if (success)
                            {
                                Log.LogInfo($"备用Python模板文件(版本:{ver})下载成功");
                                File.Copy(backupPath, pyTemplatePath, true);
                                pyDownloadSuccess = true;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"下载备用Python模板文件(版本:{ver})时出错: {ex.Message}");
                        }
                    }
                    
                    // 最后尝试不带版本号的模板
                    if (!pyDownloadSuccess)
                    {
                        string noVersionPath = Path.Combine(templatesDir, "t.py");
                        Log.LogInfo("尝试下载无版本号Python模板文件");
                        try
                        {
                            await Task.Run(() => Downloader.Cloud.DownloadFile(noVersionPath, "Templates/t.py"));
                            bool success = File.Exists(noVersionPath) && new FileInfo(noVersionPath).Length > 0;
                            if (success)
                            {
                                Log.LogInfo("无版本号Python模板文件下载成功");
                                File.Copy(noVersionPath, pyTemplatePath, true);
                                pyDownloadSuccess = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.LogError($"下载无版本号Python模板文件时出错: {ex.Message}");
                        }
                    }
                }
                
                // 创建空模板文件（如果所有下载都失败）
                if (!cppDownloadSuccess)
                {
                    Log.LogWarning("所有C++模板下载尝试均失败，创建空模板文件");
                    try
                    {
                        File.WriteAllText(cppTemplatePath, "// C++ 模板文件\n// 用户代码区域\n\n// 用户代码区域结束\n");
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"创建C++模板文件时出错: {ex.Message}");
                    }
                }
                
                if (!pyDownloadSuccess)
                {
                    Log.LogWarning("所有Python模板下载尝试均失败，创建空模板文件");
                    try
                    {
                        File.WriteAllText(pyTemplatePath, "# Python 模板文件\n# 用户代码区域\n\n# 用户代码区域结束\n");
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"创建Python模板文件时出错: {ex.Message}");
                    }
                }
                
                // 3. 下载C++ Proto库
                string protoCppPath = Path.Combine(protoDir, "protoCpp.tar.gz");
                Log.LogInfo("下载C++ Proto库");
                try
                {
                    await Task.Run(() => Downloader.Cloud.DownloadFile(protoCppPath, "Setup/proto/protoCpp.tar.gz"));
                    Log.LogInfo("C++ Proto库下载完成");
                }
                catch (Exception ex)
                {
                    Log.LogError($"下载C++ Proto库时出错: {ex.Message}");
                }
                
                // 4. 解压文件
                Log.LogInfo("开始解压文件...");
                
                // 4.1 解压主安装包
                Log.LogInfo("解压主安装包...");
                try {
                    await Task.Run(() => Downloader.Cloud.ArchieveUnzip(mainPackagePath, targetDir));
                    Log.LogInfo("主安装包解压完成");
                } catch (Exception ex) {
                    Log.LogError($"解压主安装包时出错: {ex.Message}");
                }
                
                // 4.2 解压Proto库
                Log.LogInfo("解压Proto库...");
                try {
                    Directory.CreateDirectory(Path.Combine(targetDir, "proto"));
                    await Task.Run(() => Downloader.Cloud.ArchieveUnzip(protoCppPath, Path.Combine(targetDir, "proto")));
                    Log.LogInfo("Proto库解压完成");
                } catch (Exception ex) {
                    Log.LogError($"解压Proto库时出错: {ex.Message}");
                }
                
                // 5. 创建选手代码文件
                string cppPlayerCodePath = Path.Combine(targetDir, "AI.cpp");
                string pyPlayerCodePath = Path.Combine(targetDir, "AI.py");
                
                // 如果不存在选手代码文件，则从模板创建
                if (!File.Exists(cppPlayerCodePath))
                {
                    Log.LogInfo("创建C++选手代码文件...");
                    File.Copy(cppTemplatePath, cppPlayerCodePath);
                }
                
                if (!File.Exists(pyPlayerCodePath))
                {
                    Log.LogInfo("创建Python选手代码文件...");
                    File.Copy(pyTemplatePath, pyPlayerCodePath);
                }
                
                // 6. 标记安装完成
                Installed = true;
                Downloader.Data.Installed = true;
                Downloader.Data.Config.SaveFile();
                
                Log.LogInfo("安装完成！");
            }
            catch (Exception ex)
            {
                Log.LogError($"下载或安装失败: {ex.Message}");
            }
            finally
            {
                _isDownloading = false;
                DownloadEnabled = true;
                BrowseEnabled = true;
                CheckEnabled = true;
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
                Log.LogInfo("开始检查更新...");
                
                string targetDir = DownloadPath;
                string templatesDir = Path.Combine(targetDir, "Templates");
                string setupDir = Path.Combine(targetDir, "Setup");
                
                // 确保目录存在
                Directory.CreateDirectory(templatesDir);
                Directory.CreateDirectory(setupDir);
                
                // 1. 下载最新的hash.json获取版本信息
                string tempHashPath = Path.Combine(targetDir, "temp_hash.json");
                await Task.Run(() => Downloader.Cloud.DownloadFile(tempHashPath, "hash.json"));
                
                // 读取本地和远程的hash.json
                string localHashPath = Path.Combine(targetDir, "hash.json");
                string localHashContent = File.Exists(localHashPath) ? File.ReadAllText(localHashPath) : "{}";
                string remoteHashContent = File.ReadAllText(tempHashPath);
                
                // 提取版本信息 (此处简化处理，实际应该使用JSON解析)
                // 假设版本信息格式为: {"version": "1.0", "cpp_version": "1.0", "py_version": "1.0"}
                string localVersion = ExtractVersion(localHashContent, "version");
                string remoteVersion = ExtractVersion(remoteHashContent, "version");
                
                string localCppVersion = ExtractVersion(localHashContent, "cpp_version");
                string remoteCppVersion = ExtractVersion(remoteHashContent, "cpp_version");
                
                string localPyVersion = ExtractVersion(localHashContent, "py_version");
                string remotePyVersion = ExtractVersion(remoteHashContent, "py_version");
                
                bool hasUpdates = false;
                
                // 2. 更新C++模板
                if (localCppVersion != remoteCppVersion)
                {
                    Log.LogInfo($"发现C++模板更新：{localCppVersion} -> {remoteCppVersion}");
                    
                    // 下载新模板
                    string newCppTemplatePath = Path.Combine(templatesDir, $"t.{remoteCppVersion}.cpp");
                    await Task.Run(() => Downloader.Cloud.DownloadFile(newCppTemplatePath, $"Templates/t.{remoteCppVersion}.cpp"));
                    
                    // 处理用户代码合并
                    await MergeUserCode(Path.Combine(targetDir, "AI.cpp"), 
                                        Path.Combine(templatesDir, $"t.{localCppVersion}.cpp"),
                                        newCppTemplatePath,
                                        "cpp");
                    
                    hasUpdates = true;
                }
                
                // 3. 更新Python模板
                if (localPyVersion != remotePyVersion)
                {
                    Log.LogInfo($"发现Python模板更新：{localPyVersion} -> {remotePyVersion}");
                    
                    // 下载新模板
                    string newPyTemplatePath = Path.Combine(templatesDir, $"t.{remotePyVersion}.py");
                    await Task.Run(() => Downloader.Cloud.DownloadFile(newPyTemplatePath, $"Templates/t.{remotePyVersion}.py"));
                    
                    // 处理用户代码合并
                    await MergeUserCode(Path.Combine(targetDir, "AI.py"), 
                                        Path.Combine(templatesDir, $"t.{localPyVersion}.py"),
                                        newPyTemplatePath,
                                        "py");
                    
                    hasUpdates = true;
                }
                
                // 4. 检查安装器更新
                if (localVersion != remoteVersion)
                {
                    Log.LogInfo($"发现安装器更新：{localVersion} -> {remoteVersion}");
                    
                    // 下载安装器更新包
                    string installerUpdatePath = Path.Combine(setupDir, $"Installer_v{remoteVersion}.zip");
                    await Task.Run(() => Downloader.Cloud.DownloadFile(installerUpdatePath, $"Setup/Installer_v{remoteVersion}.zip"));
                    
                    Log.LogInfo("安装器更新包已下载，将在下次启动时应用");
                    hasUpdates = true;
                }
                
                // 5. 更新hash.json
                if (hasUpdates)
                {
                    File.Copy(tempHashPath, localHashPath, true);
                    Log.LogInfo("更新完成！");
                }
                else
                {
                    Log.LogInfo("已是最新版本，无需更新");
                }
                
                // 6. 删除临时文件
                if (File.Exists(tempHashPath))
                {
                    File.Delete(tempHashPath);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"更新失败: {ex.Message}");
            }
            finally
            {
                BrowseEnabled = true;
                CheckEnabled = true;
                DownloadEnabled = true;
                UpdateEnabled = true;
            }
        }
        
        /// <summary>
        /// 从JSON内容中提取版本信息
        /// </summary>
        private string ExtractVersion(string jsonContent, string key)
        {
            // 简单解析，实际应使用JSON库
            int keyIndex = jsonContent.IndexOf($"\"{key}\"");
            if (keyIndex < 0) return "1.0"; // 默认版本
            
            int valueStart = jsonContent.IndexOf(":", keyIndex) + 1;
            int valueEnd = jsonContent.IndexOf(",", valueStart);
            if (valueEnd < 0) valueEnd = jsonContent.IndexOf("}", valueStart);
            
            if (valueStart >= 0 && valueEnd >= 0)
            {
                string version = jsonContent.Substring(valueStart, valueEnd - valueStart).Trim();
                // 移除引号
                version = version.Trim('"', ' ');
                return string.IsNullOrEmpty(version) ? "1.0" : version;
            }
            
            return "1.0"; // 默认版本
        }
        
        /// <summary>
        /// 合并用户代码与模板更新
        /// </summary>
        private async Task MergeUserCode(string userCodePath, string oldTemplatePath, string newTemplatePath, string language)
        {
            if (!File.Exists(userCodePath) || !File.Exists(oldTemplatePath) || !File.Exists(newTemplatePath))
            {
                Log.LogWarning($"合并{language}代码失败：文件缺失");
                return;
            }
            
            try
            {
                // 读取文件内容
                string userCode = File.ReadAllText(userCodePath);
                string oldTemplate = File.ReadAllText(oldTemplatePath);
                string newTemplate = File.ReadAllText(newTemplatePath);
                
                // 备份用户代码
                string backupPath = $"{userCodePath}.bak";
                File.Copy(userCodePath, backupPath, true);
                Log.LogInfo($"已备份用户{language}代码到 {backupPath}");
                
                // 标记区域
                string userCodeMarker = language == "cpp" ? "// 用户代码区域" : "# 用户代码区域";
                string endUserCodeMarker = language == "cpp" ? "// 用户代码区域结束" : "# 用户代码区域结束";
                
                // 提取用户代码区域
                int userStartIndex = userCode.IndexOf(userCodeMarker);
                int userEndIndex = userCode.IndexOf(endUserCodeMarker);
                
                if (userStartIndex >= 0 && userEndIndex > userStartIndex)
                {
                    // 提取用户自定义代码
                    string userCustomCode = userCode.Substring(
                        userStartIndex + userCodeMarker.Length, 
                        userEndIndex - userStartIndex - userCodeMarker.Length
                    );
                    
                    // 查找新模板中的用户代码区域
                    int newStartIndex = newTemplate.IndexOf(userCodeMarker);
                    int newEndIndex = newTemplate.IndexOf(endUserCodeMarker);
                    
                    if (newStartIndex >= 0 && newEndIndex > newStartIndex)
                    {
                        // 创建合并后的代码
                        string mergedCode = newTemplate.Substring(0, newStartIndex + userCodeMarker.Length) +
                                           userCustomCode +
                                           newTemplate.Substring(newEndIndex);
                        
                        // 保存合并后的代码
                        File.WriteAllText(userCodePath, mergedCode);
                        Log.LogInfo($"已成功合并{language}代码更新");
                    }
                    else
                    {
                        Log.LogWarning($"新{language}模板中未找到用户代码区域标记");
                        // 使用新模板替换，但保留警告
                        File.Copy(newTemplatePath, userCodePath, true);
                    }
                }
                else
                {
                    Log.LogWarning($"用户{language}代码中未找到代码区域标记，将使用新模板");
                    // 使用新模板替换
                    File.Copy(newTemplatePath, userCodePath, true);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"合并{language}代码时出错: {ex.Message}");
            }
        }
        #endregion
    }
}