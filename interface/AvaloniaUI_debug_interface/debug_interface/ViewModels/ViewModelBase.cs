// ViewModels\ViewModelBase.cs
using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Grpc.Core;
using installer.Model;
using installer.Data;
using System.Collections.Generic;
using Google.Protobuf;
using Protobuf;
using System.Threading.Tasks;
using System.Threading;
using System.IO;


namespace debug_interface.ViewModels
{
    // *** 确保实现了 IDisposable ***
    public partial class ViewModelBase : ObservableObject, IDisposable
    {
        private DispatcherTimer timerViewModel;

        [ObservableProperty]
        private string title = "THUAI8";

        // 连接相关字段
        private readonly string ip;
        private readonly string port;
        private long playerID; // 从配置读取
        private long teamID;   // 从配置读取
        private long CharacterIdTypeID; // 从配置读取 (注意: 这是类型ID, 不是实例ID)

        // gRPC 相关
        private AvailableService.AvailableServiceClient? client;
        private AsyncServerStreamingCall<MessageToClient>? responseStream;
        private CancellationTokenSource? _cts;
        private Task? _receiveTask;
        private volatile bool _isConnected = false;
        private volatile bool _isConnecting = false;
        private const int ReconnectIntervalMs = 3000;

        [ObservableProperty]
        private string connectionStatus = "正在初始化...";

        // 模式标志
        private bool isSpectatorMode = false; // 旁观者模式 TBD 如何确定
        private bool isPlaybackMode = false;

        // 日志
        public Logger? myLogger;
        public Logger? lockGenerator;

        // 服务器消息列表 (保持不变)
        public List<MessageOfCharacter> listOfCharacters = new();
        public List<MessageOfBarracks> listOfBarracks = new();
        public List<MessageOfTrap> listOfTraps = new();
        public List<MessageOfSpring> listOfSprings = new();
        public List<MessageOfFarm> listOfFarms = new();
        public List<MessageOfEconomyResource> listOfEconomyResources = new();
        public List<MessageOfAdditionResource> listOfAdditionResources = new();
        public List<MessageOfAll> listOfAll = new();
        // public List<MessageOfMonkeySkill> listOfPMonkeySkill = new(); // 如果需要

        public readonly object drawPicLock = new();

        public ViewModelBase()
        {
            try
            {
                var d = new ConfigData();
                ip = d.Commands.IP;
                port = d.Commands.Port;
                playerID = Convert.ToInt64(d.Commands.PlayerID);
                teamID = Convert.ToInt64(d.Commands.TeamID);
                CharacterIdTypeID = Convert.ToInt64(d.Commands.CharacterType);
                string? playbackFile = d.Commands.PlaybackFile;
                double playbackSpeed = d.Commands.PlaybackSpeed;

                // 初始化日志记录器
                string logDir = Path.Combine(d.InstallPath, "Logs"); // 先获取日志目录
                Directory.CreateDirectory(logDir); // 确保目录存在
                myLogger = LoggerProvider.FromFile(Path.Combine(logDir, $"Client.{teamID}.{playerID}.log"));
                // lockGenerator = LoggerProvider.FromFile(Path.Combine(logDir, $"lock.{teamID}.{playerID}.log")); // 如果需要

                // 初始化定时器
                timerViewModel = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timerViewModel.Tick += TimerTickHandler;
                timerViewModel.Start();

                // 判断是回放模式还是实时连接模式
                if (!string.IsNullOrEmpty(playbackFile))
                {
                    isPlaybackMode = true;
                    myLogger?.LogInfo($"进入回放模式: {playbackFile}");
                    ConnectionStatus = "回放模式";
                    Playback(playbackFile, playbackSpeed);
                }
                else
                {
                    // *** 启动异步连接循环 ***
                    _ = TryConnectAndReceiveLoopAsync();
                }
            }
            catch (Exception ex)
            {
                // 如果 myLogger 还未初始化，则输出到控制台
                Console.WriteLine($"初始化ViewModelBase时出错: {ex.Message}\n{ex.StackTrace}");
                myLogger?.LogError($"初始化ViewModelBase时出错: {ex.Message}\n{ex.StackTrace}");
                ConnectionStatus = "初始化失败";
            }
        }

