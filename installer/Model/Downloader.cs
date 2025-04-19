using COSXML.CosException;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Formats.Tar;
using installer.Data;
using installer.Services;

namespace installer.Model
{
    public struct UpdateInfo                                         // 更新信息，包括新版本版本号、更改文件数和新文件数
    {
        public string status;
        public int changedFileCount;
        public int newFileCount;
    }

    public class Downloader
    {
        #region 属性区
        public class UserInfo
        {
            public string _id = "";
            public string email = "";
        }
        public string ProgramName = "THUAI8";               // 要运行或下载的程序名称
        public string StartName = "maintest.exe";           // 启动的程序名
        public Local_Data Data;                             // 本地文件管理器
        public Tencent_Cos Cloud;                           // THUAI8 Cos桶
        public TVersion CurrentVersion { get => Data.CurrentVersion; set => Data.CurrentVersion = value; }

        public HttpClient Client = new HttpClient();
        public EEsast Web;                                  // EEsast服务器
        public Logger Log;                                  // 日志管理器
        public ListLogger LogList = new ListLogger();
        public enum UpdateStatus
        {
            success, unarchieving, downloading, hash_computing, exiting, error
        }
        public UpdateStatus Status = UpdateStatus.success;                         // 当前工作状态

        public string Route { get; set; }
        public string Username { get => Web.Username; set { Web.Username = value; } }
        public string Password { get => Web.Password; set { Web.Password = value; } }
        public Data.Command Commands
        {
            get => Data.Config.Commands;
            set
            {
                Data.Config.Commands = value;
            }
        }
        public enum UsingOS { Win, Linux, OSX };
        public UsingOS usingOS { get; set; }
        public class Updater
        {
            public string Message = string.Empty;
            public bool Working { get; set; }
            public bool CombatCompleted { get => false; }
            public bool UploadReady { get; set; } = false;
            public bool ProfileAvailable { get; set; }
        }
        public bool LoginFailed { get; set; } = false;
        public bool RememberMe { get => Data.RememberMe; set { Data.RememberMe = value; } }

        public DownloadReport CloudReport { get => Cloud.Report; }
        #endregion

        #region 方法区
        public Downloader()
        {
            Data = new Local_Data();
            Log = LoggerProvider.FromFile(Path.Combine(Data.LogPath, "Installer.log"));
            long size = 0;
            foreach (var log in new DirectoryInfo(Data.LogPath).EnumerateFiles())
            {
                size += log.Length;
            }
            // 检测到最近日志为上个月或日志总量达到10MB以上时压缩logs
            if ((Log.LastRecordTime != DateTime.MinValue && DateTime.Now.Month != Log.LastRecordTime.Month)
                || size >= (10 << 20))
            {
                string tardir = Path.Combine(Data.Config.InstallPath, "LogArchieved");
                if (!Directory.Exists(tardir))
                    Directory.CreateDirectory(tardir);
                string tarPath = Path.Combine(tardir, $"LogBackup_{DateTime.Now:yyyy_MM_dd_HH_mm}.tar");
                if (File.Exists(tarPath))
                    File.Delete(tarPath);
                if (File.Exists(tarPath + ".gz"))
                    File.Delete(tarPath + ".gz");
                TarFile.CreateFromDirectory(Data.LogPath, tarPath, false);
                using (FileStream tar = File.Open(tarPath, FileMode.Open))
                using (FileStream gz = File.Create(tarPath + ".gz"))
                using (var compressor = new GZipStream(gz, CompressionMode.Compress))
                {
                    tar.CopyTo(compressor);
                }
                File.Delete(tarPath);
                foreach (var log in Directory.EnumerateFiles(Data.LogPath))
                {
                    File.Delete(log);
                }
                if (Data.Log is FileLogger) ((FileLogger)Data.Log).Path = ((FileLogger)Data.Log).Path;
                if (Log is FileLogger) ((FileLogger)Log).Path = ((FileLogger)Log).Path;
            }
            Route = Data.Config.InstallPath;
            Cloud = new Tencent_Cos("1352014406", "ap-beijing", "thuai8");
            Web = new EEsast();
            Web.Token_Changed += SaveToken;

            Data.Log.Partner.Add(Log);
            Cloud.Log.Partner.Add(Log);
            Web.Log.Partner.Add(Log);
            Log.Partner.Add(LogList);

            if (Data.Config.Remembered)
            {
                Username = Data.Config.UserName;
                Password = Data.Config.Password;
            }
            Cloud.UpdateSecret(MauiProgram.SecretID, MauiProgram.SecretKey);
        }

