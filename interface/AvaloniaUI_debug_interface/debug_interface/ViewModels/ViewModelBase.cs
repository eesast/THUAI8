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
using Playback; // *** 添加 Playback 命名空间引用 ***
using Timothy.FrameRateTask; // *** 添加 FrameRateTask 引用 ***

namespace debug_interface.ViewModels
{
    public partial class ViewModelBase : ObservableObject, IDisposable
    {
        private DispatcherTimer timerViewModel;

        [ObservableProperty]
        private string title = "THUAI8"; // 标题

        // 连接相关字段
        private readonly string ip;
        private readonly string port;
        private long playerID;
        private long teamID;
        private long CharacterIdTypeID;

        // gRPC 相关
        private AvailableService.AvailableServiceClient? client;
        private AsyncServerStreamingCall<MessageToClient>? responseStream;
        private Task? _receiveTask;
        private volatile bool _isConnected = false;
        private volatile bool _isConnecting = false;
        private const int ReconnectIntervalMs = 3000;

        [ObservableProperty]
        private string connectionStatus = "正在初始化...";

        // 模式标志
        private bool isSpectatorMode = false;
        private bool isPlaybackMode = false; // 回放模式标志

        // *** 通用 CancellationTokenSource ***
        private CancellationTokenSource? _cts;

        // *** 回放相关字段 ***
        private FrameRateTaskExecutor<int>? _playbackExecutor; 
        private MessageReader? _playbackReader;
        private long _playbackFrameDurationMs;

        // 日志
        public Logger? myLogger;
        public Logger? lockGenerator;

        // 服务器/回放消息列表
        public List<MessageOfCharacter> listOfCharacters = new();
        public List<MessageOfBarracks> listOfBarracks = new();
        public List<MessageOfTrap> listOfTraps = new();
        public List<MessageOfSpring> listOfSprings = new();
        public List<MessageOfFarm> listOfFarms = new();
        public List<MessageOfEconomyResource> listOfEconomyResources = new();
        public List<MessageOfAdditionResource> listOfAdditionResources = new();
        public List<MessageOfAll> listOfAll = new();

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

                string logDir = Path.Combine(d.InstallPath, "Logs");
                Directory.CreateDirectory(logDir);
                myLogger = LoggerProvider.FromFile(Path.Combine(logDir, $"Client.{teamID}.{playerID}.log"));

                timerViewModel = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timerViewModel.Tick += TimerTickHandler;
                timerViewModel.Start();

                if (!string.IsNullOrEmpty(playbackFile) && File.Exists(playbackFile))
                {
                    isPlaybackMode = true;
                    myLogger?.LogInfo($"进入回放模式: {playbackFile}");
                    ConnectionStatus = $"回放: {Path.GetFileName(playbackFile)}";
                    // *** 启动异步回放 ***
                    _ = PlaybackAsync(playbackFile, playbackSpeed);
                }
                else if (!string.IsNullOrEmpty(playbackFile))
                {
                    myLogger?.LogError($"回放文件不存在: {playbackFile}");
                    ConnectionStatus = "错误: 回放文件未找到";
                }
                else
                {
                    _ = TryConnectAndReceiveLoopAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化ViewModelBase时出错: {ex.Message}\n{ex.StackTrace}");
                myLogger?.LogError($"初始化ViewModelBase时出错: {ex.Message}\n{ex.StackTrace}");
                ConnectionStatus = "初始化失败";
            }
        }

        private void TimerTickHandler(object? sender, EventArgs e)
        {
            Refresh(sender, e);
        }

        // *** 回放主逻辑 ***
        private async Task PlaybackAsync(string fileName, double pbSpeed = 2.0)
        {
            _cts?.Cancel(); // 取消之前的任务
            _cts = new CancellationTokenSource();

            _playbackReader = null;
            try
            {
                _playbackReader = new MessageReader(fileName);
                myLogger?.LogInfo($"PlaybackAsync: MessageReader 创建成功 for {fileName}");
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"PlaybackAsync: 创建 MessageReader 失败: {ex.Message}");
                ConnectionStatus = "错误: 无法读取回放文件";
                isPlaybackMode = false;
                return;
            }

            _playbackFrameDurationMs = Math.Max(1, (long)(50 / pbSpeed)); // 基础帧率 50ms
            myLogger?.LogInfo($"PlaybackAsync: Playback speed={pbSpeed}x, Frame duration={_playbackFrameDurationMs}ms");