        // 定时器处理函数
        private void TimerTickHandler(object? sender, EventArgs e)
        {
            Refresh(sender, e); // 调用刷新逻辑
            // 重连逻辑由 TryConnectAndReceiveLoopAsync 和 ReceiveMessagesAsync 驱动
        }

        // *** 核心连接与接收循环 ***
        private async Task TryConnectAndReceiveLoopAsync()
        {
            if (isPlaybackMode || _isConnected || _isConnecting) return;

            _isConnecting = true;
            ConnectionStatus = "正在连接服务器...";
            myLogger?.LogInfo("开始尝试连接服务器...");

            // 确定是否为旁观者模式 (可以在这里加逻辑，例如 PlayerID > 某个值)
            isSpectatorMode = playerID > 2020; // 示例逻辑
            if (isSpectatorMode) myLogger?.LogInfo("检测到旁观者模式。");

            while (!_isConnected && (_cts == null || !_cts.IsCancellationRequested))
            {
                Channel? channel = null;
                try
                {
                    string connect = $"{ip}:{port}";


                    var connectOptions = new List<ChannelOption>
                    {
                        new ChannelOption(ChannelOptions.MaxSendMessageLength, -1),
                        new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1),
                    };
                    channel = new Channel(connect, ChannelCredentials.Insecure, connectOptions);

                    myLogger?.LogDebug($"尝试连接到 {connect}(等待最多 5 秒)...");
                    await channel.ConnectAsync(deadline: DateTime.UtcNow.AddSeconds(5));

                    myLogger?.LogDebug("通道连接成功，创建客户端...");
                    client = new AvailableService.AvailableServiceClient(channel);

                    // 准备玩家消息
                    CharacterMsg playerMsg = new CharacterMsg();
                    playerMsg.CharacterId = playerID; // 使用构造函数中读取的值

                    if (!isSpectatorMode)
                    {
                        playerMsg.TeamId = teamID; // 使用构造函数中读取的值
                        // *** 补全 switch 语句 ***
                        playerMsg.CharacterType = CharacterIdTypeID switch // 使用构造函数中读取的 CharacterIdTypeID
                        {
                            1 => CharacterType.TangSeng,
                            2 => CharacterType.SunWukong,
                            3 => CharacterType.ZhuBajie,
                            4 => CharacterType.ShaWujing,
                            5 => CharacterType.BaiLongma,
                            6 => CharacterType.Monkid,
                            7 => CharacterType.JiuLing,
                            8 => CharacterType.HongHaier,
                            9 => CharacterType.NiuMowang,
                            10 => CharacterType.TieShan,
                            11 => CharacterType.ZhiZhujing, // 确认是否包含蜘蛛精
                            12 => CharacterType.Pawn,
                            _ => CharacterType.NullCharacterType // 默认值
                        };
                        myLogger?.LogDebug($"准备发送 AddCharacter: PlayerID={playerMsg.CharacterId}, TeamID={playerMsg.TeamId}, CharacterType={playerMsg.CharacterType}");
                    }
                    else
                    {
                        myLogger?.LogDebug($"准备发送 AddCharacter (旁观者): PlayerID={playerMsg.CharacterId}");
                        // 旁观者可能不需要发送 TeamID 和 CharacterType，根据服务器要求调整
                        // playerMsg.TeamId = ...; // 可能不需要设置
                        // playerMsg.CharacterType = CharacterType.NullCharacterType; // 可能设为 Null
                    }

                    myLogger?.LogDebug("调用 AddCharacter...");
                    _cts = new CancellationTokenSource();
                    responseStream = client.AddCharacter(playerMsg, cancellationToken: _cts.Token);
                    myLogger?.LogInfo("AddCharacter 调用成功，等待服务器消息...");

                    // 启动后台接收任务
                    _receiveTask = Task.Run(ReceiveMessagesAsync, _cts.Token);

                    _isConnected = true;
                    _isConnecting = false;
                    ConnectionStatus = "已连接";
                    myLogger?.LogInfo("服务器连接成功，开始接收消息。");
                    break; // 连接成功，退出重试循环
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded || ex.StatusCode == StatusCode.Cancelled)
                {
                    myLogger?.LogWarning($"连接服务器失败 (Code: {ex.StatusCode})，将在 {ReconnectIntervalMs / 1000} 秒后重试...");
                    ConnectionStatus = $"连接失败，{ReconnectIntervalMs / 1000}秒后重试...";
                    await CleanupConnectionAsync();
                    await Task.Delay(ReconnectIntervalMs, CancellationToken.None); // 使用 CancellationToken.None 防止在 Dispose 时被取消
                }
                catch (Exception ex)
                {
                    myLogger?.LogError($"连接过程中发生意外错误: {ex.Message}\n{ex.StackTrace}");
                    ConnectionStatus = "连接出错，请检查";
                    await CleanupConnectionAsync();
                    await Task.Delay(ReconnectIntervalMs * 2, CancellationToken.None);
                }
            }
            _isConnecting = false;
            if (!_isConnected && (_cts == null || !_cts.IsCancellationRequested))
            {
                myLogger?.LogWarning("连接重试循环结束但未连接成功 (可能被外部取消)。");
            }
        }

