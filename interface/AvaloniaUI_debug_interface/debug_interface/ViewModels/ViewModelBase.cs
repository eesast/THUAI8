//ViewModeBase.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using debug_interface.Models;
using Google.Protobuf;
using Grpc.Core;
//using installer;
//using installer.Model;
using Avalonia.Controls.Shapes;
using Avalonia.Logging;
using System.Threading.Channels;

namespace debug_interface.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        // 用于 UI 刷新的定时器（Avalonia 的 DispatcherTimer）
        private DispatcherTimer timerViewModel;
        private int counterViewModelTest = 0;

        // 使用 CommunityToolkit 的 ObservableProperty 自动实现 INotifyPropertyChanged
        //[ObservableProperty]
        //private string title;

        //private MapPatch testPatch;
        //public MapPatch TestPatch
        //{
        //    get => testPatch;
        //    set => SetProperty(ref testPatch, value);
        //}

        //private List<Link> links;
        //public List<Link> Links
        //{
        //    get => links ??= new List<Link>();
        //    set => SetProperty(ref links, value);
        //}

        // 与服务器通信相关的字段
        private long playerID;
        private readonly string ip;
        private readonly string port;
        private readonly int shipTypeID;
        private long teamID;

        //private ShipType shipType;
        //private AvailableService.AvailableServiceClient? client;
        //private AsyncServerStreamingCall<MessageToClient>? responseStream;
        private bool isSpectatorMode = false;
        private bool isPlaybackMode = false;

        // 用于存储上次移动角度（示例，仅作为参考）
        private double lastMoveAngle;

        // 日志记录（保持你原来的 Logger 类）
        //public Logger myLogger;
        //public Logger lockGenerator;

        // 以下定义各个操作的命令（基于 CommunityToolkit.Mvvm.Input 的 RelayCommand）
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }
        public RelayCommand MoveLeftCommand { get; }
        public RelayCommand MoveRightCommand { get; }
        public RelayCommand MoveLeftUpCommand { get; }
        public RelayCommand MoveRightUpCommand { get; }
        public RelayCommand MoveLeftDownCommand { get; }
        public RelayCommand MoveRightDownCommand { get; }
        public RelayCommand AttackCommand { get; }
        public RelayCommand RecoverCommand { get; }
        public RelayCommand ProduceCommand { get; }
        public RelayCommand ConstructCommand { get; }

        public ViewModelBase()
        {
            //Title = "THUAI8;

            // 读取配置（假设 ConfigData 类来自 installer.Data 命名空间）
            //var d = new installer.Data.ConfigData();
            //ip = d.Commands.IP;
            //port = d.Commands.Port;
            //playerID = Convert.ToInt64(d.Commands.PlayerID);
            //teamID = Convert.ToInt64(d.Commands.TeamID);
            //shipTypeID = Convert.ToInt32(d.Commands.ShipType);
            //string playbackFile = d.Commands.PlaybackFile;
            //double playbackSpeed = d.Commands.PlaybackSpeed;

            // 初始化日志记录器
            //myLogger = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"Client.{teamID}.{playerID}.log"));
            //lockGenerator = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"lock.{teamID}.{playerID}.log"));

            // 初始化命令：这里仅示例了部分命令，其他命令同理
            MoveUpCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //// 构造移动消息，这里采用 gRPC 的消息格式（注意：根据你的 proto 定义，字段名称可能有所不同）
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI,
                //    time_in_milliseconds = 50
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveDownCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    // 这里使用 0 表示“负零”
                //    angle = 0,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveLeftCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI * 3 / 2,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveRightCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI / 2,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveLeftUpCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI * 5 / 4,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveRightUpCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI * 3 / 4,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveLeftDownCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI * 7 / 4,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            MoveRightDownCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //MoveMsg movemsg = new MoveMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    angle = Math.PI / 4,
                //    time_in_milliseconds = 100
                //};
                //lastMoveAngle = movemsg.angle;
                //client.Move(movemsg);
            });

            AttackCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //AttackMsg attackMsg = new AttackMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    attack_range = 50, // 示例值，根据实际情况修改
                //    attacked_character_id = 0 // 示例目标ID
                //};
                //client.Attack(attackMsg);
            });

            RecoverCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //RecoverMsg recoverMsg = new RecoverMsg
                //{
                //    character_id = playerID,
                //    recovered_hp = 10, // 示例数值
                //    team_id = teamID
                //};
                //client.Recover(recoverMsg);
            });

            ProduceCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //// 此处 Produce 可使用对应的 gRPC 方法（例如 Equip 或其他你定义的生产逻辑）
                //IDMsg idMsg = new IDMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID
                //};
                //client.Equip(idMsg);
            });

            ConstructCommand = new RelayCommand(() =>
            {
                //if (client == null || isSpectatorMode || isPlaybackMode)
                //{
                //    myLogger.LogInfo("Client is null or in Spectator/Playback mode");
                //    return;
                //}
                //ConstructMsg constructMsg = new ConstructMsg
                //{
                //    character_id = playerID,
                //    team_id = teamID,
                //    construction_type = ConstructionType.BARRACKS // 示例建筑类型
                //};
                //client.Construct(constructMsg);
            });

            // 使用 Avalonia 的 DispatcherTimer 定时刷新 UI
            timerViewModel = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            timerViewModel.Tick += Refresh;
            timerViewModel.Start();

            // 判断是否走回放模式
            //if (string.IsNullOrEmpty(d.Commands.PlaybackFile))
            //{
            //    string[] comInfo = new string[]
            //    {
            //        ip,
            //        port,
            //        Convert.ToString(playerID),
            //        Convert.ToString(teamID),
            //        Convert.ToString(shipTypeID),
            //    };
            //    ConnectToServer(comInfo);
            //    OnReceive();
            //}
            //else
            //{
            //    myLogger.LogInfo($"PlaybackFile: {d.Commands.PlaybackFile}");
            //    Playback(d.Commands.PlaybackFile, playbackSpeed);
            //}
        }

        /// <summary>
        /// 连接到服务器，初始化 gRPC 客户端
        /// </summary>
        /// <param name="comInfo">包含 ip、port、playerID、teamID、shipTypeID 的数组</param>
        public void ConnectToServer(string[] comInfo)
        {
            //if (isPlaybackMode) return;
            //if (Convert.ToInt64(comInfo[2]) > 2023)
            //{
            //    isSpectatorMode = true;
            //    myLogger.LogInfo("isSpectatorMode = true");
            //}
            //if (comInfo.Length != 5)
            //{
            //    throw new Exception("Error Registration Information！");
            //}

            //string connect = $"{comInfo[0]}:{comInfo[1]}";
            //Channel channel = new Channel(connect, ChannelCredentials.Insecure);
            //client = new AvailableService.AvailableServiceClient(channel);
            //PlayerMsg playerMsg = new PlayerMsg();
            //playerID = Convert.ToInt64(comInfo[2]);
            //playerMsg.PlayerId = playerID;
            //if (!isSpectatorMode)
            //{
            //    teamID = Convert.ToInt64(comInfo[3]);
            //    playerMsg.TeamId = teamID;
            //    shipType = Convert.ToInt64(comInfo[4]) switch
            //    {
            //        0 => ShipType.NullShipType,
            //        1 => ShipType.CivilianShip,
            //        2 => ShipType.MilitaryShip,
            //        3 => ShipType.FlagShip,
            //        _ => ShipType.NullShipType
            //    };
            //    playerMsg.ShipType = shipType;
            //}
            //responseStream = client.AddPlayer(playerMsg);
        }

        /// <summary>
        /// 异步接收服务器流消息
        /// </summary>
        private async void OnReceive()
        {
            //try
            //{
            //    myLogger.LogInfo("============= OnReceiving Server Stream ================");
            //    while (responseStream != null && await responseStream.ResponseStream.MoveNext())
            //    {
            //        myLogger.LogInfo("============= Receiving Server Stream ================");
            //        MessageToClient content = responseStream.ResponseStream.Current;
            //        // 根据 game_state 与消息内容，更新游戏状态，这里只给出一个示例
            //        switch (content.game_state)
            //        {
            //            case GameState.GAME_START:
            //                myLogger.LogInfo("Game Start");
            //                // TODO: 处理游戏开始消息
            //                break;
            //            case GameState.GAME_RUNNING:
            //                myLogger.LogInfo("Game Running");
            //                // TODO: 处理游戏运行消息
            //                break;
            //            case GameState.GAME_END:
            //                myLogger.LogInfo("Game End");
            //                // TODO: 处理游戏结束消息
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    myLogger.LogInfo($"OnReceive Exception: {ex.Message}");
            //}
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
        /// 定时器回调方法，用于刷新 UI 与游戏状态
        /// </summary>
        private void Refresh(object? sender, EventArgs e)
        {
            try
            {
                counterViewModelTest++;
                // TODO: 在此处添加更新绘制（例如调用绘图方法、更新数据绑定属性）的逻辑
            }
            catch (Exception ex)
            {
                //myLogger.LogInfo($"Refresh error: {ex.Message}");
            }
        }
    }
}
