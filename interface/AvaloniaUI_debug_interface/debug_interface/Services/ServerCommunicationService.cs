//using System;
//using System.Threading.Tasks;
//using intaller.Data;
//using installer.Utils;
//using debug_interface.Utils;
//using THUAI8.Proto; // 确保这是你实际的Proto命名空间

//namespace debug_interface.Services
//{
//    public class ServerCommunicationService
//    {
//        private readonly DebugConfig _config;
//        private readonly Logger _logger;
//        private Channel _channel;
//        private DebugInterface.DebugInterfaceClient _client; // 使用你的Proto定义的客户端类
//        private AsyncServerStreamingCall<MessageToClient> _responseStream;
//        private bool _isConnected = false;
//        private bool _isSpectatorMode = false;

//        public ServerCommunicationService(
//            IConfiguration configuration,
//            Logger logger)
//        {
//            _config = configuration.GetSection("DebugConfig").Get<DebugConfig>();
//            _logger = logger;
//        }

//        public bool IsConnected => _isConnected;

//        public async Task ConnectToServer()
//        {
//            try
//            {
//                string ip = _config.Commands.IP;
//                string port = _config.Commands.Port;
//                long playerID = long.Parse(_config.Commands.PlayerID);
//                long teamID = long.Parse(_config.Commands.TeamID);

//                _logger.LogInfo($"正在连接到服务器 {ip}:{port}");
//                _logger.LogInfo($"玩家ID: {playerID}, 队伍ID: {teamID}");

//                if (playerID > 2023)
//                {
//                    _isSpectatorMode = true;
//                    _logger.LogInfo("启用观察者模式");
//                }

//                // 创建连接地址
//                string connect = $"{ip}:{port}";
//                _logger.LogInfo($"创建gRPC通道: {connect}");

//                // 创建Channel
//                _channel = new Channel(connect, ChannelCredentials.Insecure);

//                // 创建客户端
//                _client = new DebugInterface.DebugInterfaceClient(_channel);
//                _logger.LogInfo("创建gRPC客户端成功");

//                // 创建玩家消息
//                PlayerMsg playerMsg = new PlayerMsg();
//                playerMsg.PlayerId = playerID;
//                playerMsg.TeamId = teamID;

//                // 如果不是观察者模式，设置附加信息
//                if (!_isSpectatorMode)
//                {
//                    // 设置Ship类型等信息
//                    int shipTypeID = int.Parse(_config.Commands.ShipType);
//                    ShipType shipType = (ShipType)shipTypeID;
//                    playerMsg.ShipType = shipType;
//                    _logger.LogInfo($"玩家舰船类型: {shipType}");
//                }

//                // 连接到服务器
//                _logger.LogInfo("尝试连接服务器...");
//                _responseStream = _client.AddPlayer(playerMsg);
//                _logger.LogInfo("成功获取服务器响应流");

//                _isConnected = true;
//                _logger.LogInfo("成功连接到服务器!");

//                // 开始接收服务器消息
//                StartReceivingMessages();
//            }
//            catch (RpcException ex)
//            {
//                _isConnected = false;
//                _logger.LogError($"gRPC连接错误: {ex.Status.Detail}");
//                _logger.LogError($"错误状态码: {ex.Status.StatusCode}");
//            }
//            catch (Exception ex)
//            {
//                _isConnected = false;
//                _logger.LogError($"连接失败: {ex.Message}");
//                _logger.LogError($"异常类型: {ex.GetType().FullName}");
//                if (ex.InnerException != null)
//                {
//                    _logger.LogError($"内部异常: {ex.InnerException.Message}");
//                }
//            }
//        }

//        private async void StartReceivingMessages()
//        {
//            try
//            {
//                _logger.LogInfo("开始接收服务器消息...");

//                while (_responseStream != null && await _responseStream.ResponseStream.MoveNext())
//                {
//                    _logger.LogInfo("收到服务器消息");

//                    // 获取当前消息
//                    MessageToClient message = _responseStream.ResponseStream.Current;

//                    // 处理游戏状态
//                    switch (message.GameState)
//                    {
//                        case GameState.GameStart:
//                            _logger.LogInfo("游戏开始");
//                            ProcessGameStart(message);
//                            break;

//                        case GameState.GameRunning:
//                            _logger.LogInfo("游戏运行中");
//                            ProcessGameRunning(message);
//                            break;

//                        case GameState.GameEnd:
//                            _logger.LogInfo("游戏结束");
//                            ProcessGameEnd(message);
//                            break;
//                    }
//                }

//                _logger.LogInfo("服务器消息流结束");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"接收消息时发生错误: {ex.Message}");
//                _isConnected = false;
//            }
//        }

//        private void ProcessGameStart(MessageToClient message)
//        {
//            try
//            {
//                _logger.LogInfo("处理游戏开始消息");

//                // 处理对象消息
//                foreach (var obj in message.ObjMessage)
//                {
//                    ProcessObjectMessage(obj);
//                }