            _playbackExecutor = new FrameRateTaskExecutor<int>(
                PlaybackLoopCondition,    // Func<bool> loopCondition
                PlaybackLoopAction,       // Func<bool> loopToDo
                _playbackFrameDurationMs, // long timeInterval
                PlaybackOnFinished        // Func<int> finallyReturn
            );
            // _playbackExecutor.AllowTimeExceed = true; // 如果需要

            _playbackExecutor.Start(); // 非阻塞启动
            myLogger?.LogInfo("PlaybackAsync: Playback executor started.");
            await Task.CompletedTask; // PlaybackAsync 本身可以是非 async void，但 async Task 更规范
        }

        // *** 回放循环条件 ***
        private bool PlaybackLoopCondition()
        {
            return !_cts!.IsCancellationRequested && _playbackReader != null;
        }

        // *** 回放循环动作 ***
        private bool PlaybackLoopAction()
        {
            if (_cts!.IsCancellationRequested || _playbackReader == null) return false;

            try
            {
                MessageToClient? content = _playbackReader.ReadOne();
                if (content == null)
                {
                    myLogger?.LogInfo("PlaybackLoopAction: Reached end of file.");
                    _cts.Cancel(); // 文件结束，请求停止
                    return false;
                }

                lock (drawPicLock)
                {
                    // 清理当前帧数据列表
                    listOfCharacters.Clear();
                    listOfBarracks.Clear();
                    listOfTraps.Clear();
                    listOfSprings.Clear();
                    listOfFarms.Clear();
                    listOfEconomyResources.Clear();
                    listOfAdditionResources.Clear();
                    listOfAll.Clear();

                    if (content.GameState != GameState.GameRunning && content.GameState != GameState.NullGameState)
                        myLogger?.LogInfo($"Playback GameState: {content.GameState}");

                    // 填充列表
                    myLogger?.LogDebug($"GameState: {content.GameState}");
                    foreach (var obj in content.ObjMessage)
                    {
                        //myLogger?.LogDebug($"MessageOfObjCase总共: {obj.MessageOfObjCase}");
                        switch (obj.MessageOfObjCase)
                        {
                            
                            case MessageOfObj.MessageOfObjOneofCase.CharacterMessage:
                                listOfCharacters.Add(obj.CharacterMessage);
                                break;
                            case MessageOfObj.MessageOfObjOneofCase.BarracksMessage:
                                //myLogger?.LogDebug("BarracksMessage: " + obj.BarracksMessage.ToString());
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
                                var mapMsg = obj.MapMessage;
                                if (this is MainWindowViewModel vm && vm.MapVM != null)
                                {
                                    vm.currentMapMessage = mapMsg;
                                    // 地图更新直接在回放线程安排到UI线程
                                    Dispatcher.UIThread.InvokeAsync(() => vm.MapVM.UpdateMap(mapMsg));
                                }
                                break;
                        }
                    }
                    if (content.AllMessage != null)
                    {
                        listOfAll.Add(content.AllMessage);
                    }
                } // lock 结束
                return true; // 成功处理，继续循环
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"PlaybackLoopAction: Error during frame processing: {ex.Message}");
                _cts.Cancel(); // 出错时请求停止
                return false;
            }
        }

        // *** 回放结束回调 ***
        private int PlaybackOnFinished()
        {
            myLogger?.LogInfo("PlaybackOnFinished called.");
            ConnectionStatus = "回放结束";
            _playbackReader?.Dispose();
            _playbackReader = null;
            return 0; // 返回结果
        }

        // *** 核心连接与接收循环***
        private async Task TryConnectAndReceiveLoopAsync()
        {
            if (isPlaybackMode || _isConnected || _isConnecting) return;
            _isConnecting = true;
            ConnectionStatus = "正在连接服务器...";
            myLogger?.LogInfo("开始尝试连接服务器...");
            isSpectatorMode = playerID > 2020; // 保持旁观者判断逻辑
            if (isSpectatorMode) myLogger?.LogInfo("检测到旁观者模式。");

            while (!_isConnected && (_cts == null || !_cts.IsCancellationRequested))
            {
                Channel? channel = null;
                try
                {
                    string connect = $"{ip}:{port}";
                    var connectOptions = new List<ChannelOption> { /* ... */ new ChannelOption(ChannelOptions.MaxSendMessageLength, -1), new ChannelOption(ChannelOptions.MaxReceiveMessageLength, -1), };
                    channel = new Channel(connect, ChannelCredentials.Insecure, connectOptions);

                    myLogger?.LogDebug($"尝试连接到 {connect}(等待最多 5 秒)...");
                    await channel.ConnectAsync(deadline: DateTime.UtcNow.AddSeconds(60));
                    myLogger?.LogDebug("通道连接成功，创建客户端...");
                    client = new AvailableService.AvailableServiceClient(channel);

                    CharacterMsg playerMsg = new CharacterMsg { CharacterId = playerID };
                    if (!isSpectatorMode)
                    {
                        playerMsg.TeamId = teamID;
                        playerMsg.CharacterType = CharacterIdTypeID switch
                        { /* ... case ... */
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
                            11 => CharacterType.ZhiZhujing,
                            12 => CharacterType.Pawn,
                            _ => CharacterType.NullCharacterType
                        };
                        myLogger?.LogDebug($"准备发送 AddCharacter: PlayerID={playerMsg.CharacterId}, TeamID={playerMsg.TeamId}, CharacterType={playerMsg.CharacterType}");
                    }
                    else
                    {
                        myLogger?.LogDebug($"准备发送 AddCharacter (旁观者): PlayerID={playerMsg.CharacterId} (TeamID和CharacterType将使用默认值)");
                    }

                    myLogger?.LogDebug("调用 AddCharacter...");
                    _cts?.Cancel(); // 取消旧的
                    _cts = new CancellationTokenSource();
                    responseStream = client.AddCharacter(playerMsg, cancellationToken: _cts.Token);
                    myLogger?.LogDebug("AddCharacter 调用成功，等待服务器消息...");

                    _receiveTask = Task.Run(ReceiveMessagesAsync, _cts.Token);

                    _isConnected = true;
                    _isConnecting = false;
                    ConnectionStatus = "已连接";
                    myLogger?.LogInfo("服务器连接成功，开始接收消息。");
                    break;
                }
                catch (RpcException ex) when (/* ... */ ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded || ex.StatusCode == StatusCode.Cancelled)
                {
                    myLogger?.LogDebug($"连接服务器失败 (Code: {ex.StatusCode})，将在 {ReconnectIntervalMs / 1000} 秒后重试...");
                    ConnectionStatus = $"连接失败，{ReconnectIntervalMs / 1000}秒后重试...";
                    await CleanupConnectionAsync();
                    _cts?.Cancel(); _cts?.Dispose(); _cts = null; // 清理CTS
                    await Task.Delay(ReconnectIntervalMs, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    myLogger?.LogDebug($"连接过程中发生意外错误: {ex.Message}\n{ex.StackTrace}");
                    ConnectionStatus = "连接出错，请检查";
                    await CleanupConnectionAsync();
                    _cts?.Cancel(); _cts?.Dispose(); _cts = null; // 清理CTS
                    await Task.Delay(ReconnectIntervalMs * 2, CancellationToken.None);
                }
            }
            _isConnecting = false;
            if (!_isConnected && (_cts == null || !_cts.IsCancellationRequested)) { /* ... */ }
        }

        // *** 消息接收循环 (恢复地图直接更新) ***
        private async Task ReceiveMessagesAsync()
        {
            try
            {
                myLogger?.LogInfo("后台接收任务启动...");
                while (responseStream != null && await responseStream.ResponseStream.MoveNext(_cts!.Token))
                {
                    lock (drawPicLock)
                    {
                        // 清理列表
                        listOfCharacters.Clear(); listOfBarracks.Clear(); listOfTraps.Clear();
                        listOfSprings.Clear(); listOfFarms.Clear(); listOfEconomyResources.Clear();
                        listOfAdditionResources.Clear(); listOfAll.Clear();

                        MessageToClient content = responseStream.ResponseStream.Current;

                        if (content.GameState != GameState.GameRunning)
                            myLogger?.LogInfo($"GameState: {content.GameState}");

                        foreach (var obj in content.ObjMessage)
                        {
                            switch (obj.MessageOfObjCase)
                            {
                                
                                case MessageOfObj.MessageOfObjOneofCase.CharacterMessage:
                                    listOfCharacters.Add(obj.CharacterMessage);
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
                                    // *** 直接更新地图 ***
                                    var mapMsg = obj.MapMessage;

                                    if (this is MainWindowViewModel vm && vm.MapVM != null)
                                    {
                                        vm.currentMapMessage = mapMsg;
                                        Dispatcher.UIThread.InvokeAsync(() => vm.MapVM.UpdateMap(mapMsg));
                                    }
                                    break;
                            }
                        }
                        if (listOfCharacters.Count == 0 && listOfBarracks.Count == 0 && listOfTraps.Count == 0 && listOfSprings.Count == 0 && listOfFarms.Count == 0 && listOfEconomyResources.Count == 0 && listOfAdditionResources.Count == 0 && listOfAll.Count == 0)
                        { myLogger?.LogWarning("服务器消息流中没有任何消息。"); }
                        //输出对应没有的信息
                        if (content.AllMessage != null) { listOfAll.Add(content.AllMessage); }
                    } // lock 结束
                }
                myLogger?.LogInfo("服务器消息流结束 (正常关闭或取消)。");
                
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled) { myLogger?.LogWarning("消息接收任务被取消。"); }
            catch (IOException ex) when (ex.InnerException is System.Net.Sockets.SocketException socketEx) { myLogger?.LogError($"网络错误导致接收中断: {socketEx.Message}"); }
            catch (Exception ex) { myLogger?.LogError($"接收消息时发生意外错误: {ex.Message}\n{ex.StackTrace}"); }
            finally
            {
                myLogger?.LogDebug("后台接收任务结束。");
                bool wasConnected = _isConnected;
                await CleanupConnectionAsync();
                if (wasConnected && !isPlaybackMode && (_cts == null || !_cts.IsCancellationRequested))
                {
                    myLogger?.LogInfo("检测到连接断开，尝试自动重连...");
                    _ = TryConnectAndReceiveLoopAsync();
                }
            }
        }

        // *** 清理连接资源 (使用 _cts) ***
        private async Task CleanupConnectionAsync()
        {
            myLogger?.LogDebug("开始清理连接资源...");
            _isConnected = false;
            _isConnecting = false; // 确保重置
            ConnectionStatus = "连接已断开";
            //myLogger?.LogDebug("正在取消 CancellationToken...");
            if (_cts != null) { try { _cts.Cancel(); myLogger?.LogDebug(" CancellationToken 已请求取消。"); } catch (ObjectDisposedException) { } }
            if (_receiveTask != null && !_receiveTask.IsCompleted) { /* ... 等待任务 ... */ try { myLogger?.LogDebug(" 等待接收任务结束..."); await Task.WhenAny(_receiveTask, Task.Delay(500)); if (!_receiveTask.IsCompleted) myLogger?.LogWarning(" 接收任务在超时后仍未结束。"); else myLogger?.LogDebug(" 接收任务已结束。"); } catch (Exception ex) { myLogger?.LogError($"等待接收任务结束时出错: {ex.Message}"); } }
            if (responseStream != null) { try { responseStream.Dispose(); } catch { } responseStream = null; myLogger?.LogDebug(" responseStream 已 Dispose。"); }
            // 不在此处 Dispose _cts，由调用者（Dispose 或重连逻辑）处理
            client = null;
            _receiveTask = null;
            //myLogger?.LogDebug("连接资源清理完毕 (除了CTS Dispose)。");
        }

        // *** Dispose 方法 (清理 _cts 和 _playbackReader) ***
        public virtual void Dispose()
        {
            myLogger?.LogInfo("ViewModelBase Dispose 开始...");
            timerViewModel?.Stop();

            _cts?.Cancel(); // 请求取消当前活动（gRPC 或 Playback）
            CleanupConnectionAsync().ConfigureAwait(false).GetAwaiter().GetResult(); // 清理 gRPC 特定资源

            _playbackReader?.Dispose(); // 清理回放 Reader
            _playbackReader = null;

            _cts?.Dispose(); // 最后 Dispose CTS
            _cts = null;

            myLogger?.LogInfo("ViewModelBase Dispose 完成。");
            GC.SuppressFinalize(this);
        }

        // *** Refresh 方法 (更新 UI) ***
        private void Refresh(object? sender, EventArgs e)
        {
            try
            {
                OnTimerTick(sender, e);
                if (this is MainWindowViewModel vm)
                {
                    // 地图更新由 ReceiveMessagesAsync 或 PlaybackLoopAction 直接触发
                    // Refresh 只负责根据列表更新其他 UI
                    vm.UpdateCharacters();
                    vm.UpdateMapElements();
                    vm.UpdateGameStatus();
                }
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"刷新UI时出错: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected virtual void OnTimerTick(object? sender, EventArgs e) { }

    }
}