        public void UpdateMD5()
        {
            if (File.Exists(Data.MD5DataPath))
                File.Delete(Data.MD5DataPath);
            Log.LogInfo($"正在下载校验文件……");
            Status = UpdateStatus.downloading;
            Log.CountDict[LogLevel.Error] = 0;
            (CloudReport.ComCount, CloudReport.Count) = (0, 1);
            Cloud.DownloadFile(Data.MD5DataPath, "hash.json");
            if (Log.CountDict[LogLevel.Error] > 0)
            {
                Status = UpdateStatus.error;
                return;
            }
            CloudReport.ComCount = 1;
            Data.ReadMD5Data();
            Status = UpdateStatus.success;
        }

        /// <summary>
        /// 已有安装目录时移动安装目录到其他位置
        /// </summary>
        /// <param name="newPath">新的THUAI8根目录</param>
        public void ResetInstallPath(string newPath)
        {
            newPath = newPath.EndsWith(Path.DirectorySeparatorChar) ? newPath[0..-1] : newPath;
            var installPath = Data.Config.InstallPath.EndsWith(Path.DirectorySeparatorChar) ? Data.Config.InstallPath[0..-1] : Data.Config.InstallPath;
            if (newPath != installPath)
            {
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
                if (Directory.Exists(Path.Combine(newPath, "Logs")))
                {
                    Directory.Delete(Path.Combine(newPath, "Logs"), true);
                }
                Directory.CreateDirectory(Path.Combine(newPath, "Logs"));
                foreach (var f1 in Directory.EnumerateFiles(Path.Combine(installPath, "Logs")))
                {
                    var m = FileService.ConvertAbsToRel(installPath, f1);
                    var n = Path.Combine(newPath, m);
                    File.Move(f1, n);
                }
                if (Cloud.Log is FileLogger) ((FileLogger)Cloud.Log).Path = Path.Combine(newPath, "Logs", "TencentCos.log");
                if (Web.Log is FileLogger) ((FileLogger)Web.Log).Path = Path.Combine(newPath, "Logs", "EESAST.log");
                if (Data.Log is FileLogger) ((FileLogger)Data.Log).Path = Path.Combine(newPath, "Logs", "Local_Data.log");
                if (Log is FileLogger) ((FileLogger)Log).Path = Path.Combine(newPath, "Logs", "Installer.log");
                Data.ResetInstallPath(newPath);
            }
            Update();
        }

        /// <summary>
        /// 全新安装
        /// </summary>
        public void Install(string? path = null)
        {
            Data.Installed = false;
            Data.Config.InstallPath = path ?? Data.Config.InstallPath;
            UpdateMD5();
            if (Status == UpdateStatus.error)
            {
                Cloud.Log.LogError($"校验文件下载失败，退出安装。");
                return;
            }

            Log.CountDict[LogLevel.Error] = 0;
            Action<DirectoryInfo> action = (dir) => { };
            var deleteTask = (DirectoryInfo dir) =>
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    if (!Local_Data.IsUserFile(file.FullName))
                        file.Delete();
                }
                foreach (var sub in dir.EnumerateDirectories())
                {
                    action(sub);
                }
            };
            action = deleteTask;
            Data.Log.LogWarning($"全新安装开始，所有位于{Data.Config.InstallPath}的文件都将被删除。");
            if (Directory.Exists(Data.Config.InstallPath))
                deleteTask(new DirectoryInfo(Data.Config.InstallPath));
            else
                Directory.CreateDirectory(Data.Config.InstallPath);
            if (Directory.Exists(Path.Combine(Data.Config.InstallPath, "Logs")))
            {
                Directory.Delete(Path.Combine(Data.Config.InstallPath, "Logs"), true);
            }
            Directory.CreateDirectory(Path.Combine(Data.Config.InstallPath, "Logs"));
            if (Cloud.Log is FileLogger) ((FileLogger)Cloud.Log).Path = Path.Combine(Data.Config.InstallPath, "Logs", "TencentCos.log");
            if (Web.Log is FileLogger) ((FileLogger)Web.Log).Path = Path.Combine(Data.Config.InstallPath, "Logs", "EESAST.log");
            if (Data.Log is FileLogger) ((FileLogger)Data.Log).Path = Path.Combine(Data.Config.InstallPath, "Logs", "Local_Data.log");
            if (Log is FileLogger) ((FileLogger)Log).Path = Path.Combine(Data.Config.InstallPath, "Logs", "Installer.log");
            Data.ResetInstallPath(Data.Config.InstallPath);

