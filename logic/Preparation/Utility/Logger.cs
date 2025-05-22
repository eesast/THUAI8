using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Preparation.Utility.Logging;

/// <summary>
/// 日志相关静态工具方法集合。
/// </summary>
public static class LogUtility
{
    public static string GetTypeName(object obj) => obj.GetType().Name;
    public static string GetTypeName(Type type) => type.Name;

    public static string GetObjectInfo(object obj, string message = "") =>
        string.IsNullOrEmpty(message)
        ? $"<{GetTypeName(obj)}>"
        : $"<{GetTypeName(obj)} {message}>";

    public static string GetObjectInfo(Type type, string message = "") =>
        string.IsNullOrEmpty(message)
        ? $"<{GetTypeName(type)}>"
        : $"<{GetTypeName(type)} {message}>";

    public static string GetCurrentTime() =>
        DateTime.Now.ToString("HH:mm:ss.fff");

    public static string GetCurrentDate() =>
        $"{DateTime.Today:yyyy/MM/dd} {DateTime.Today.DayOfWeek}";
}

/// <summary>
/// 高级日志工厂，支持多文件输出和自动携带调用者信息。
/// </summary>
public class AdvancedLoggerFactory
{
    private static ILoggerFactory? _loggerFactory;
    public static ILoggerFactory LoggerFactory
    {
        get
        {
            if (_loggerFactory is null)
                throw new InvalidOperationException("AdvancedLoggerFactory.LoggerFactory 尚未初始化！");
            return _loggerFactory;
        }
        private set => _loggerFactory = value;
    }

    /// <summary>
    /// 日志调用者信息载体
    /// </summary>
    public record CallerState(string Message, string Member, string File)
    {
        public string FileName => Path.GetFileNameWithoutExtension(File);
        public override string ToString() => Message;
    }

    /// <summary>
    /// 日志封装类，便于自动携带调用者信息
    /// </summary>
    public class Logger
    {
        private readonly ILogger _logger;
        public Logger(ILogger logger) => _logger = logger;

        public void LogError(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Log(LogLevel.Error, msg, file, member);

        public void LogWarning(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Log(LogLevel.Warning, msg, file, member);

        public void LogInfo(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Log(LogLevel.Information, msg, file, member);

        public void LogDebug(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Log(LogLevel.Debug, msg, file, member);

        public void LogTrace(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Log(LogLevel.Trace, msg, file, member);

        public void LogRaw(string msg, [CallerFilePath] string file = "", [CallerMemberName] string member = "")
            => Console.WriteLine(msg);

        private void Log(LogLevel level, string msg, string file, string member)
        {
            _logger.Log(level, new EventId(0), new CallerState(msg, member, file), null, (s, e) => s.ToString());
        }
    }

    /// <summary>
    /// 多文件日志提供器
    /// </summary>
    private class MultiFileLoggerProvider : ILoggerProvider
    {
        private readonly Dictionary<string, StreamWriter> _writers = new();
        private static readonly string AllLogPath = "logs/all.log";
        private static readonly StreamWriter AllLogWriter;

        static MultiFileLoggerProvider()
        {
            Directory.CreateDirectory("logs");
            AllLogWriter = new StreamWriter(AllLogPath, append: false) { AutoFlush = true };
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (!_writers.ContainsKey(categoryName))
            {
                var file = $"logs/{categoryName}.log";
                _writers[categoryName] = new StreamWriter(file, append: false) { AutoFlush = true };
            }
            return new MultiFileLogger(_writers[categoryName], categoryName);
        }

        public void Dispose()
        {
            foreach (var writer in _writers.Values)
                writer.Dispose();
            AllLogWriter.Dispose();
        }

        /// <summary>
        /// 多文件日志实现
        /// </summary>
        private class MultiFileLogger : ILogger
        {
            private readonly StreamWriter _writer;
            private readonly string _categoryName;

            public MultiFileLogger(StreamWriter writer, string categoryName)
            {
                _writer = writer;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId,
                TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                string file = "", member = "";
                if (state is CallerState callerState)
                {
                    file = callerState.FileName;
                    member = callerState.Member;
                }

                var logLine = $"[{DateTime.Now:HH:mm:ss.fff}]" +
                              $"[{_categoryName}]" +
                              $"[{file}.{member}]" +
                              $"[{logLevel}]" +
                              $" {formatter(state, exception)}";
                lock (_writer)
                {
                    _writer.WriteLine(logLine);
                }
                lock (AllLogWriter)
                {
                    AllLogWriter.WriteLine(logLine);
                }
            }
        }
    }

    /// <summary>
    /// 控制台日志格式化
    /// </summary>
    private class LogConsoleFormatter : ConsoleFormatter
    {
        public LogConsoleFormatter() : base("logFormatter") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            var logLevel = logEntry.LogLevel;
            var category = logEntry.Category;
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

            string file = "", member = "";
            if (logEntry.State is CallerState callerState)
            {
                file = callerState.FileName;
                member = callerState.Member;
            }

            // 定义不同级别的ANSI颜色
            string color = logLevel switch
            {
                LogLevel.Trace => "\u001b[90m",        // 灰色
                LogLevel.Debug => "\u001b[36m",        // 青色
                LogLevel.Information => "\u001b[32m",  // 绿色
                LogLevel.Warning => "\u001b[33m",      // 黄色
                LogLevel.Error => "\u001b[31m",        // 红色
                LogLevel.Critical => "\u001b[35m",     // 洋红
                _ => "\u001b[0m"                       // 默认
            };

            textWriter.Write(color);
            textWriter.Write(
                $"[{DateTime.Now:HH:mm:ss.fff}][{category}][{file}.{member}][{logLevel}] {message}\n"
            );
            textWriter.Write("\u001b[0m");
        }
    }

    /// <summary>
    /// 创建带有调用者信息的 Logger 实例。
    /// </summary>
    public static Logger CreateLogger(string name)
        => new(LoggerFactory.CreateLogger(name));

    /// <summary>
    /// 动态设置日志级别（会重建 LoggerFactory，已创建的 Logger 实例需重新获取）。
    /// </summary>
    public static void SetLogLevel(LogLevel loglevel)
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(loglevel)
                .AddProvider(new MultiFileLoggerProvider())
                .AddConsoleFormatter<LogConsoleFormatter, ConsoleFormatterOptions>()
                .AddConsole(options =>
                {
                    options.FormatterName = "logFormatter";
                });
        });
    }

    /// <summary>
    /// 初始化 AdvancedLoggerFactory
    /// </summary>
    static AdvancedLoggerFactory()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder
                .AddProvider(new MultiFileLoggerProvider())
                .AddConsole();
        });
    }
}
