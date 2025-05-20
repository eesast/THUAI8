using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Timothy.FrameRateTask;

namespace Preparation.Utility.Logging;

public class LogQueue
{
    public static LogQueue Global { get; } = new();
    private static uint logNum = 0;
    private static uint logCopyNum = 0;
    private static readonly object queueLock = new();

    private readonly Queue<string> logInfoQueue = new();

    public void Commit(string info)
    {
        lock (queueLock) logInfoQueue.Enqueue(info);
    }

    public static bool IsClosed { get; private set; } = false;
    public static void Close()
    {
        if (IsClosed) return;
        LogWrite();
        LogCopy();
        IsClosed = true;
    }

    static void LogCopy()
    {
        if (IsClosed) return;
        string copyPath = $"{LoggingData.ServerLogPath}-copy{logCopyNum}.txt";
        if (File.Exists(copyPath))
            File.Delete(copyPath);
        File.Copy(LoggingData.ServerLogPath, copyPath);
        logCopyNum++;
        File.Delete(LoggingData.ServerLogPath);
        logNum = 0;
    }
    static void LogWrite()
    {
        if (IsClosed) return;
        lock (queueLock)
        {
            while (Global.logInfoQueue.Count != 0)
            {
                var info = Global.logInfoQueue.Dequeue();
                File.AppendAllText(LoggingData.ServerLogPath, info + Environment.NewLine);
                logNum++;
                if (logNum >= LoggingData.MaxLogNum)
                    LogCopy();
            }
        }
    }

    private LogQueue()
    {
        if (File.Exists(LoggingData.ServerLogPath))
            File.Delete(LoggingData.ServerLogPath);
        File.AppendAllText(LoggingData.ServerLogPath, $"[{Logger.NowDate()}]" + Environment.NewLine);
        new Thread(() =>
        {
            new FrameRateTaskExecutor<int>(
                loopCondition: () => Global != null,
                loopToDo: LogWrite,
                timeInterval: 100,
                finallyReturn: () =>
                {
                    Close();
                    return 0;
                }
                ).Start();
        })
        { IsBackground = true }.Start();
    }
}

public class Logger(string module)
{
    public readonly string Module = module;
    public bool Enable { get; set; } = true;
    public bool Background { get; set; } = false;

    public void ConsoleLog(string msg, bool Duplicate = true)
    {
        var info = $"[{NowTime()}][{Module}] {msg}";
        if (Enable)
        {
            if (!Background)
                Console.WriteLine(info);
            if (Duplicate)
                LogQueue.Global.Commit(info);
        }
    }
    public void ConsoleLogDebug(string msg, bool Duplicate = true)
    {
#if DEBUG
        var info = $"[{NowTime()}][{Module}] {msg}";
        if (Enable)
        {
            if (!Background)
                Console.WriteLine(info);
            if (Duplicate)
                LogQueue.Global.Commit(info);
        }
#endif
    }
    public static void RawConsoleLog(string msg, bool Duplicate = true)
    {
        Console.WriteLine(msg);
        if (Duplicate)
            LogQueue.Global.Commit(msg);
    }
    public static void RawLogDebug(string msg, bool Duplicate = true)
    {
#if DEBUG
        Console.WriteLine(msg);
        if (Duplicate)
            LogQueue.Global.Commit(msg);
#endif
    }

    public static string TypeName(object obj)
        => obj.GetType().Name;
    public static string TypeName(Type tp)
        => tp.Name;
    public static string ObjInfo(object obj, string msg = "")
        => msg == "" ? $"<{TypeName(obj)}>"
                     : $"<{TypeName(obj)} {msg}>";
    public static string ObjInfo(Type tp, string msg = "")
        => msg == "" ? $"<{TypeName(tp)}>"
                     : $"<{TypeName(tp)} {msg}>";

    public static string NowTime()
    {
        DateTime now = DateTime.Now;
        return $"{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}.{now.Millisecond:D3}";
    }
    public static string NowDate()
    {
        DateTime now = DateTime.Today;
        return $"{now.Year:D4}/{now.Month:D2}/{now.Day:D2} {now.DayOfWeek}";
    }
}

public static class LoggingData
{
    public const string ServerLogPath = "log.txt";
    public const uint MaxLogNum = 5000;
}

public class LoggerF
{
    private static ILoggerFactory? _loggerFactory;
    public static ILoggerFactory loggerFactory
    {
        get
        {
            if (_loggerFactory is null)
                throw new InvalidOperationException("LoggerF.loggerFactory 尚未初始化！");
            return _loggerFactory;
        }
        private set => _loggerFactory = value;
    }
    private class MultiFileLoggerProvider : ILoggerProvider
    {
        private readonly Dictionary<string, StreamWriter> _writers = new();
        private static readonly string AllLogPath = "logs/all.log";
        private static readonly StreamWriter AllLogWriter;

        static MultiFileLoggerProvider()
        {
            var logDir = "logs";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            AllLogWriter = new StreamWriter(AllLogPath, append: false) { AutoFlush = true };
        }
        public ILogger CreateLogger(string categoryName)
        {
            // categoryName 就是 logger 名称
            if (!_writers.ContainsKey(categoryName))
            {
                var logDir = "logs";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
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
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{_categoryName}][{eventId.Id}:{eventId.Name}][{logLevel}] {formatter(state, exception)}";
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

    };

    private class LogConsoleFormatter : ConsoleFormatter
    {
        public LogConsoleFormatter() : base("logFormatter") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            var logLevel = logEntry.LogLevel;
            var category = logEntry.Category;
            var eventId = logEntry.EventId;
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

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
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{category}][{eventId.Id}:{eventId.Name}][{logLevel}] {message}\n"
            );

            textWriter.Write("\u001b[0m");
        }
    }
    public LoggerF(LogLevel logLevel)
    {
        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders()
                .SetMinimumLevel(logLevel)
                .AddProvider(new MultiFileLoggerProvider())
                .AddConsoleFormatter<LogConsoleFormatter, ConsoleFormatterOptions>()
                .AddConsole(options =>
                {
                    options.FormatterName = "logFormatter";
                });
        });
    }
}