//                // 处理全局消息
//                _logger.LogInfo($"红方能量: {message.AllMessage.RedTeamEnergy}, 蓝方能量: {message.AllMessage.BlueTeamEnergy}");
//                _logger.LogInfo($"红方血量: {message.AllMessage.RedHomeHp}, 蓝方血量: {message.AllMessage.BlueHomeHp}");
//                _logger.LogInfo($"红方分数: {message.AllMessage.RedTeamScore}, 蓝方分数: {message.AllMessage.BlueTeamScore}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理游戏开始消息时发生错误: {ex.Message}");
//            }
//        }

//        private void ProcessGameRunning(MessageToClient message)
//        {
//            try
//            {
//                _logger.LogInfo("处理游戏运行消息");

//                // 处理对象消息
//                foreach (var obj in message.ObjMessage)
//                {
//                    ProcessObjectMessage(obj);
//                }

//                // 处理全局消息
//                _logger.LogInfo($"红方能量: {message.AllMessage.RedTeamEnergy}, 蓝方能量: {message.AllMessage.BlueTeamEnergy}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理游戏运行消息时发生错误: {ex.Message}");
//            }
//        }

//        private void ProcessGameEnd(MessageToClient message)
//        {
//            try
//            {
//                _logger.LogInfo("处理游戏结束消息");

//                // 处理对象消息
//                foreach (var obj in message.ObjMessage)
//                {
//                    ProcessObjectMessage(obj);
//                }

//                // 处理全局消息
//                _logger.LogInfo($"红方最终分数: {message.AllMessage.RedTeamScore}, 蓝方最终分数: {message.AllMessage.BlueTeamScore}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理游戏结束消息时发生错误: {ex.Message}");
//            }
//        }

//        private void ProcessObjectMessage(MessageOfObj obj)
//        {
//            try
//            {
//                switch (obj.MessageOfObjCase)
//                {
//                    case MessageOfObj.MessageOfObjOneofCase.ShipMessage:
//                        _logger.LogInfo($"舰船位置: ({obj.ShipMessage.X}, {obj.ShipMessage.Y})");
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.MapMessage:
//                        _logger.LogInfo("收到地图信息");
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.BulletMessage:
//                        _logger.LogInfo($"子弹位置: ({obj.BulletMessage.X}, {obj.BulletMessage.Y})");
//                        break;

//                        // 可以添加其他消息类型的处理
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理对象消息时发生错误: {ex.Message}");
//            }
//        }

//        public async Task DisconnectFromServer()
//        {
//            if (!_isConnected) return;

//            try
//            {
//                _logger.LogInfo("正在断开服务器连接...");

//                // 关闭响应流
//                _responseStream?.Dispose();

//                // 关闭通道
//                if (_channel != null)
//                {
//                    await _channel.ShutdownAsync();
//                }

//                _isConnected = false;
//                _logger.LogInfo("已断开服务器连接");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"断开连接时发生错误: {ex.Message}");
//            }
//        }

//        public async Task SendCommand(string command)
//        {
//            if (!_isConnected || _client == null)
//            {
//                _logger.LogWarning("无法发送命令：未连接到服务器");
//                return;
//            }

//            try
//            {
//                _logger.LogInfo($"发送命令: {command}");

//                // 这里需要根据实际的Proto定义来实现命令发送
//                // 以下代码仅作为示例
//                if (command.StartsWith("move"))
//                {
//                    string direction = command.Substring(5);
//                    double angle = 0;

//                    switch (direction.ToLower())
//                    {
//                        case "up": angle = Math.PI; break;
//                        case "down": angle = 0; break;
//                        case "left": angle = Math.PI * 3 / 2; break;
//                        case "right": angle = Math.PI / 2; break;
//                    }

//                    MoveMsg moveMsg = new MoveMsg
//                    {
//                        PlayerId = long.Parse(_config.Commands.PlayerID),
//                        TeamId = long.Parse(_config.Commands.TeamID),
//                        Angle = angle,
//                        TimeInMilliseconds = 100
//                    };

//                    await _client.MoveAsync(moveMsg);
//                    _logger.LogInfo("移动命令已发送");
//                }
//                else if (command.StartsWith("attack"))
//                {
//                    AttackMsg attackMsg = new AttackMsg
//                    {
//                        PlayerId = long.Parse(_config.Commands.PlayerID),
//                        TeamId = long.Parse(_config.Commands.TeamID),
//                        Angle = 0 // 或者根据需要设置角度
//                    };

//                    await _client.AttackAsync(attackMsg);
//                    _logger.LogInfo("攻击命令已发送");
//                }
//                // 添加其他命令的处理
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"发送命令时发生错误: {ex.Message}");
//            }
//        }

//        public void CleanUp()
//        {
//            _responseStream?.Dispose();
//            _channel?.Dispose();
//        }
//    }
//}