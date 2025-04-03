//ViewModeBase.cs
using System;

using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using debug_interface.Models;
using Google.Protobuf;
using Protobuf;
using Grpc.Core;

using installer.Model;
using installer.Data;
using Avalonia.Controls.Shapes;
using Avalonia.Logging;


namespace debug_interface.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        // 用于 UI 刷新的定时器（Avalonia 的 DispatcherTimer）
        private DispatcherTimer timerViewModel;
        private int counterViewModelTest = 0;

        // 使用 CommunityToolkit 的 ObservableProperty 自动实现 INotifyPropertyChanged
        [ObservableProperty]
        private string title;

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
        private AvailableService.AvailableServiceClient? client;
        private AsyncServerStreamingCall<MessageToClient>? responseStream;
        private bool isSpectatorMode = false;
        private bool isPlaybackMode = false;

        // 用于存储上次移动角度（示例，仅作为参考）
        private double lastMoveAngle;

        // 日志记录
        //public Logger myLogger;
        //public Logger lockGenerator;


        public ViewModelBase()
        {
            title = "THUAI8";

            // 读取配置（假设 ConfigData 类来自 installer.Data 命名空间）
            var d = new installer.Data.ConfigData();
            ip = d.Commands.IP;
            port = d.Commands.Port;
            playerID = Convert.ToInt64(d.Commands.PlayerID);
            teamID = Convert.ToInt64(d.Commands.TeamID);
            //shipTypeID = Convert.ToInt32(d.Commands.ShipType);
            string playbackFile = d.Commands.PlaybackFile;
            double playbackSpeed = d.Commands.PlaybackSpeed;

            //初始化日志记录器
            //myLogger = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"Client.{teamID}.{playerID}.log"));
            //lockGenerator = LoggerProvider.FromFile(System.IO.Path.Combine(d.InstallPath, "Logs", $"lock.{teamID}.{playerID}.log"));



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
                    Convert.ToString(0),
                };

                ConnectToServer(comInfo);
                OnReceive();
            }
            else
            {
                //myLogger.LogInfo($"PlaybackFile: {d.Commands.PlaybackFile}");
                //Playback(d.Commands.PlaybackFile, playbackSpeed);
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
                //myLogger.LogInfo("isSpectatorMode = true");
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
                //shipType = Convert.ToInt64(comInfo[4]) switch
                //{
                //    0 => ShipType.NullShipType,
                //    1 => ShipType.CivilianShip,
                //    2 => ShipType.MilitaryShip,
                //    3 => ShipType.FlagShip,
                //    _ => ShipType.NullShipType
                //};
                playerMsg.CharacterType = CharacterType.TangSeng;
            }
            responseStream = client.AddCharacter(playerMsg);
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
