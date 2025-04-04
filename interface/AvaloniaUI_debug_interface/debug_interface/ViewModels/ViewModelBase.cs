using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Grpc.Core;
using installer.Model;
using installer.Data;
using System.Collections.Generic;
using Google.Protobuf;
using Protobuf;



namespace debug_interface.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        // 用于 UI 刷新的定时器（Avalonia 的 DispatcherTimer）
        private DispatcherTimer timerViewModel;
        private int counterViewModelTest = 0;

        // 使用 CommunityToolkit 的 ObservableProperty 自动实现 INotifyPropertyChanged
        [ObservableProperty]
        private string title = "THUAI8";

        // 与服务器通信相关的字段
        private long playerID;
        private readonly string ip;
        private readonly string port;
        public readonly long CharacterIdTypeID;
        private long teamID;

        // 与服务器通信相关的 gRPC 客户端
        private AvailableService.AvailableServiceClient? client;
        private AsyncServerStreamingCall<MessageToClient>? responseStream;
        private bool isSpectatorMode = false;
        private bool isPlaybackMode = false;

        // 日志记录
        public Logger? myLogger; //  ？表示可空
        public Logger? lockGenerator;

        // 服务器消息相关的字段
        public List<MessageOfMonkeySkill> listOfPMonkeySkill = new();
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
            // 读取配置
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

                //初始化日志记录器
                myLogger = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"Client.{teamID}.{playerID}.log"));
                lockGenerator = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"lock.{teamID}.{playerID}.log"));

                // 使用 Avalonia 的 DispatcherTimer 定时刷新 UI
                timerViewModel = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                timerViewModel.Tick += Refresh;
                timerViewModel.Start();

                //判断是否走回放模式
                if (string.IsNullOrEmpty(d.Commands.PlaybackFile))
                {
                    string[] comInfo = new string[]
                    {
                        ip,
                        port,
                        Convert.ToString(playerID),
                        Convert.ToString(teamID),
                        Convert.ToString(CharacterIdTypeID),
                    };

                    ConnectToServer(comInfo);
                    OnReceive();
                }
                else
                {
                    myLogger?.LogInfo($"PlaybackFile: {d.Commands.PlaybackFile}");
                    Playback(d.Commands.PlaybackFile, playbackSpeed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化ViewModelBase时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 连接到服务器，初始化 gRPC 客户端
        /// </summary>
        /// <param name="comInfo">包含 ip、port、playerID、teamID、shipTypeID 的数组</param>
        public void ConnectToServer(string[] comInfo)
        {
            if (isPlaybackMode) return;
            if (Convert.ToInt64(comInfo[2]) > 2023)
            {
                isSpectatorMode = true;
                myLogger?.LogInfo("isSpectatorMode = true");
            }
            if (comInfo.Length != 5)
            {
                throw new Exception("Error Registration Information！");
            }

            string connect = $"{comInfo[0]}:{comInfo[1]}";
            Channel channel = new Channel(connect, ChannelCredentials.Insecure);
            client = new AvailableService.AvailableServiceClient(channel);
            CharacterMsg playerMsg = new CharacterMsg();
            playerID = Convert.ToInt64(comInfo[2]);
            playerMsg.CharacterId = playerID;
            if (!isSpectatorMode)
            {
                teamID = Convert.ToInt64(comInfo[3]);
                playerMsg.TeamId = teamID;

                playerMsg.CharacterType = Convert.ToInt64(comInfo[4]) switch
                {
                    1 => CharacterType.TangSeng,
                    2 => CharacterType.SunWukong,
                    3 => CharacterType.ZhuBajie,
                    4 => CharacterType.ShaWujing,
                    5 => CharacterType.BaiLongma,
                    6 => CharacterType.Monkid,
                    // 妖怪团队阵营角色
                    7 => CharacterType.JiuLing,
                    8 => CharacterType.HongHaier,
                    9 => CharacterType.NiuMowang,
                    10 => CharacterType.TieShan,
                    12 => CharacterType.Pawn,
                    _ => CharacterType.NullCharacterType
                };
            }
            responseStream = client.AddCharacter(playerMsg);
        }

        /// <summary>
        /// 异步接收服务器流消息
        /// </summary>
        private async void OnReceive()
        {
            try
            {
                myLogger?.LogInfo("============= OnReceiving Server Stream ================");
                while (responseStream != null && await responseStream.ResponseStream.MoveNext())
                {
                    myLogger?.LogInfo("============= Receiving Server Stream ================");
                    lock (drawPicLock)
                    {
                        // 清除所有列表
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

                        switch (content.GameState)
                        {
                            case GameState.GameStart:
                                myLogger?.LogInfo("============= GameState: Game Start ================");
                                break;
                            case GameState.GameRunning:
                                myLogger?.LogInfo("============= GameState: Game Running ================");
                                break;
                            case GameState.GameEnd:
                                myLogger?.LogInfo("============= GameState: Game End ================");
                                break;
                        }

                        // 处理所有消息
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
                                    mapMessage = obj.MapMessage;
                                    hasMapMessage = true;
                                    break;
                            }
                        }

                        // 存储全局游戏状态
                        listOfAll.Add(content.AllMessage);

                        // 如果有地图消息并且当前ViewModel是MainWindowViewModel
                        if (hasMapMessage && this is MainWindowViewModel vm)
                        {
                            vm.MapVM.UpdateMap(mapMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                myLogger?.LogError($"接收消息时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 播放回放数据（示例方法）
        /// </summary>
        /// <param name="fileName">回放文件</param>
        /// <param name="pbSpeed">播放速度</param>
        private void Playback(string fileName, double pbSpeed = 2.0)
        {
            //myLogger.LogInfo($"Starting playback with file: {fileName} at speed {pbSpeed}");
            // TODO: 实现回放逻辑
            isPlaybackMode = true;
        }

        /// <summary>
        /// 可被子类重写的定时器事件处理方法
        /// </summary>
        protected virtual void OnTimerTick(object? sender, EventArgs e)
        {
            // 基类实现为空，子类可以重写
        }

        /// <summary>
        /// 定时器回调方法，用于刷新 UI 与游戏状态
        /// </summary>
        private void Refresh(object? sender, EventArgs e)
        {
            try
            {
                // 调用可被重写的方法
                OnTimerTick(sender, e);

                // 默认实现，更新UI
                if (this is MainWindowViewModel vm)
                {
                    // 更新角色信息
                    vm.UpdateCharacters();

                    // 更新地图上的各种元素
                    vm.UpdateMapElements();

                    // 更新游戏状态信息
                    vm.UpdateGameStatus();
                }
            }
            catch (Exception ex)
            {
                if (myLogger != null)
                    myLogger.LogError($"刷新UI时出错: {ex.Message}");
                else
                    Console.WriteLine($"刷新UI时出错: {ex.Message}");
            }
        }
    }
}