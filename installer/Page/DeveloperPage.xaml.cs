using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using installer.Model;
using installer.Services;
using System.Formats.Tar;

namespace installer.Page
{
    public partial class DeveloperPage : ContentPage
    {
        private readonly Logger Log = LoggerProvider.FromFile("developer.log");
        private bool _resourceKeyGenerated = false;
        private string _encryptedKeyFilePath = string.Empty;
        private string keyFilePath = string.Empty;
        private bool resourceKeyGenerated = false;
        private Tencent_Cos cosClient;
        private string _baseDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string tempDir = Path.Combine(Path.GetTempPath(), "THUAI8_Temp");

        public string BaseDir 
        { 
            get => _baseDir; 
            set 
            { 
                _baseDir = value; 
                OnPropertyChanged(nameof(BaseDir)); 
            } 
        }

        public bool ResourceKeyGenerated
        {
            get => _resourceKeyGenerated;
            set
            {
                _resourceKeyGenerated = value;
                OnPropertyChanged(nameof(ResourceKeyGenerated));
            }
        }

        public DeveloperPage()
        {
            InitializeComponent();
            BindingContext = this;
            cosClient = new Tencent_Cos(
                appid: "1352014406", 
                region: "ap-beijing", 
                bucketName: "thuai8",
                _log: Log
            );
            
            // 创建临时目录
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            
            Log.LogInfo("开发者页面已加载");
            BaseDirLabel.Text = BaseDir;
            
            // 设置默认版本号
            VersionEntry.Text = "1.0.0";
        }

        private async void OnSelectBaseDirClicked(object sender, EventArgs e)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // 创建临时脚本来调用文件夹选择对话框
                    string scriptPath = Path.Combine(Path.GetTempPath(), "select_folder.ps1");
                    string script = @"
Add-Type -AssemblyName System.Windows.Forms
$folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
$folderBrowser.Description = '选择THUAI8源代码根目录'
$folderBrowser.RootFolder = 'MyComputer'
$folderBrowser.SelectedPath = '" + BaseDir.Replace("'", "''") + @"'
$folderBrowser.ShowNewFolderButton = $true
if ($folderBrowser.ShowDialog() -eq 'OK') {
    $folderBrowser.SelectedPath
} else {
    'cancelled'
}";
                    File.WriteAllText(scriptPath, script);

                    // 执行PowerShell脚本
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    // 清理临时文件
                    File.Delete(scriptPath);

