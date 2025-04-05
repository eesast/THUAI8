using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using installer.ViewModel;
using installer.Model;
using installer.Services;
using System.Text;
using System.Diagnostics;
using Microsoft.Maui.LifecycleEvents;
using System.Threading;

namespace installer
{
    public static class MauiProgram
    {
        // 添加UI线程同步对象
        public static SynchronizationContext? UISynchronizationContext { get; private set; }

        // public static Model.Logger logger = Model.LoggerProvider.FromFile(@"E:\bin\log\123.log");
        public static bool ErrorTrigger_WhileDebug = true;
        public static bool RefreshLogs_WhileDebug = false;
        public static string? SecretID = null;
        public static string? SecretKey = null;

        public static MauiApp CreateMauiApp()
        {
            try
            {
                // 确保初始化调试工具
                DebugTool.Initialize();
                DebugTool.Log("调试工具已初始化");
            }
            catch (Exception ex)
            {
                // 无法记录日志，但尝试继续
                Debug.WriteLine($"初始化调试工具失败: {ex.Message}");
            }

            var builder = MauiApp.CreateBuilder();
            try
            {
                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });

                // 初始化默认空密钥
                SecretID = "***";
                SecretKey = "***";

                try
                {
                    // 首先尝试从嵌入式资源中读取密钥
                    try
                    {
                        LoadSecretFromEmbeddedResource();
                    }
                    catch (Exception ex)
                    {
                        DebugTool.Log($"从嵌入式资源加载密钥失败: {ex.Message}");
                    }

                    // 如果嵌入式资源中没有密钥，则尝试从外部文件读取
                    if (string.IsNullOrEmpty(SecretID) || SecretID == "***" || string.IsNullOrEmpty(SecretKey) || SecretKey == "***")
                    {
                        try
                        {
                            LoadSecretFromExternalFile();
                        }
                        catch (Exception ex)
                        {
                            DebugTool.Log($"从外部文件加载密钥失败: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 确保任何密钥加载问题都不会导致应用崩溃
                    DebugTool.LogException(ex, "密钥加载过程中发生未处理的异常");
                }

                DebugTool.Log("开始配置MAUI应用程序");
                builder
                    .UseMauiCommunityToolkitCore();

                try
                {
                    // 尝试单独使用CommunityToolkit
                    builder.UseMauiCommunityToolkit();
                }
                catch (Exception ex)
                {
                    DebugTool.LogException(ex, "初始化CommunityToolkit");
                    // 继续执行，不要中断应用初始化
                }

                builder.ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                        fonts.AddFont("FontAwesome.ttf", "FontAwesome");
                    })
                    .ConfigureLifecycleEvents(events =>
                    {
#if WINDOWS
                        // 确保Windows应用正确初始化
                        events.AddWindows(windows => windows
                            .OnWindowCreated((window) => 
                            {
                                window.ExtendsContentIntoTitleBar = false;
                                DebugTool.Log("Windows窗口已创建");
                            })
                            .OnActivated((window, args) => 
                            {
                                DebugTool.Log("Windows窗口已激活");
                            })
                            .OnLaunched((window, args) => 
                            {
                                DebugTool.Log("Windows窗口已启动");
                            })
                            .OnClosed((window, args) =>
                            {
                                DebugTool.Log("Windows窗口已关闭");
                            })
                            .OnVisibilityChanged((window, args) =>
                            {
                                DebugTool.Log($"Windows窗口可见性改变: {args.Visible}");
                            })
                        );
#endif
                    });

                DebugTool.Log("正在注册服务");
                var c = builder.Services.AddSingleton<Downloader>().First();

                builder.Services.AddSingleton(FolderPicker.Default);
                builder.Services.AddSingleton(FilePicker.Default);

                // 注册特定业务服务
                DebugTool.Log("注册特定服务");

                // 如果需要在视图模型注入之前添加特定的服务依赖，可以在这里添加

                DebugTool.Log("开始注册视图模型和页面");
                AddViewModelService(builder);
                AddPageService(builder);
#if DEBUG
                builder.Logging.AddDebug();
#endif
                DebugTool.Log("开始构建MAUI应用程序");
                var app = builder.Build();
                DebugTool.Log("MAUI应用程序构建完成");
                return app;
            }
            catch (Exception ex)
            {
                DebugTool.LogException(ex, "MAUI应用程序初始化");
                throw; // 重新抛出异常以便能够看到崩溃信息
            }
        }

        private static void LoadSecretFromEmbeddedResource()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "installer.Resources.Raw.secured_key.csv";

                // 列出所有嵌入资源以进行调试
                var resources = assembly.GetManifestResourceNames();
                DebugTool.Log($"可用资源: {string.Join(", ", resources)}");

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = reader.ReadToEnd();
                            var lines = content.Split('\n').Select(s => s.Trim().Trim('\r')).ToArray();