        // *** 消息接收循环 (保持不变) ***
        private async Task ReceiveMessagesAsync()
        {
            try
            {
                myLogger?.LogInfo("后台接收任务启动...");
                while (responseStream != null && await responseStream.ResponseStream.MoveNext(_cts!.Token))
                {
                    // myLogger?.LogTrace("接收到新消息帧...");
                    lock (drawPicLock)
                    {
                        // 清理列表
                        listOfCharacters.Clear();
                        listOfBarracks.Clear();
                        listOfTraps.Clear();
                        listOfSprings.Clear();
                        listOfFarms.Clear();
                        listOfEconomyResources.Clear();
                        listOfAdditionResources.Clear();
                        listOfAll.Clear();

                        MessageToClient content = responseStream.ResponseStream.Current;
                        MessageOfMap mapMessage = new MessageOfMap();
                        bool hasMapMessage = false;

                        // 处理 GameState
                        if (content.GameState != GameState.GameRunning)
                            myLogger?.LogInfo($"GameState: {content.GameState}");

                        // 处理消息对象
                        foreach (var obj in content.ObjMessage)
                        {
                            switch (obj.MessageOfObjCase)
                            {
                                case MessageOfObj.MessageOfObjOneofCase.CharacterMessage:
                                    listOfCharacters.Add(obj.CharacterMessage);
                                    // myLogger?.LogTrace($"接收角色: Guid={obj.CharacterMessage.Guid}, HP={obj.CharacterMessage.Hp}");
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.BarracksMessage:
                                    listOfBarracks.Add(obj.BarracksMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.TrapMessage:
                                    listOfTraps.Add(obj.TrapMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.SpringMessage:
                                    listOfSprings.Add(obj.SpringMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.FarmMessage:
                                    listOfFarms.Add(obj.FarmMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.EconomyResourceMessage:
                                    listOfEconomyResources.Add(obj.EconomyResourceMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.AdditionResourceMessage:
                                    listOfAdditionResources.Add(obj.AdditionResourceMessage);
                                    break;
                                case MessageOfObj.MessageOfObjOneofCase.MapMessage:
                                    mapMessage = obj.MapMessage;
                                    hasMapMessage = true;
                                    break;
                                    // default: myLogger?.LogWarning($"收到未处理的消息类型: {obj.MessageOfObjCase}"); break;
                            }
                        }
                        // myLogger?.LogDebug($"本帧处理完成，角色数: {listOfCharacters.Count}");

                        // 处理全局信息
                        if (content.AllMessage != null)
                        {
                            listOfAll.Add(content.AllMessage);
                        }

                        // 处理地图更新
                        if (hasMapMessage && this is MainWindowViewModel vm && vm.MapVM != null)
                        {
                            Dispatcher.UIThread.InvokeAsync(() => vm.MapVM.UpdateMap(mapMessage));
                        }
                    } // lock 结束
                }
                myLogger?.LogInfo("服务器消息流结束 (正常关闭或取消)。");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                myLogger?.LogWarning("消息接收任务被取消。");
            }
            catch (IOException ex) when (ex.InnerException is System.Net.Sockets.SocketException socketEx)
            {
                myLogger?.LogError($"网络错误导致接收中断: {socketEx.Message}");
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"接收消息时发生意外错误: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                myLogger?.LogInfo("后台接收任务结束。");
                bool wasConnected = _isConnected; // 记录断开前的状态
                await CleanupConnectionAsync(); // 清理连接资源
                if (wasConnected && !isPlaybackMode && (_cts == null || !_cts.IsCancellationRequested)) // 只有之前连接过且未被明确取消才重连
                {
                    myLogger?.LogInfo("检测到连接断开，尝试自动重连...");
                    _ = TryConnectAndReceiveLoopAsync(); // 触发自动重连
                }
            }
        }

        // *** 清理连接资源 (保持不变) ***
        private async Task CleanupConnectionAsync()
        {
            myLogger?.LogDebug("开始清理连接资源...");
            _isConnected = false;
            ConnectionStatus = "连接已断开"; // 更新状态
            if (_cts != null)
            {
                try { _cts.Cancel(); myLogger?.LogDebug(" CancellationToken 已请求取消。"); }
                catch (ObjectDisposedException) { }
            }
            if (_receiveTask != null && !_receiveTask.IsCompleted)
            {
                try
                {
                    myLogger?.LogDebug(" 等待接收任务结束...");
                    // 不要在此处等待太久，避免 Dispose 卡死
                    await Task.WhenAny(_receiveTask, Task.Delay(500)); // 短暂等待
                    if (!_receiveTask.IsCompleted) myLogger?.LogWarning(" 接收任务在超时后仍未结束。");
                    else myLogger?.LogDebug(" 接收任务已结束。");
                }
                catch (Exception ex) { myLogger?.LogError($"等待接收任务结束时出错: {ex.Message}"); }
            }
            if (responseStream != null)
            {
                try { responseStream.Dispose(); } catch { }
                responseStream = null;
                myLogger?.LogDebug(" responseStream 已 Dispose。");
            }
            if (_cts != null)
            {
                try { _cts.Dispose(); } catch { }
                _cts = null;
                myLogger?.LogDebug(" CancellationTokenSource 已 Dispose。");
            }
            client = null;
            _receiveTask = null;
            myLogger?.LogDebug("连接资源清理完毕。");
        }

        // *** Dispose 方法 (保持不变) ***
        public virtual void Dispose()
        {
            myLogger?.LogInfo("ViewModelBase Dispose 开始...");
            timerViewModel?.Stop();
            // 使用 ConfigureAwait(false) 避免死锁风险，如果 Wait 可能在 UI 线程调用
            CleanupConnectionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            myLogger?.LogInfo("ViewModelBase Dispose 完成。");
            GC.SuppressFinalize(this);
        }

        // *** Refresh 方法  更新人物地图状态 ***
        private void Refresh(object? sender, EventArgs e)
        {
            try
            {
                OnTimerTick(sender, e);
                if (this is MainWindowViewModel vm)
                {
                    // *** 只在连接时更新数据 ***
                    if (_isConnected)
                    {
                        vm.UpdateCharacters();
                        vm.UpdateMapElements();
                        vm.UpdateGameStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"刷新UI时出错: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // *** OnTimerTick 方法***
        protected virtual void OnTimerTick(object? sender, EventArgs e) { }

        // *** Playback 方法  ***
        private void Playback(string fileName, double pbSpeed = 2.0)
        {
            isPlaybackMode = true;
            ConnectionStatus = $"回放: {Path.GetFileName(fileName)}";
            myLogger?.LogInfo($"Starting playback: {fileName} at speed {pbSpeed}");
            // TODO: 实现回放逻辑
        }


    }
}