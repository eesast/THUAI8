﻿using COSXML.CosException;
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
        public Tencent_Cos Cloud;                           // 这里是否需要新建一个cos桶？
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
        /// 判断文件是否为用户文件（不应被删除）
        /// </summary>
        private bool IsUserFile(string filePath)
        {
            // 实现判断用户文件的逻辑，比如通过文件名后缀、路径等
            // 以下是示例
            return filePath.Contains("AI.cpp") ||
                   filePath.Contains("AI.py") ||
                   filePath.EndsWith("user.config") ||
                   Path.GetFileName(filePath) == "源代码链接.txt";
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
                try
                {
                    // 确保目标日志目录存在
                    string newLogsDir = Path.Combine(newPath, "Logs");
                    if (!Directory.Exists(newLogsDir))
                    {
                        Directory.CreateDirectory(newLogsDir);
                    }

                    if (Directory.Exists(Path.Combine(installPath, "Logs")))
                    {
                        foreach (var f1 in Directory.EnumerateFiles(Path.Combine(installPath, "Logs")))
                        {
                            var m = FileService.ConvertAbsToRel(installPath, f1);
                            var n = Path.Combine(newPath, m);

                            // 确保目标目录存在
                            Directory.CreateDirectory(Path.GetDirectoryName(n));

                            File.Move(f1, n);
                        }
                    }

                    // 更新日志文件路径
                    if (Cloud.Log is FileLogger)
                    {
                        string cosLogPath = Path.Combine(newPath, "Logs", "TencentCos.log");
                        ((FileLogger)Cloud.Log).Path = cosLogPath;
                        Directory.CreateDirectory(Path.GetDirectoryName(cosLogPath));
                    }

                    if (Web.Log is FileLogger)
                    {
                        string webLogPath = Path.Combine(newPath, "Logs", "EESAST.log");
                        ((FileLogger)Web.Log).Path = webLogPath;
                        Directory.CreateDirectory(Path.GetDirectoryName(webLogPath));
                    }

                    if (Data.Log is FileLogger)
                    {
                        string dataLogPath = Path.Combine(newPath, "Logs", "Local_Data.log");
                        ((FileLogger)Data.Log).Path = dataLogPath;
                        Directory.CreateDirectory(Path.GetDirectoryName(dataLogPath));
                    }

                    if (Log is FileLogger)
                    {
                        string installerLogPath = Path.Combine(newPath, "Logs", "Installer.log");
                        ((FileLogger)Log).Path = installerLogPath;
                        Directory.CreateDirectory(Path.GetDirectoryName(installerLogPath));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"移动日志文件时出错: {ex.Message}");
                }

                Data.ResetInstallPath(newPath);
            }
            Update();
        }

        /// <summary>
        /// 全新安装THUAI8
        /// </summary>
        public void Install(string? path = null)
        {
            if (path != null)
            {
                Data.Config.InstallPath = path;
            }

            // 清理旧文件（如果存在）
            Cloud.Log.LogWarning($"全新安装开始，所有位于{Data.Config.InstallPath}的文件都将被删除。");

            // 仅在目录存在时执行删除操作
            if (Directory.Exists(Data.Config.InstallPath))
            {
                try
                {
                    // 使用递归删除非用户文件
                    Action<DirectoryInfo> deleteTask = null;
                    deleteTask = (dir) =>
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    // 只删除非用户文件，保留用户文件
                    if (!IsUserFile(file.FullName))
                        file.Delete();
                }
                foreach (var sub in dir.EnumerateDirectories())
                {
                    deleteTask(sub);
                }
            };

                    deleteTask(new DirectoryInfo(Data.Config.InstallPath));
                }
                catch (Exception ex)
                {
                    Cloud.Log.LogError($"删除旧文件时出错: {ex.Message}");
                }
            }
            else
            {
                Directory.CreateDirectory(Data.Config.InstallPath);
            }

            // 创建日志目录
            string logsDir = Path.Combine(Data.Config.InstallPath, "Logs");
            if (!Directory.Exists(logsDir))
            {
                try
                {
                    Directory.CreateDirectory(logsDir);
                    Cloud.Log.LogInfo($"创建日志目录: {logsDir}");
                }
                catch (Exception ex)
                {
                    Cloud.Log.LogError($"创建日志目录时出错: {ex.Message}");
                }
            }

            try
            {
                // 更新日志文件路径
                if (Cloud.Log is FileLogger)
                {
                    string cosLogPath = Path.Combine(logsDir, "TencentCos.log");
                    ((FileLogger)Cloud.Log).Path = cosLogPath;
                    // 确保日志文件所在目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(cosLogPath));
                }

                if (Data.Log is FileLogger)
                {
                    string dataLogPath = Path.Combine(logsDir, "Local_Data.log");
                    ((FileLogger)Data.Log).Path = dataLogPath;
                    // 确保日志文件所在目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(dataLogPath));
                }

                if (Log is FileLogger)
                {
                    string installerLogPath = Path.Combine(logsDir, "Installer.log");
                    ((FileLogger)Log).Path = installerLogPath;
                    // 确保日志文件所在目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(installerLogPath));
                }
            }
            catch (Exception ex)
            {
                // 使用控制台记录错误，因为日志可能无法工作
                Console.WriteLine($"设置日志路径时出错: {ex.Message}");
            }

            // 创建所需的目录结构
            string[] directories = new string[]
            {
                Path.Combine(Data.Config.InstallPath, "CAPI"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "include"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "src"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "proto"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "python"),
                Path.Combine(Data.Config.InstallPath, "CAPI", "python", "PyAPI"),
                Path.Combine(Data.Config.InstallPath, "dependency"),
                Path.Combine(Data.Config.InstallPath, "dependency", "proto"),
                Path.Combine(Data.Config.InstallPath, "dependency", "shell"),
                Path.Combine(Data.Config.InstallPath, "interface"),
                Path.Combine(Data.Config.InstallPath, "launcher"),
                Path.Combine(Data.Config.InstallPath, "logic"),
                Path.Combine(Data.Config.InstallPath, "logic", "Server"),
                Path.Combine(Data.Config.InstallPath, "logic", "Client"),
                Path.Combine(Data.Config.InstallPath, "playback")
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Cloud.Log.LogInfo($"创建目录: {dir}");
                }
            }

            Data.ResetInstallPath(Data.Config.InstallPath);

            // 下载和解压主安装包
            bool mainPackageDownloaded = false;
            string mainPackagePath = Path.Combine(Data.Config.InstallPath, "THUAI8.tar.gz");

            Cloud.Log.LogInfo("正在下载THUAI8安装包...");
            try
            {
                var result = Cloud.DownloadFileAsync(mainPackagePath, "THUAI8.tar.gz").Result;
                if (result >= 0 && File.Exists(mainPackagePath))
                {
                    Cloud.Log.LogInfo("THUAI8安装包下载完成，开始解压...");
                    Cloud.ArchieveUnzip(mainPackagePath, Data.Config.InstallPath);
                    Cloud.Log.LogInfo("THUAI8安装包解压完成");
                    mainPackageDownloaded = true;

                    // 删除压缩包以节省空间
                    File.Delete(mainPackagePath);
                }
                else
                {
                    Cloud.Log.LogError("THUAI8安装包下载失败");
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"下载或解压THUAI8安装包时出错: {ex.Message}");
            }

            // 设置当前版本
            CurrentVersion = Data.FileHashData.TVersion;

            // 下载选手代码
            Cloud.Log.LogInfo("正在下载选手代码...");
            bool cppCodeDownloaded = false;
            bool pythonCodeDownloaded = false;

            string cppCodePath = Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "API", "src", "AI.cpp");
            string pythonCodePath = Path.Combine(Data.Config.InstallPath, "CAPI", "python", "PyAPI", "AI.py");

            try
            {
                var cppResult = Cloud.DownloadFileAsync(cppCodePath, $"./Templates/t.{CurrentVersion.TemplateVersion}.cpp").Result;
                if (cppResult >= 0 && File.Exists(cppCodePath))
                {
                    Cloud.Log.LogInfo("C++选手代码下载成功");
                    cppCodeDownloaded = true;
                }
                else
                {
                    Cloud.Log.LogWarning("C++选手代码下载失败");
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"下载C++选手代码时出错: {ex.Message}");
            }

            try
            {
                var pyResult = Cloud.DownloadFileAsync(pythonCodePath, $"./Templates/t.{CurrentVersion.TemplateVersion}.py").Result;
                if (pyResult >= 0 && File.Exists(pythonCodePath))
                {
                    Cloud.Log.LogInfo("Python选手代码下载成功");
                    pythonCodeDownloaded = true;
                }
                else
                {
                    Cloud.Log.LogWarning("Python选手代码下载失败");
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"下载Python选手代码时出错: {ex.Message}");
            }

            // 下载和解压proto库
            bool protoLibDownloaded = false;
            string protoLibArchivePath = Path.Combine(Data.Config.InstallPath, "protoCpp.tar.gz");
            string protoLibDestPath = Path.Combine(Data.Config.InstallPath, "CAPI", "cpp", "lib");

            Cloud.Log.LogInfo("正在下载proto cpp库...");
            try
            {
                var result = Cloud.DownloadFileAsync(protoLibArchivePath, "Setup/proto/protoCpp.tar.gz").Result;
                if (result >= 0 && File.Exists(protoLibArchivePath))
                {
                    Cloud.Log.LogInfo("proto cpp库下载完成，开始解压...");

                    // 确保目标目录存在
                    if (!Directory.Exists(protoLibDestPath))
                    {
                        Directory.CreateDirectory(protoLibDestPath);
                    }

                    Cloud.ArchieveUnzip(protoLibArchivePath, protoLibDestPath);
                    Cloud.Log.LogInfo("proto cpp库解压完成");
                    protoLibDownloaded = true;

                    // 删除压缩包以节省空间
                    File.Delete(protoLibArchivePath);
                }
                else
                {
                    Cloud.Log.LogError("proto cpp库下载失败");
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"下载或解压proto cpp库时出错: {ex.Message}");
            }

            // 下载服务器和客户端可执行文件
            string serverExePath = Path.Combine(Data.Config.InstallPath, "logic", "Server", "Server.exe");
            string clientExePath = Path.Combine(Data.Config.InstallPath, "logic", "Client", "Client.exe");

            bool serverDownloaded = false;
            bool clientDownloaded = false;

            // 尝试从多个可能的路径下载服务器可执行文件
            string[] possibleServerPaths = new string[] {
                "Server/Server.exe",
                "server/Server.exe",
                "logic/Server/Server.exe",
                "GameServer/GameServer.exe",
                "Server.exe"
            };

            // 尝试下载服务器
            foreach (var cloudPath in possibleServerPaths)
            {
                Cloud.Log.LogInfo($"尝试从 {cloudPath} 下载服务器可执行文件...");

                try
                {
                    var result = Cloud.DownloadFileAsync(serverExePath, cloudPath).Result;
                    if (result >= 0 && File.Exists(serverExePath))
                    {
                        Cloud.Log.LogInfo($"服务器可执行文件成功从 {cloudPath} 下载");
                        serverDownloaded = true;
                        break;
                    }
                    Cloud.Log.LogWarning($"从 {cloudPath} 下载失败");
                }
                catch (Exception ex)
                {
                    Cloud.Log.LogError($"从 {cloudPath} 下载服务器时出错: {ex.Message}");
                }
            }

            // 尝试从多个可能的路径下载客户端可执行文件
            string[] possibleClientPaths = new string[] {
                "Client/Client.exe",
                "client/Client.exe",
                "logic/Client/Client.exe",
                "GameClient/GameClient.exe",
                "Client.exe"
            };

            // 尝试下载客户端
            foreach (var cloudPath in possibleClientPaths)
            {
                Cloud.Log.LogInfo($"尝试从 {cloudPath} 下载客户端可执行文件...");

                try
                {
                    var result = Cloud.DownloadFileAsync(clientExePath, cloudPath).Result;
                    if (result >= 0 && File.Exists(clientExePath))
                    {
                        Cloud.Log.LogInfo($"客户端可执行文件成功从 {cloudPath} 下载");
                        clientDownloaded = true;
                        break;
                    }
                    Cloud.Log.LogWarning($"从 {cloudPath} 下载失败");
                }
                catch (Exception ex)
                {
                    Cloud.Log.LogError($"从 {cloudPath} 下载客户端时出错: {ex.Message}");
                }
            }

            // 创建链接到GitHub源代码的提示文件
            try
            {
                string sourceLinkPath = Path.Combine(Data.Config.InstallPath, "源代码链接.txt");
                string sourceContent = "THUAI8源代码：https://github.com/eesast/THUAI8\r\n\r\n" +
                    "在GitHub上查看源代码，可以自行编译服务器和客户端可执行文件。\r\n\r\n" +
                    "服务器：https://github.com/eesast/THUAI8/tree/main/logic/Server\r\n" +
                    "客户端：https://github.com/eesast/THUAI8/tree/main/logic/Client\r\n\r\n" +
                    "如果自动下载的可执行文件无法运行，请按照GitHub上的说明自行编译。";

                File.WriteAllText(sourceLinkPath, sourceContent);
                Cloud.Log.LogInfo($"已创建源代码链接文件: {sourceLinkPath}");
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"创建源代码链接文件失败: {ex.Message}");
            }

            // 为Python选手创建generate_proto脚本
            try
            {
                string pythonGenProtoWindows = Path.Combine(Data.Config.InstallPath, "CAPI", "python", "generate_proto.cmd");
                string pythonGenProtoUnix = Path.Combine(Data.Config.InstallPath, "CAPI", "python", "generate_proto.sh");

                string windowsContent =
                    "python -m pip install -r requirements.txt\r\n" +
                    "python -m grpc_tools.protoc -I../../dependency/proto --python_out=. --grpc_python_out=. ../../dependency/proto/Services.proto ../../dependency/proto/Message2Server.proto ../../dependency/proto/Message2Clients.proto ../../dependency/proto/MessageType.proto\r\n";

                string unixContent =
                    "#!/bin/bash\n" +
                    "python3 -m pip install -r requirements.txt\n" +
                    "python3 -m grpc_tools.protoc -I../../dependency/proto --python_out=. --grpc_python_out=. ../../dependency/proto/Services.proto ../../dependency/proto/Message2Server.proto ../../dependency/proto/Message2Clients.proto ../../dependency/proto/MessageType.proto\n";

                File.WriteAllText(pythonGenProtoWindows, windowsContent);
                File.WriteAllText(pythonGenProtoUnix, unixContent);

                Cloud.Log.LogInfo("已创建Python proto生成脚本");
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"创建Python proto生成脚本失败: {ex.Message}");
            }

            // 创建Python要求文件
            try
            {
                string pythonRequirements = Path.Combine(Data.Config.InstallPath, "CAPI", "python", "requirements.txt");
                string requirementsContent =
                    "grpcio==1.53.0\n" +
                    "grpcio-tools==1.53.0\n" +
                    "protobuf==4.22.3\n";

                File.WriteAllText(pythonRequirements, requirementsContent);
                Cloud.Log.LogInfo("已创建Python requirements文件");
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"创建Python requirements文件失败: {ex.Message}");
            }

            // 创建批处理文件来辅助编译服务器和客户端
            try
            {
                string buildServerBatchPath = Path.Combine(Data.Config.InstallPath, "编译服务器.bat");
                string buildClientBatchPath = Path.Combine(Data.Config.InstallPath, "编译客户端.bat");

                // 服务器编译批处理内容
                string buildServerContent =
                    "@echo off\r\n" +
                    "echo 正在编译THUAI8服务器...\r\n" +
                    $"cd /d \"{Data.Config.InstallPath}\"\r\n" +
                    "if not exist logic\\Server mkdir logic\\Server\r\n" +
                    "cd logic\\Server\r\n" +
                    "if exist build rmdir /s /q build\r\n" +
                    "mkdir build\r\n" +
                    "cd build\r\n" +
                    "cmake ..\r\n" +
                    "cmake --build . --config Release\r\n" +
                    "echo 服务器编译完成，请检查build/Release目录下是否有Server.exe\r\n" +
                    "if exist Release\\Server.exe (\r\n" +
                    "    copy /Y Release\\Server.exe ..\\\r\n" +
                    "    echo 服务器可执行文件已复制到logic\\Server目录\r\n" +
                    ") else (\r\n" +
                    "    echo 服务器编译似乎失败，请查看错误信息\r\n" +
                    ")\r\n" +
                    "pause\r\n";

                // 客户端编译批处理内容
                string buildClientContent =
                    "@echo off\r\n" +
                    "echo 正在编译THUAI8客户端...\r\n" +
                    $"cd /d \"{Data.Config.InstallPath}\"\r\n" +
                    "if not exist logic\\Client mkdir logic\\Client\r\n" +
                    "cd logic\\Client\r\n" +
                    "if exist build rmdir /s /q build\r\n" +
                    "mkdir build\r\n" +
                    "cd build\r\n" +
                    "cmake ..\r\n" +
                    "cmake --build . --config Release\r\n" +
                    "echo 客户端编译完成，请检查build/Release目录下是否有Client.exe\r\n" +
                    "if exist Release\\Client.exe (\r\n" +
                    "    copy /Y Release\\Client.exe ..\\\r\n" +
                    "    echo 客户端可执行文件已复制到logic\\Client目录\r\n" +
                    ") else (\r\n" +
                    "    echo 客户端编译似乎失败，请查看错误信息\r\n" +
                    ")\r\n" +
                    "pause\r\n";

                File.WriteAllText(buildServerBatchPath, buildServerContent);
                File.WriteAllText(buildClientBatchPath, buildClientContent);

                Cloud.Log.LogInfo($"已创建编译服务器和客户端的批处理文件");
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"创建编译批处理文件失败: {ex.Message}");
            }

            // 尝试直接从installer应用目录复制可执行文件
            try
            {
                string appServerExe = Path.Combine(AppContext.BaseDirectory, "Server.exe");
                string appClientExe = Path.Combine(AppContext.BaseDirectory, "Client.exe");

                if (File.Exists(appServerExe) && !serverDownloaded)
                {
                    File.Copy(appServerExe, serverExePath, true);
                    Cloud.Log.LogInfo($"已从应用目录复制服务器可执行文件");
                    serverDownloaded = true;
                }

                if (File.Exists(appClientExe) && !clientDownloaded)
                {
                    File.Copy(appClientExe, clientExePath, true);
                    Cloud.Log.LogInfo($"已从应用目录复制客户端可执行文件");
                    clientDownloaded = true;
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"从应用目录复制可执行文件失败: {ex.Message}");
            }

            // 校验安装结果
            if (cppCodeDownloaded && pythonCodeDownloaded)
            {
                Cloud.Log.LogInfo("选手代码下载成功！");
            }
            else
            {
                Cloud.Log.LogError("选手代码下载不完整，选手可自行下载，网址：https://github.com/eesast/THUAI8/tree/main/CAPI/cpp/API/src/AI.cpp，https://github.com/eesast/THUAI8/tree/main/CAPI/python/PyAPI/AI.py");
            }

            if (!serverDownloaded)
            {
                Cloud.Log.LogWarning("无法下载服务器可执行文件，请使用编译服务器批处理文件或从GitHub下载");
            }

            if (!clientDownloaded)
            {
                Cloud.Log.LogWarning("无法下载客户端可执行文件，请使用编译客户端批处理文件或从GitHub下载");
            }

            // 检查安装结果
            try
            {
                Data.MD5Update.Clear();
                Data.ScanDir();

                if (Data.MD5Update.Count != 0)
                {
                    Cloud.Log.LogWarning($"校验有{Data.MD5Update.Count}个文件不匹配，将尝试补全...");
                    Update();
                }
                else
                {
                    Cloud.Log.LogInfo($"安装成功！开始您的THUAI8探索之旅吧！");
                    Data.Installed = true;

                    // 打开安装目录
                    try
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            Arguments = Data.Config.InstallPath,
                            FileName = "explorer.exe",
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Cloud.Log.LogError($"打开安装目录失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Cloud.Log.LogError($"校验安装结果时出错: {ex.Message}");
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
                }
                // 启动器本身需要更新，返回结果为16
                if (CurrentVersion.InstallerVersion < Data.FileHashData.TVersion.InstallerVersion)
                {
                    var local = Path.Combine(AppContext.BaseDirectory, "Cache", $"Installer_v{Data.FileHashData.TVersion.InstallerVersion}.zip");
                    Cloud.Log.LogWarning("启动器即将升级，正在下载压缩包……");
                    Status = UpdateStatus.downloading;
                    Log.CountDict[LogLevel.Error] = 0;
                    CloudReport.Count++;
                    var i = Cloud.DownloadFileAsync(local, $"Setup/Installer_v{Data.FileHashData.TVersion.InstallerVersion}.zip").Result;
                    if (i >= 0)
                    {
                        CloudReport.ComCount++;
                        Cloud.Log.LogWarning("下载完成，请退出下载器，并将压缩包解压到原下载器安装位置。");
                        Status = UpdateStatus.exiting;
                        if (DeviceInfo.Platform == DevicePlatform.WinUI)
                        {
                            Process.Start(new ProcessStartInfo()
                            {
                                Arguments = '\"' + local + '\"',
                                FileName = "explorer.exe"
                            });
                        }
                        CurrentVersion = Data.FileHashData.TVersion;
                        Data.SaveMD5Data();
                        return 16;
                    }
                    else
                    {
                        // 下载失败
                        Cloud.Log.LogError("启动器下载失败。");
                        Data.SaveMD5Data();
                        return -1;
                    }
                }
                // AI.cpp/AI.py有改动
                // 返回结果为Flags，1: AI.cpp升级；2: AI.py升级
                if (CurrentVersion.TemplateVersion < Data.FileHashData.TVersion.TemplateVersion)
                {
                    Cloud.Log.LogWarning("检测到选手代码升级，即将下载选手代码模板……");
                    Status = UpdateStatus.downloading;
                    var p = Path.Combine(Data.Config.InstallPath, "Templates");
                    if (!Directory.Exists(p))
                    {
                        Directory.CreateDirectory(p);
                    }
                    Log.CountDict[LogLevel.Error] = 0;
                    CloudReport.Count += 4;

                    // 下载路径
                    var tpocpp = Path.Combine(Directory.GetParent(Data.LangEnabled[LanguageOption.cpp].Item2)?.FullName ?? Data.Config.InstallPath, $"oldTemplate.cpp");
                    var tpncpp = Path.Combine(Directory.GetParent(Data.LangEnabled[LanguageOption.cpp].Item2)?.FullName ?? Data.Config.InstallPath, $"newTemplate.cpp");
                    var tpopy = Path.Combine(Directory.GetParent(Data.LangEnabled[LanguageOption.python].Item2)?.FullName ?? Data.Config.InstallPath, $"oldTemplate.py");
                    var tpnpy = Path.Combine(Directory.GetParent(Data.LangEnabled[LanguageOption.python].Item2)?.FullName ?? Data.Config.InstallPath, $"newTemplate.py");
                    // 下载任务
                    var tocpp = Cloud.DownloadFileAsync(tpocpp, $"./Templates/t.{CurrentVersion.TemplateVersion}.cpp");
                    var topy = Cloud.DownloadFileAsync(tpopy, $"./Templates/t.{CurrentVersion.TemplateVersion}.py");
                    var tncpp = Cloud.DownloadFileAsync(tpncpp, $"./Templates/t.{Data.FileHashData.TVersion.TemplateVersion}.cpp");
                    var tnpy = Cloud.DownloadFileAsync(tpnpy, $"./Templates/t.{Data.FileHashData.TVersion.TemplateVersion}.py");
                    Task.WaitAll(tocpp, topy, tncpp, tnpy);
                    var r = (tocpp.Result >= 0 ? 1 : 0) + (topy.Result >= 0 ? 1 : 0) + (tncpp.Result >= 0 ? 1 : 0) + (tnpy.Result >= 0 ? 1 : 0);
                    CloudReport.ComCount += r;
                    if (r == 4)
                    {
                        Cloud.Log.LogWarning("下载完毕，即将合并模板与用户代码，结果可能出现问题，请务必核实新旧模板代码和选手代码，确认正确后用同名temp文件覆盖源文件");
                        if (Data.LangEnabled[LanguageOption.cpp].Item1)
                        {
                            var so = FileService.ReadToEnd(tpocpp);
                            var sn = FileService.ReadToEnd(tpncpp);
                            var sa = FileService.ReadToEnd(Data.LangEnabled[LanguageOption.cpp].Item2);
                            var s = FileService.MergeUserCode(sa, so, sn);
                            using (var f = new FileStream(Data.LangEnabled[LanguageOption.cpp].Item2 + ".temp", FileMode.Create))
                            using (var w = new StreamWriter(f))
                            {
                                w.Write(s);
                                w.Flush();
                            }
                            result |= 1;
                            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                            {
                                Process.Start(new ProcessStartInfo()
                                {
                                    Arguments = Directory.GetParent(Data.LangEnabled[LanguageOption.cpp].Item2)?.FullName,
                                    FileName = "explorer.exe"
                                });
                            }
                        }
                        if (Data.LangEnabled[LanguageOption.python].Item1)
                        {
                            var so = FileService.ReadToEnd(tpopy);
                            var sn = FileService.ReadToEnd(tpnpy);
                            var sa = FileService.ReadToEnd(Data.LangEnabled[LanguageOption.python].Item2);
                            var s = FileService.MergeUserCode(sa, so, sn);
                            using (var f = new FileStream(Data.LangEnabled[LanguageOption.python].Item2 + ".temp", FileMode.Create))
                            using (var w = new StreamWriter(f))
                            {
                                w.Write(s);
                                w.Flush();
                            }
                            result |= 2;
                            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                            {
                                Process.Start(new ProcessStartInfo()
                                {
                                    Arguments = Directory.GetParent(Data.LangEnabled[LanguageOption.python].Item2)?.FullName,
                                    FileName = "explorer.exe"
                                });
                            }
                        }
                    }
                }
                Log.CountDict[LogLevel.Error] = 0;

                // 更新成功后返回值Flags增加0x8
                Status = UpdateStatus.downloading;
                Cloud.Log.LogInfo("正在更新……");
                Cloud.DownloadQueueAsync(Data.Config.InstallPath,
                    from item in Data.MD5Update where item.state != System.Data.DataRowState.Added select item.name).Wait();
                Cloud.Log.LogWarning("正在删除冗余文件……");
                foreach (var item in Data.MD5Update.Where((s) => s.state == System.Data.DataRowState.Added))
                {
                    var _file = item.name;
                    var file = _file.StartsWith('.') ?
                        Path.Combine(Data.Config.InstallPath, _file) : _file;
                    File.Delete(file);
                }
                if (Log.CountDict[LogLevel.Error] == 0)
                {
                    Data.MD5Update.Clear();
                    var c = CurrentVersion;
                    CurrentVersion = Data.FileHashData.TVersion;
                    Status = UpdateStatus.hash_computing;
                    Data.Log.LogInfo("正在校验……");
                    if (!CheckUpdate())
                    {
                        Data.Log.LogInfo("更新成功！");
                        Status = UpdateStatus.success;
                        Data.Installed = true;
                        result |= 8;
                        Data.SaveMD5Data();
                        return result;
                    }
                    else
                        CurrentVersion = c;
                }
            }
            else
            {
                Cloud.Log.LogInfo("已经是最新版本啦！");
                Data.Installed = true;
                Status = UpdateStatus.success;
                Data.SaveMD5Data();
                return 0;
            }
            Cloud.Log.LogError("更新出问题了 -_-b");
            Status = UpdateStatus.error;
            Data.FileHashData.TVersion = CurrentVersion;
            Data.SaveMD5Data();
            return -1;
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