            string zp = Path.Combine(Data.Config.InstallPath, "THUAI8.tar.gz");
            Status = UpdateStatus.downloading;
            (CloudReport.ComCount, CloudReport.Count) = (0, 1);
            Cloud.Log.LogInfo($"正在下载installer安装包……");
            Cloud.DownloadFileAsync(zp, "THUAI8.tar.gz").Wait();
            CloudReport.ComCount = 1;
            Status = UpdateStatus.unarchieving;
            Cloud.Log.LogInfo($"installer安装包下载完毕，正在解压……");
            Cloud.ArchieveUnzip(zp, Data.Config.InstallPath);
            Cloud.Log.LogInfo($"installer解压完成");
            File.Delete(zp);

            CurrentVersion = Data.FileHashData.TVersion;
            Cloud.Log.LogInfo("正在下载选手代码……");
            Status = UpdateStatus.downloading;
            CloudReport.Count += 2;
            var tocpp = Cloud.DownloadFileAsync(Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "src", "AI.cpp"),
                $"./Templates/t.{CurrentVersion.TemplateVersion}.cpp").ContinueWith(_ => CloudReport.ComCount++);
            var topy = Cloud.DownloadFileAsync(Path.Combine(Data.Config.InstallPath, "CAPI", "python", "PyAPI", "AI.py"),
                $"./Templates/t.{CurrentVersion.TemplateVersion}.py").ContinueWith(_ => CloudReport.ComCount++);
            Task.WaitAll(tocpp, topy);

            Cloud.Report.Count += 1;
            zp = Path.Combine(Data.Config.InstallPath, "protoCpp.tar.gz");
            Cloud.Log.LogInfo("正在下载proto cpp库……");
            Cloud.DownloadFileAsync(zp, "Setup/proto/protoCpp.tar.gz").Wait();
            CloudReport.ComCount += 1;
            Status = UpdateStatus.unarchieving;
            Cloud.Log.LogInfo($"proto cpp库下载完毕，正在解压……");
            var protoCppLibPath = Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib");
            if (!Directory.Exists(protoCppLibPath))
                Directory.CreateDirectory(protoCppLibPath);
            Cloud.ArchieveUnzip(zp, protoCppLibPath);
            Cloud.Log.LogInfo($"proto cpp库解压完成");
            File.Delete(zp);

            if (CloudReport.ComCount == CloudReport.Count)
            {
                Cloud.Log.LogInfo("选手代码下载成功！");
            }
            else
            {
                Cloud.Log.LogError("选手代码下载失败，选手可自行下载，网址：https://github.com/eesast/THUAI8/tree/main/CAPI/cpp/API/src/AI.cpp，https://github.com/eesast/THUAI8/tree/main/CAPI/python/PyAPI/AI.py");
            }

            Status = UpdateStatus.hash_computing;
            Data.Log.LogInfo($"正在校验……");
            Data.MD5Update.Clear();
            Data.ScanDir();
            if (Data.MD5Update.Count != 0)
            {
                Status = UpdateStatus.error;
                Data.Log.LogInfo($"校验失败，试图进行升级以修复……");
                Update();
            }
            else
            {
                Status = UpdateStatus.success;
                Cloud.Log.LogInfo($"安装成功！开始您的THUAI8探索之旅吧！");
                Data.Installed = true;
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = Data.Config.InstallPath,
                        FileName = "explorer.exe"
                    });
                }
            }
        }

        /// <summary>
        /// 检测是否需要进行更新
        /// 返回真时则表明需要更新
        /// </summary>
        /// <returns></returns>
        public bool CheckUpdate(bool writeMD5 = true)
        {
            UpdateMD5();
            Data.MD5Update.Clear();
            Data.Log.LogInfo("校验文件中……");
            Status = UpdateStatus.hash_computing;
            Data.ScanDir(false);
            Status = UpdateStatus.success;
            if (Data.MD5Update.Count != 0 || CurrentVersion < Data.FileHashData.TVersion)
            {
                Data.Log.LogInfo("代码库需要更新，请点击更新按钮以更新。");
                if (writeMD5)
                {
                    Data.SaveMD5Data();
                }
                return true;
            }
            else if (!Data.LangEnabled[LanguageOption.cpp].Item1 || !Data.LangEnabled[LanguageOption.python].Item1)
            {
                Data.Log.LogInfo("未检测到选手代码，请点击更新按钮以修复。");
                if (writeMD5)
                {
                    Data.SaveMD5Data();
                }
                return true;
            }
            else if (!Directory.Exists(Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib")))
            {
                Data.Log.LogInfo("未检测到proto cpp库，请点击更新按钮以修复。");
                if (writeMD5)
                {
                    Data.SaveMD5Data();
                }
                return true;
            }
            else
            {
                Data.Log.LogInfo("您的版本已经是最新版本！");
                if (writeMD5)
                {
                    Data.SaveMD5Data();
                }
                return false;
            }
        }

        /// <summary>
        /// 更新文件
        /// </summary>
        public int Update()
        {
            int result = 0;
            if (CheckUpdate(false))
            {
                // 如果缺少选手代码，应当立刻下载最新的选手代码
                if (!Data.LangEnabled[LanguageOption.cpp].Item1)
                {
                    Log.LogWarning("已检测到选手包cpp代码缺失。");
                    CloudReport.Count++;
                    var tocpp = Cloud.DownloadFileAsync(Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "src", "AI.cpp"),
                        $"./Templates/t.{CurrentVersion.TemplateVersion}.cpp").ContinueWith(_ => CloudReport.ComCount++);
                    tocpp.Wait();
                    if (CloudReport.ComCount == CloudReport.Count)
                    {
                        Cloud.Log.LogInfo("选手包cpp代码下载成功！");
                        Data.LangEnabled[LanguageOption.cpp] = (true, Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "src", "AI.cpp"));
                    }
                    else
                    {
                        Cloud.Log.LogError("选手包cpp代码下载失败！");
                        Data.SaveMD5Data();
                        return -1;
                    }
                }
                if (!Data.LangEnabled[LanguageOption.python].Item1)
                {
                    Log.LogWarning("已检测到选手包py代码缺失。");
                    CloudReport.Count++;
                    var topy = Cloud.DownloadFileAsync(Path.Combine(Data.Config.InstallPath, "CAPI", "python", "PyAPI", "AI.py"),
                        $"./Templates/t.{CurrentVersion.TemplateVersion}.py").ContinueWith(_ => CloudReport.ComCount++);
                    topy.Wait();
                    if (CloudReport.ComCount == CloudReport.Count)
                    {
                        Cloud.Log.LogInfo("选手包py代码下载成功！");
                        Data.LangEnabled[LanguageOption.python] = (true, Path.Combine(Data.Config.InstallPath, "CAPI", "python", "PyAPI", "AI.py"));
                    }
                    else
                    {
                        Cloud.Log.LogError("选手包py代码下载失败！");
                        Data.SaveMD5Data();
                        return -1;
                    }
                }
                // 如果缺少proto cpp库，应当立刻下载
                if (!Directory.Exists(Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib")))
                {
                    Cloud.Report.Count += 1;
                    string zp = Path.Combine(Data.Config.InstallPath, "protoCpp.tar.gz");
                    Cloud.Log.LogInfo("已检测到proto cpp库缺失，正在下载……");
                    Cloud.DownloadFileAsync(zp, "Setup/proto/protoCpp.tar.gz").Wait();
                    CloudReport.ComCount += 1;
                    Status = UpdateStatus.unarchieving;
                    Cloud.Log.LogInfo($"proto cpp库下载完毕，正在解压……");
                    var protoCppLibPath = Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib");
                    if (!Directory.Exists(protoCppLibPath))
                        Directory.CreateDirectory(protoCppLibPath);
                    Cloud.ArchieveUnzip(zp, protoCppLibPath);
                    Cloud.Log.LogInfo($"proto cpp库解压完成");
                    File.Delete(zp);
                }

                // 如果是更新模板，应当尝试使用AI.cpp.temp和AI.py.temp来存储选手原来的AI代码
                if (CurrentVersion < Data.FileHashData.TVersion &&
                    Data.LangEnabled[LanguageOption.cpp].Item1 && Data.LangEnabled[LanguageOption.python].Item1 &&
                    CurrentVersion.TemplateVersion < Data.FileHashData.TVersion.TemplateVersion)
                {
                    Log.LogInfo("待更新：新的选手代码模板");
                    // 对于C++ API
                    {
                        var aiCppFile = Data.LangEnabled[LanguageOption.cpp].Item2;
                        if (aiCppFile == null || !File.Exists(aiCppFile))
                        {
                            Data.LangEnabled[LanguageOption.cpp] = (false, string.Empty);
                            goto skip_cpp;
                        }
                        try
                        {
                            // 备份原始AI.cpp
                            string aiCppTemp = Path.ChangeExtension(aiCppFile, ".cpp.temp");
                            if (File.Exists(aiCppTemp))
                                File.Delete(aiCppTemp);
                            Log.LogInfo("正在备份旧的选手cpp代码……");
                            File.Move(aiCppFile, aiCppTemp);
                            // 保存旧模板到oldTemplate.cpp
                            string oldTemplatePath = Path.Combine(
                                Path.GetDirectoryName(aiCppFile), "oldTemplate.cpp"
                            );
                            string oldTemplate = $"./Templates/t.{CurrentVersion.TemplateVersion}.cpp";
                            string newTemplate = $"./Templates/t.{Data.FileHashData.TVersion.TemplateVersion}.cpp";
                            Log.LogInfo($"正在下载旧模板 {oldTemplate}");
                            CloudReport.Count++;
                            var toOld = Cloud.DownloadFileAsync(oldTemplatePath, oldTemplate).ContinueWith(_ => CloudReport.ComCount++);
                            toOld.Wait();
                            // 下载新的CPP模板
                            Log.LogInfo($"正在下载新模板 {newTemplate}");
                            CloudReport.Count++;
                            var toCpp = Cloud.DownloadFileAsync(aiCppFile, newTemplate).ContinueWith(_ => CloudReport.ComCount++);
                            toCpp.Wait();
                            // 如果下载成功，则进行选手代码和模板代码的合并
                            if (File.Exists(oldTemplatePath) && File.Exists(aiCppFile))
                            {
                                if (File.Exists(aiCppTemp))
                                {
                                    Log.LogInfo("正在合并选手cpp代码……");
                                    string temp = FileService.ReadToEnd(aiCppTemp);
                                    string oldTpl = FileService.ReadToEnd(oldTemplatePath);
                                    string newTpl = FileService.ReadToEnd(aiCppFile);
                                    string mergedTpl = FileService.MergeUserCode(temp, oldTpl, newTpl);
                                    File.WriteAllText(aiCppFile, mergedTpl);
                                    Log.LogInfo("选手cpp代码合并成功！");
                                }
                            }
                            else
                            {
                                Log.LogError("下载模板失败，尝试回滚……");
                                File.Move(aiCppTemp, aiCppFile);
                            }
                        }
                        catch (Exception exc)
                        {
                            Log.LogError($"cpp选手代码合并发生错误：{exc.Message}");
                        }
                    skip_cpp:;
                    }
                    // 对于Python API
                    {
                        var aiPyFile = Data.LangEnabled[LanguageOption.python].Item2;
                        if (aiPyFile == null || !File.Exists(aiPyFile))
                        {
                            Data.LangEnabled[LanguageOption.python] = (false, string.Empty);
                            goto skip_py;
                        }
                        try
                        {
                            // 备份原始AI.py
                            string aiPyTemp = Path.ChangeExtension(aiPyFile, ".py.temp");
                            if (File.Exists(aiPyTemp))
                                File.Delete(aiPyTemp);
                            Log.LogInfo("正在备份旧的选手py代码……");
                            File.Move(aiPyFile, aiPyTemp);
                            // 保存旧模板到oldTemplate.py
                            string oldTemplatePath = Path.Combine(
                                Path.GetDirectoryName(aiPyFile), "oldTemplate.py"
                            );
                            string oldTemplate = $"./Templates/t.{CurrentVersion.TemplateVersion}.py";
                            string newTemplate = $"./Templates/t.{Data.FileHashData.TVersion.TemplateVersion}.py";
                            Log.LogInfo($"正在下载旧模板 {oldTemplate}");
                            CloudReport.Count++;
                            var toOld = Cloud.DownloadFileAsync(oldTemplatePath, oldTemplate).ContinueWith(_ => CloudReport.ComCount++);
                            toOld.Wait();
                            // 下载新的Python模板
                            Log.LogInfo($"正在下载新模板 {newTemplate}");
                            CloudReport.Count++;
                            var toPy = Cloud.DownloadFileAsync(aiPyFile, newTemplate).ContinueWith(_ => CloudReport.ComCount++);
                            toPy.Wait();
                            // 如果下载成功，则进行选手代码和模板代码的合并
                            if (File.Exists(oldTemplatePath) && File.Exists(aiPyFile))
                            {
                                if (File.Exists(aiPyTemp))
                                {
                                    Log.LogInfo("正在合并选手py代码……");
                                    string temp = FileService.ReadToEnd(aiPyTemp);
                                    string oldTpl = FileService.ReadToEnd(oldTemplatePath);
                                    string newTpl = FileService.ReadToEnd(aiPyFile);
                                    string mergedTpl = FileService.MergeUserCode(temp, oldTpl, newTpl);
                                    File.WriteAllText(aiPyFile, mergedTpl);
                                    Log.LogInfo("选手py代码合并成功！");
                                }
                            }
                            else
                            {
                                Log.LogError("下载模板失败，尝试回滚……");
                                File.Move(aiPyTemp, aiPyFile);
                            }
                        }
                        catch (Exception exc)
                        {
                            Log.LogError($"python选手代码合并发生错误：{exc.Message}");
                        }
                    skip_py:;
                    }
                }
                // 如果是更新libVersion，需要使用COS下载器下载
                if (CurrentVersion < Data.FileHashData.TVersion &&
                    CurrentVersion.LibVersion < Data.FileHashData.TVersion.LibVersion)
                {
                    result = 1;
                    Log.LogInfo("待更新：新的代码库");
                    // 按照需要自动删除和修改的文件列表进行处理
                    foreach (var item in Data.MD5Update)
                    {
                        switch (item.state)
                        {
                            case DataRowState.Deleted:
                                { // 已经不存在的文件，进行删除
                                    Log.LogInfo($"删除 {item.name}");
                                    var file = item.name;
                                    if (file.StartsWith('.'))
                                    {
                                        file = Path.Combine(Data.Config.InstallPath, file);
                                    }
                                    try
                                    {
                                        if (File.Exists(file)) File.Delete(file);
                                    }
                                    catch (IOException ioe)
                                    {
                                        Log.LogWarning($"文件删除失败 {ioe.Message}");
                                    }
                                    break;
                                }
                            case DataRowState.Modified:
                                { // 修改后的文件，用服务器的覆盖
                                    Log.LogInfo($"修改 {item.name}");
                                    var fileLocal = item.name;
                                    if (fileLocal.StartsWith('.'))
                                    {
                                        fileLocal = Path.Combine(Data.Config.InstallPath, fileLocal);
                                    }
                                    var fileRemote = item.name.Replace(Path.DirectorySeparatorChar, '/');
                                    if (fileRemote.StartsWith("./"))
                                    {
                                        fileRemote = fileRemote[2..];
                                    }
                                    if (File.Exists(fileLocal))
                                    {
                                        try
                                        {
                                            File.Delete(fileLocal);
                                        }
                                        catch (IOException)
                                        {
                                            Log.LogWarning($"文件 {fileLocal} 被占用，跳过。");
                                            continue;
                                        }
                                    }
                                    // 确保目录存在
                                    string dir = Path.GetDirectoryName(fileLocal);
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }
                                    CloudReport.Count++;
                                    Cloud.DownloadFileAsync(fileLocal, fileRemote).ContinueWith(_ => CloudReport.ComCount++);
                                    break;
                                }
                            case DataRowState.Added:
                                { // 新增的文件，下载
                                    Log.LogInfo($"新增 {item.name}");
                                    var fileLocal = item.name;
                                    if (fileLocal.StartsWith('.'))
                                    {
                                        fileLocal = Path.Combine(Data.Config.InstallPath, fileLocal);
                                    }
                                    var fileRemote = item.name.Replace(Path.DirectorySeparatorChar, '/');
                                    if (fileRemote.StartsWith("./"))
                                    {
                                        fileRemote = fileRemote[2..];
                                    }
                                    // 确保目录存在
                                    string dir = Path.GetDirectoryName(fileLocal);
                                    if (!Directory.Exists(dir))
                                    {
                                        Directory.CreateDirectory(dir);
                                    }
                                    CloudReport.Count++;
                                    Cloud.DownloadFileAsync(fileLocal, fileRemote).ContinueWith(_ => CloudReport.ComCount++);
                                    break;
                                }
                        }
                    }
                    CurrentVersion = Data.FileHashData.TVersion;
                    Data.MD5Update.Clear();
                    Data.SaveMD5Data();
                }
            }
            return result;
        }

        /// <summary>
        /// 登录到EEsast
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public void Login(string username = "", string password = "")
        {
            Username = string.IsNullOrEmpty(username) ? Username : username;
            Password = string.IsNullOrEmpty(password) ? Password : password;
            Web.LoginToEEsast(Client, Username, Password).Wait();
        }

        /// <summary>
        /// 存储EEsast身份标识（修改Token时自动触发）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SaveToken(object? sender, EventArgs args)
        {
            Data.Config.Token = Web.Token;
        }

        public void RememberUser()
        {
            Data.Config.UserName = Username;
            Data.Config.Password = Password;
            Data.Config.Remembered = true;
            Web.Log.LogInfo("用户已记住。");
        }

        public void ForgetUser()
        {
            Data.Config.UserName = string.Empty;
            Data.Config.Password = string.Empty;
            Data.Config.Remembered = false;
            Web.Log.LogInfo("用户已忘记。");
        }

        /// <summary>
        /// 上传选手代码
        /// </summary>
        /// <param name="player_id">对应玩家id</param>
        public void UploadCode(int player_id)
        {
            string lang;
            switch (Commands.Language)
            {
                case LanguageOption.cpp:
                    lang = "cpp";
                    break;
                case LanguageOption.python:
                    lang = "python";
                    break;
                default:
                    lang = "unknown";
                    break;
            }
            Web.UploadFilesAsync(Client, Data.LangEnabled[Commands.Language].Item2, lang, $"player_{player_id}").Wait();
        }
        #endregion

        #region 异步类包装区
        public Task InstallAsync(string? path = null)
        {
            return Task.Run(() => Install(path));
        }

        public Task ResetInstallPathAsync(string newPath)
        {
            return Task.Run(() => ResetInstallPath(newPath));
        }

        public Task<bool> CheckUpdateAsync()
        {
            return Task.Run(() => CheckUpdate());
        }

        public Task<int> UpdateAsync()
        {
            return Task.Run(() => Update());
        }

        public Task LoginAsync(string username = "", string password = "")
        {
            return Task.Run(() => Login(username, password));
        }

        public Task UploadCodeAsync(int player_id)
        {
            return Task.Run(() => UploadCode(player_id));
        }
        #endregion
    }
}