                    // 处理选择结果
                    string selectedPath = output.Trim();
                    if (!string.IsNullOrEmpty(selectedPath) && selectedPath != "cancelled")
                    {
                        // 验证是否是THUAI8源代码目录
                        if (Directory.Exists(Path.Combine(selectedPath, "THUAI8")) || 
                            selectedPath.Contains("thuai8", StringComparison.OrdinalIgnoreCase))
                        {
                            BaseDir = selectedPath;
                            BaseDirLabel.Text = BaseDir;
                            LogOperation($"已选择源代码目录: {BaseDir}");
                        }
                        else
                        {
                            bool confirm = await DisplayAlert("警告", 
                                "所选目录可能不是THUAI8源代码目录。是否仍要使用此目录?", 
                                "是", "否");
                            
                            if (confirm)
                            {
                                BaseDir = selectedPath;
                                BaseDirLabel.Text = BaseDir;
                                LogOperation($"已选择源代码目录: {BaseDir}（警告：可能不是标准THUAI8目录）");
                            }
                        }
                    }
                }
                else
                {
                    // 非Windows平台使用文本输入
                    string result = await DisplayPromptAsync("选择源代码目录", 
                        "请输入THUAI8源代码根目录的完整路径:", 
                        initialValue: BaseDir);
                    
                    if (!string.IsNullOrEmpty(result) && Directory.Exists(result))
                    {
                        BaseDir = result;
                        BaseDirLabel.Text = BaseDir;
                        LogOperation($"已选择源代码目录: {BaseDir}");
                    }
                    else if (!string.IsNullOrEmpty(result))
                    {
                        await DisplayAlert("错误", "所选目录不存在", "确定");
                        LogOperation($"错误: 目录不存在: {result}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation($"选择目录失败: {ex.Message}", true);
                await DisplayAlert("错误", $"选择目录失败: {ex.Message}", "确定");
            }
        }

        // 生成密钥相关方法
        private async void OnGenerateKeyClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SecretIDEntry.Text) ||
                    string.IsNullOrWhiteSpace(SecretKeyEntry.Text) ||
                    string.IsNullOrWhiteSpace(EncryptionPasswordEntry.Text))
                {
                    await DisplayAlert("输入错误", "请填写所有必填字段", "确定");
                    return;
                }

                // 显示进度指示
                StatusLabel.Text = "正在生成密钥...";
                StatusLabel.TextColor = Colors.Blue;
                GenerateKeyButton.IsEnabled = false;

                // 在后台线程中执行密钥生成
                await Task.Run(() =>
                {
                    try
                    {
                        // 创建安全随机密钥和IV
                        using (Aes aes = Aes.Create())
                        {
                            // 从密码生成密钥
                            using (var deriveBytes = new Rfc2898DeriveBytes(
                                EncryptionPasswordEntry.Text,
                                new byte[16], // 静态盐，在解密时需要相同
                                10000,
                                HashAlgorithmName.SHA256))
                            {
                                aes.Key = deriveBytes.GetBytes(32); // 256位密钥
                                aes.IV = deriveBytes.GetBytes(16);  // 128位IV
                            }

                            // 加密SecretID和SecretKey
                            var encryptedSecretID = EncryptString(SecretIDEntry.Text, aes.Key, aes.IV);
                            var encryptedSecretKey = EncryptString(SecretKeyEntry.Text, aes.Key, aes.IV);

                            // 创建密钥文件内容
                            string resourceContent = $"{Convert.ToBase64String(aes.Key)}\n{Convert.ToBase64String(aes.IV)}\n{encryptedSecretID}\n{encryptedSecretKey}";

                            try
                            {
                                // 为嵌入式资源创建目录
                                string resourceDirPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Raw");
                                Directory.CreateDirectory(resourceDirPath);

                                // 保存密钥文件
                                _encryptedKeyFilePath = Path.Combine(resourceDirPath, "secured_key.csv");
                                File.WriteAllText(_encryptedKeyFilePath, resourceContent);

                                // 更新UI必须在主线程中执行
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    KeyFilePathLabel.Text = _encryptedKeyFilePath;
                                    ResourceKeyGenerated = true;
                                    StatusLabel.Text = "密钥已生成，请将其添加为嵌入式资源";
                                    StatusLabel.TextColor = Colors.Green;
                                    GenerateKeyButton.IsEnabled = true;
                                    
                                    // 更新COS客户端密钥
                                    cosClient.UpdateSecret(SecretIDEntry.Text, SecretKeyEntry.Text);
                                });
                            }
                            catch (Exception ex)
                            {
                                DebugTool.LogException(ex, "保存加密密钥文件");
                                MainThread.BeginInvokeOnMainThread(async () =>
                                {
                                    await DisplayAlert("错误", $"保存密钥文件失败: {ex.Message}", "确定");
                                    StatusLabel.Text = "保存密钥文件失败，请检查日志";
                                    StatusLabel.TextColor = Colors.Red;
                                    GenerateKeyButton.IsEnabled = true;
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugTool.LogException(ex, "生成密钥加密过程");
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("错误", $"加密密钥失败: {ex.Message}", "确定");
                            StatusLabel.Text = "加密密钥失败，请重试";
                            StatusLabel.TextColor = Colors.Red;
                            GenerateKeyButton.IsEnabled = true;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                // 处理UI线程异常
                await DisplayAlert("错误", $"生成密钥时出错: {ex.Message}", "确定");
                StatusLabel.Text = "密钥生成失败，请重试";
                StatusLabel.TextColor = Colors.Red;
                GenerateKeyButton.IsEnabled = true;
                DebugTool.LogException(ex, "生成密钥");
            }
        }

        private string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(
                        memoryStream,
                        aes.CreateEncryptor(),
                        CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        private void OnOpenFolderClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_encryptedKeyFilePath) && File.Exists(_encryptedKeyFilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{_encryptedKeyFilePath}\"");
                }
                else
                {
                    StatusLabel.Text = "密钥文件不存在，请先生成密钥";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"无法打开文件位置: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                DebugTool.LogException(ex, "打开文件位置");
            }
        }

        private async void OnCopyPathClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_encryptedKeyFilePath))
                {
                    await Clipboard.SetTextAsync(_encryptedKeyFilePath);
                    StatusLabel.Text = "路径已复制到剪贴板";
                    StatusLabel.TextColor = Colors.Green;
                }
                else
                {
                    StatusLabel.Text = "密钥文件不存在，请先生成密钥";
                    StatusLabel.TextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"无法复制路径: {ex.Message}";
                StatusLabel.TextColor = Colors.Red;
                DebugTool.LogException(ex, "复制路径");
            }
        }

        // 文件生成和上传相关方法
        private async void OnGenerateTemplatesClicked(object sender, EventArgs e)
        {
            LogOperation("开始生成选手模板文件...");
            
            try
            {
                string version = VersionEntry.Text.Trim();
                if (string.IsNullOrEmpty(version))
                {
                    await DisplayAlert("错误", "请输入有效的版本号", "确定");
                    return;
                }
                
                // 创建C++模板文件 - 使用正确的路径
                string cppTemplatePath = Path.Combine(tempDir, $"t.{version}.cpp");
                string cppSourcePath = Path.Combine(BaseDir, @"THUAI8\CAPI\cpp\API\src\AI.cpp");
                
                if (File.Exists(cppSourcePath))
                {
                    File.Copy(cppSourcePath, cppTemplatePath, true);
                    LogOperation($"C++模板文件已生成: {cppTemplatePath}");
                }
                else
                {
                    LogOperation($"错误: C++模板源文件不存在: {cppSourcePath}", true);
                }
                
                // 创建Python模板文件 - 使用正确的路径
                string pyTemplatePath = Path.Combine(tempDir, $"t.{version}.py");
                string pySourcePath = Path.Combine(BaseDir, @"THUAI8\CAPI\python\AI.py");
                
                if (File.Exists(pySourcePath))
                {
                    File.Copy(pySourcePath, pyTemplatePath, true);
                    LogOperation($"Python模板文件已生成: {pyTemplatePath}");
                }
                else
                {
                    LogOperation($"错误: Python模板源文件不存在: {pySourcePath}", true);
                }
                
                LogOperation("选手模板文件生成完成");
            }
            catch (Exception ex)
            {
                LogOperation($"生成选手模板文件时出错: {ex.Message}", true);
            }
        }
        
        private async void OnGenerateProtoClicked(object sender, EventArgs e)
        {
            LogOperation("开始生成Proto库文件...");
            
            try
            {
                // 源proto目录
                string protoSourceDir = Path.Combine(BaseDir, @"THUAI8\CAPI\cpp\proto");
                
                if (!Directory.Exists(protoSourceDir))
                {
                    LogOperation($"错误: 源proto目录不存在: {protoSourceDir}", true);
                    return;
                }
                
                // 创建临时proto目录
                string tempProtoDir = Path.Combine(tempDir, "proto");
                if (Directory.Exists(tempProtoDir))
                {
                    Directory.Delete(tempProtoDir, true);
                }
                Directory.CreateDirectory(tempProtoDir);
                
                // 复制proto文件
                foreach (string file in Directory.GetFiles(protoSourceDir))
                {
                    File.Copy(file, Path.Combine(tempProtoDir, Path.GetFileName(file)));
                }
                
                // 创建tar.gz文件 - 修改输出路径
                string protoCppPath = Path.Combine(tempDir, "Setup", "proto", "protoCpp.tar.gz");
                
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(protoCppPath));
                
                // 删除已存在的文件
                if (File.Exists(protoCppPath))
                {
                    File.Delete(protoCppPath);
                }
                
                // 创建tar.gz文件
                await CreateTarGzAsync(tempProtoDir, protoCppPath);
                
                LogOperation($"Proto库文件已生成: {protoCppPath}");
            }
            catch (Exception ex)
            {
                LogOperation($"生成Proto库文件时出错: {ex.Message}", true);
            }
        }
        
        private async void OnGenerateMainPackageClicked(object sender, EventArgs e)
        {
            LogOperation("开始生成主安装包...");
            
            try
            {
                // 要打包的目录
                string[] sourceDirs = new string[] 
                {
                    Path.Combine(BaseDir, @"THUAI8\logic"),
                    Path.Combine(BaseDir, @"THUAI8\interface"),
                    Path.Combine(BaseDir, @"THUAI8\dependency"),
                    Path.Combine(BaseDir, @"THUAI8\resource")
                };
                
                // 检查目录是否存在
                foreach (string dir in sourceDirs)
                {
                    if (!Directory.Exists(dir))
                    {
                        LogOperation($"错误: 源目录不存在: {dir}", true);
                        return;
                    }
                }
                
                // 创建临时主包目录
                string tempMainDir = Path.Combine(tempDir, "THUAI8");
                if (Directory.Exists(tempMainDir))
                {
                    Directory.Delete(tempMainDir, true);
                }
                Directory.CreateDirectory(tempMainDir);
                
                // 复制文件夹
                foreach (string dir in sourceDirs)
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    string destDir = Path.Combine(tempMainDir, dirName);
                    CopyDirectory(dir, destDir);
                    }
                    
                    // 创建tar.gz文件
                string mainPackagePath = Path.Combine(tempDir, "THUAI8.tar.gz");
                
                // 删除已存在的文件
                if (File.Exists(mainPackagePath))
                {
                    File.Delete(mainPackagePath);
                }
                
                // 创建tar.gz文件
                await CreateTarGzAsync(tempMainDir, mainPackagePath);
                
                LogOperation($"主安装包已生成: {mainPackagePath}");
            }
            catch (Exception ex)
            {
                LogOperation($"生成主安装包时出错: {ex.Message}", true);
            }
        }
        
        private async void OnGenerateHashClicked(object sender, EventArgs e)
        {
            LogOperation("开始生成Hash文件...");
            
            try
            {
                string version = VersionEntry.Text.Trim();
                if (string.IsNullOrEmpty(version))
                {
                    await DisplayAlert("错误", "请输入有效的版本号", "确定");
                    return;
                }
                
                // 要计算哈希的文件映射为COS上的路径
                var filesToHash = new Dictionary<string, string>
                {
                    { Path.Combine(tempDir, "THUAI8.tar.gz"), "THUAI8.tar.gz" },
                    { Path.Combine(tempDir, $"t.{version}.cpp"), $"Templates/t.{version}.cpp" },
                    { Path.Combine(tempDir, $"t.{version}.py"), $"Templates/t.{version}.py" },
                    { Path.Combine(tempDir, "Setup", "proto", "protoCpp.tar.gz"), "Setup/proto/protoCpp.tar.gz" }
                };
                
                // 检查文件是否存在并计算哈希
                bool allFilesExist = true;
                var rootObject = new Dictionary<string, object>();
                var filesDict = new Dictionary<string, string>();
                
                foreach (var file in filesToHash)
                {
                    if (!File.Exists(file.Key))
                    {
                        LogOperation($"错误: 文件不存在: {file.Key}", true);
                        allFilesExist = false;
                        continue;
                    }
                    
                    try
                    {
                        string hash = FileService.GetFileMd5Hash(file.Key);
                        if (string.IsNullOrEmpty(hash) || hash == "conflict")
                        {
                            LogOperation($"错误: 无法计算文件哈希: {file.Key}", true);
                            allFilesExist = false;
                        }
                        else
                        {
                            filesDict.Add(file.Value, hash);
                            LogOperation($"计算哈希: {file.Value} -> {hash}");
                        }
            }
            catch (Exception ex)
            {
                        LogOperation($"计算哈希时出错: {file.Key} - {ex.Message}", true);
                        allFilesExist = false;
                    }
                }
                
                if (!allFilesExist)
                {
                    LogOperation("由于部分文件缺失或哈希计算失败，无法生成完整的hash.json文件", true);
                    return;
                }
                
                string hashFilePath = Path.Combine(tempDir, "hash.json");
                
                // 调用新方法生成标准格式的hash.json
                GenerateMD5Json(hashFilePath, filesDict, version);
                
                LogOperation($"Hash文件已生成: {hashFilePath}");
            }
            catch (Exception ex)
            {
                LogOperation($"生成Hash文件时出错: {ex.Message}", true);
            }
        }

        private async void OnUploadAllClicked(object sender, EventArgs e)
        {
            LogOperation("开始上传所有文件...");
            
            try
            {
                string version = VersionEntry.Text.Trim();
                if (string.IsNullOrEmpty(version))
                {
                    await DisplayAlert("错误", "请输入有效的版本号", "确定");
                    return;
                }
                
                // 要上传的文件 - 修改protoCpp.tar.gz的路径
                var filesToUpload = new Dictionary<string, string>
                {
                    { Path.Combine(tempDir, "hash.json"), "hash.json" },
                    { Path.Combine(tempDir, "THUAI8.tar.gz"), "THUAI8.tar.gz" },
                    { Path.Combine(tempDir, $"t.{version}.cpp"), $"Templates/t.{version}.cpp" },
                    { Path.Combine(tempDir, $"t.{version}.py"), $"Templates/t.{version}.py" },
                    { Path.Combine(tempDir, "Setup", "proto", "protoCpp.tar.gz"), "Setup/proto/protoCpp.tar.gz" }
                };
                
                // 检查文件是否存在
                foreach (var kvp in filesToUpload)
                {
                    if (!File.Exists(kvp.Key))
                    {
                        LogOperation($"错误: 文件不存在: {kvp.Key}", true);
                        return;
                    }
                }
                
                // 上传文件并验证
                bool allUploaded = true;
                int totalFiles = filesToUpload.Count;
                int completedFiles = 0;
                var progressIndicator = new Progress<double>(value => 
                {
                    // 更新UI上的进度显示
                    double overallProgress = (completedFiles + value) / totalFiles;
                    UploadProgressBar.Progress = overallProgress;
                    UploadProgressLabel.Text = $"上传进度: {overallProgress:P0}";
                });
                
                // 注册全局上传进度事件
                cosClient.UploadReport.ProgressChanged += (sender, value) => 
                {
                    MainThread.BeginInvokeOnMainThread(() => 
                    {
                        string currentFile = filesToUpload.ElementAt(completedFiles).Value;
                        LogOperation($"上传中: {currentFile} - {value:P0}");
                    });
                };
                
                // 显示进度UI
                UploadProgressBar.IsVisible = true;
                UploadProgressLabel.IsVisible = true;
                UploadProgressBar.Progress = 0;
                
                foreach (var kvp in filesToUpload)
        {
            try
            {
                        LogOperation($"正在上传: {Path.GetFileName(kvp.Key)} -> {kvp.Value}");
                        await cosClient.UploadFileAsync(kvp.Key, kvp.Value, progressIndicator);
                        completedFiles++;
                        
                        // 验证上传是否成功
                        if (cosClient.DetectFile(kvp.Value))
                        {
                            LogOperation($"上传成功并验证: {kvp.Value}");
                        }
                        else
                        {
                            LogOperation($"上传失败: 无法验证文件 {kvp.Value}", true);
                            allUploaded = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogOperation($"上传文件时出错: {ex.Message}", true);
                        allUploaded = false;
                    }
                }
                
                // 隐藏进度UI
                UploadProgressBar.IsVisible = false;
                UploadProgressLabel.IsVisible = false;
                
                if (allUploaded)
                {
                    LogOperation("所有文件上传成功");
                    await DisplayAlert("成功", "所有文件已成功上传并验证", "确定");
                    
                    // 清理临时文件
                    try
                    {
                        foreach (var file in filesToUpload.Keys)
                        {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                                LogOperation($"已删除临时文件: {file}");
                            }
                        }
                        
                        // 清理临时目录
                        if (Directory.Exists(tempDir) && !Directory.EnumerateFileSystemEntries(tempDir).Any())
                        {
                            Directory.Delete(tempDir);
                            LogOperation($"已删除临时目录: {tempDir}");
                        }
            }
            catch (Exception ex)
            {
                        LogOperation($"清理临时文件时出错: {ex.Message}", true);
                    }
                }
                else
                {
                    LogOperation("部分文件上传失败", true);
                    await DisplayAlert("警告", "部分文件上传失败，请检查日志", "确定");
                }
            }
            catch (Exception ex)
            {
                LogOperation($"上传过程中出错: {ex.Message}", true);
                await DisplayAlert("错误", $"上传失败: {ex.Message}", "确定");
            }
        }
        
        private async void OnExitButtonClicked(object sender, EventArgs e)
        {
            try
            {
                bool answer = await DisplayAlert("确认", "确定要退出开发者模式吗？", "是", "否");
                if (answer)
                {
                    // 返回到主Shell
                    await Shell.Current.GoToAsync("//HelpPage");
                }
            }
            catch (Exception ex)
            {
                // 如果导航失败，记录错误并尝试使用其他导航方式
                DebugTool.LogException(ex, "退出开发者模式");
                try
                {
                    // 尝试返回
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // 忽略错误
                }
            }
        }

        // 辅助方法
        private void LogOperation(string message, bool isError = false)
        {
            if (isError)
            {
                Log.LogError(message);
                OperationLogLabel.Text += $"[错误] {message}\n";
            }
            else
            {
                Log.LogInfo(message);
                OperationLogLabel.Text += $"{message}\n";
            }
        }
        
        private async Task CreateTarGzAsync(string sourceDir, string outputFile)
        {
            try
            {
                // 创建临时tar文件
                string tempTarPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tar");
                
                try
                {
                    // 第一步：创建tar文件
                    using (FileStream tarFileStream = File.Create(tempTarPath))
                    {
                        TarFile.CreateFromDirectory(sourceDir, tarFileStream, false);
                    }

                    // 第二步：压缩tar文件为gz
                    using (FileStream tarFileStream = File.OpenRead(tempTarPath))
                    using (FileStream gzFileStream = File.Create(outputFile))
                    using (GZipStream gzipStream = new GZipStream(gzFileStream, CompressionMode.Compress))
                    {
                        await tarFileStream.CopyToAsync(gzipStream);
                    }

                    LogOperation($"压缩文件已创建: {outputFile}");
                }
                finally
                {
                    // 清理临时文件
                    if (File.Exists(tempTarPath))
                    {
                        File.Delete(tempTarPath);
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation($"创建压缩文件时出错: {ex.Message}", true);
                throw;
            }
        }
        
        private void CopyDirectory(string sourceDirName, string destDirName)
        {
            // 创建目标目录
            Directory.CreateDirectory(destDirName);
                
                // 复制文件
            foreach (string file in Directory.GetFiles(sourceDirName))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDirName, fileName);
                File.Copy(file, destFile, true);
            }

            // 递归复制子目录
            foreach (string dir in Directory.GetDirectories(sourceDirName))
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(destDirName, dirName);
                CopyDirectory(dir, destDir);
            }
        }

        // 创建MD5Json文件
        private void GenerateMD5Json(string targetPath, Dictionary<string, string> filesDict, string version)
        {
            try
            {
                // 解析版本号
                var versionParts = version.Split('.');
                Version versionObj = new Version(
                    versionParts.Length > 0 ? int.Parse(versionParts[0]) : 1,
                    versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0,
                    versionParts.Length > 2 ? int.Parse(versionParts[2]) : 0,
                    versionParts.Length > 3 ? int.Parse(versionParts[3]) : 0
                );
                
                // 创建MD5DataFile对象
                var md5Data = new Data.MD5DataFile
                {
                    Data = filesDict,
                    Version = versionObj,
                    TVersion = new Data.TVersion
                    {
                        LibVersion = new Version(1, 0, 2, 3),
                        TemplateVersion = versionObj,
                        InstallerVersion = new Version(1, 1, 0, 2)
                    },
                    Description = $"THUAI8安装文件版本 {version}",
                    BugFixed = "修复了已知问题",
                    BugGenerated = "可能包含新问题"
                };
                
                // 序列化并保存
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(md5Data, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(targetPath, jsonContent);
                LogOperation($"MD5数据文件已生成: {targetPath}");
            }
            catch (Exception ex)
            {
                LogOperation($"生成MD5数据文件时出错: {ex.Message}", true);
            }
        }
    }
}