                            if (lines.Length >= 4)
                            {
                                try
                                {
                                    using (Aes aes = Aes.Create())
                                    {
                                        aes.Key = Convert.FromBase64String(lines[0]);
                                        aes.IV = Convert.FromBase64String(lines[1]);
                                        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                                        try
                                        {
                                            // 解密SecretID
                                            using (MemoryStream memory = new MemoryStream(Convert.FromBase64String(lines[2])))
                                            using (CryptoStream crypto = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                                            using (StreamReader cryptoReader = new StreamReader(crypto))
                                            {
                                                SecretID = cryptoReader.ReadToEnd();
                                            }

                                            // 解密SecretKey
                                            using (MemoryStream memory = new MemoryStream(Convert.FromBase64String(lines[3])))
                                            using (CryptoStream crypto = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                                            using (StreamReader cryptoReader = new StreamReader(crypto))
                                            {
                                                SecretKey = cryptoReader.ReadToEnd();
                                            }

                                            DebugTool.Log("从嵌入式资源成功加载密钥");
                                        }
                                        catch (Exception ex)
                                        {
                                            DebugTool.LogException(ex, "解密嵌入式资源密钥内容");
                                            // 不抛出异常，使程序可以继续运行
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DebugTool.LogException(ex, "初始化嵌入式资源AES解密器");
                                    // 不抛出异常，使程序可以继续运行
                                }
                            }
                            else
                            {
                                DebugTool.Log($"嵌入式资源密钥文件格式不正确，行数: {lines.Length}");
                            }
                        }
                    }
                    else
                    {
                        DebugTool.Log("未找到嵌入式资源密钥文件");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugTool.LogException(ex, "读取嵌入式资源出错");
                // 不抛出异常，允许回退到其他加载方式
            }
        }

        private static void LoadSecretFromExternalFile()
        {
            // 保留原有的外部文件读取逻辑
            // read SecretID & SecretKey from filePath for debug
            var filePath = Debugger.IsAttached ? "D:\\Secret.csv" : Path.Combine(AppContext.BaseDirectory, "Secret.csv");
            try
            {
                if (File.Exists(filePath))
                {
                    DebugTool.Log($"正在读取Secret文件: {filePath}");
                    var lines = File.ReadAllLines(filePath);
                    if (lines.Length >= 4)
                    {
                        try
                        {
                            lines = lines.Select(s => s.Trim().Trim('\r', '\n')).ToArray();
                            using (Aes aes = Aes.Create())
                            {
                                try
                                {
                                    aes.Key = Convert.FromBase64String(lines[0]);
                                    aes.IV = Convert.FromBase64String(lines[1]);
                                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                                    try
                                    {
                                        using (MemoryStream memory = new MemoryStream(Convert.FromBase64String(lines[2])))
                                        {
                                            using (CryptoStream crypto = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                                            {
                                                using (StreamReader reader = new StreamReader(crypto, Encoding.ASCII))
                                                    SecretID = reader.ReadToEnd();
                                            }
                                        }

                                        using (MemoryStream memory = new MemoryStream(Convert.FromBase64String(lines[3])))
                                        {
                                            using (CryptoStream crypto = new CryptoStream(memory, decryptor, CryptoStreamMode.Read))
                                            {
                                                using (StreamReader reader = new StreamReader(crypto, Encoding.ASCII))
                                                    SecretKey = reader.ReadToEnd();
                                            }
                                        }
                                        DebugTool.Log("Secret文件解密完成");
                                    }
                                    catch (Exception ex)
                                    {
                                        DebugTool.LogException(ex, "解密Secret内容");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DebugTool.LogException(ex, "初始化AES解密器");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugTool.LogException(ex, "处理Secret文件行");
                        }
                    }
                    else
                    {
                        DebugTool.Log($"Secret文件格式不正确，行数: {lines.Length}");
                    }
                }
            }
            catch (Exception ex)
            {
                DebugTool.LogException(ex, "读取外部Secret文件");
            }
        }

        public static void AddViewModelService(MauiAppBuilder builder)
        {
            var a = typeof(MauiProgram).Assembly;
            foreach (var t in a.GetTypes())
            {
                if ((t.FullName ?? string.Empty).StartsWith($"{a.GetName().Name}.ViewModel") && !t.IsAbstract)
                {
                    builder.Services.AddSingleton(t);
                }
            }
        }
        public static void AddPageService(MauiAppBuilder builder)
        {
            var a = typeof(MauiProgram).Assembly;
            foreach (var t in a.GetTypes())
            {
                if ((t.FullName ?? string.Empty).StartsWith($"{a.GetName().Name}.Page") && !t.IsAbstract)
                {
                    builder.Services.AddSingleton(t);
                }
            }
        }
    }
}