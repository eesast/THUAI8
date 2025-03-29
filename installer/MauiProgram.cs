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
        public static string SecretID = "***";
        public static string SecretKey = "***";
        
        public static MauiApp CreateMauiApp()
        {
            try
            {
                // 首先初始化调试工具
                DebugTool.Initialize();
                DebugTool.Log("开始创建MAUI应用程序");
                
                // 捕获UI线程同步上下文
                UISynchronizationContext = SynchronizationContext.Current;
                DebugTool.Log("UI同步上下文已捕获");
                
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
                    else
                    {
                        DebugTool.Log($"Secret文件不存在: {filePath}");
                        // 创建一个简单的Secret.csv用于测试
                        try
                        {
                            DebugTool.Log("创建默认Secret.csv文件");
                            using (StreamWriter sw = new StreamWriter(filePath))
                            {
                                // 使用固定密钥，仅用于测试
                                sw.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="); // Key
                                sw.WriteLine("AAAAAAAAAAAAAAAAAAAAAA=="); // IV
                                sw.WriteLine("AAAAAAAAAAAAAAAAAAAAAA=="); // Encrypted SecretID
                                sw.WriteLine("AAAAAAAAAAAAAAAAAAAAAA=="); // Encrypted SecretKey
                            }
                        }
                        catch (Exception ex)
                        {
                            DebugTool.LogException(ex, "创建默认Secret.csv");
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugTool.LogException(ex, "读取Secret文件");
                }

                DebugTool.Log("开始配置MAUI应用程序");
                var builder = MauiApp.CreateBuilder();
                builder
                    .UseMauiApp<App>()
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
                DebugTool.LogException(ex, "创建MAUI应用");
                throw; // 重新抛出异常以便能够看到崩溃信息
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