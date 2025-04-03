using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using debug_interface.ViewModels;
using Grpc.Core;
using installer.Model;
using installer.Data;

namespace debug_interface.Services
{
    public class ServerCommunicationService
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly Logger _logger;
        private readonly ConfigData _config;
        private bool _isConnected = false;
        private bool _isPlaybackMode = false;

        public ServerCommunicationService(MainWindowViewModel viewModel, Logger logger, ConfigData config)
        {
            _viewModel = viewModel;
            _logger = logger;
            _config = config;
        }

        public async Task ConnectToServer()
        {
            try
            {
                string ip = _config.Commands.IP;
                string port = _config.Commands.Port;

                _logger.LogInfo($"尝试连接服务器 {ip}:{port}");

                // 由于没有正确的Proto引用，这里暂时只模拟连接行为
                await Task.Delay(1000); // 模拟连接延迟

                await Dispatcher.UIThread.InvokeAsync(() => {
                    _viewModel.GameLog = $"已连接到服务器 {ip}:{port}";
                });

                _isConnected = true;
                _logger.LogInfo("已模拟连接到服务器");
            }
            catch (Exception ex)
            {
                _logger.LogError($"连接失败: {ex.Message}");

                await Dispatcher.UIThread.InvokeAsync(() => {
                    _viewModel.GameLog = $"连接失败: {ex.Message}";
                });
            }
        }

        public void StartPlaybackMode()
        {
            try
            {
                _isPlaybackMode = true;
                string playbackFile = _config.Commands.PlaybackFile ?? "";

                _logger.LogInfo($"模拟回放: {playbackFile}");

                Dispatcher.UIThread.InvokeAsync(() => {
                    _viewModel.GameLog = $"模拟回放: {playbackFile}";
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"回放错误: {ex.Message}");

                Dispatcher.UIThread.InvokeAsync(() => {
                    _viewModel.GameLog = $"回放错误: {ex.Message}";
                });
            }
        }
    }
}


//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Avalonia.Threading;
//using debug_interface.ViewModels;
//using Grpc.Core;
//using Google.Protobuf; // 假设这是proto文件生成的命名空间
//using installer.Model; // Logger的命名空间
//using installer.Data; // ConfigData的命名空间
//using System.Collections.Generic;
//using Avalonia.Data;
//using System.Threading.Channels;

//namespace debug_interface.Services
//{
//    public class ServerCommunicationService
//    {
//        private readonly MainWindowViewModel _viewModel;
//        private readonly Logger _logger;
//        private readonly ConfigData _config;
//        private AvailableService.AvailableServiceClient? _client;
//        private AsyncServerStreamingCall<MessageToClient>? _responseStream;
//        private bool _isConnected = false;
//        private bool _isPlaybackMode = false;

//        public ServerCommunicationService(MainWindowViewModel viewModel, Logger logger, ConfigData config)
//        {
//            _viewModel = viewModel;
//            _logger = logger;
//            _config = config;
//        }

//        public async Task ConnectToServer()
//        {
//            try
//            {
//                if (_isPlaybackMode)
//                    return;

//                string ip = _config.Commands.IP;
//                string port = _config.Commands.Port;
//                long playerId = _config.Commands.PlayerID;
//                long teamId = _config.Commands.TeamID;
//                int characterType = _config.Commands.CharacterType;

//                string connectionString = $"{ip}:{port}";
//                _logger.LogInfo($"正在连接到服务器 {connectionString}");

//                // 创建gRPC通道和客户端
//                var channel = new Channel(connectionString, ChannelCredentials.Insecure);
//                _client = new AvailableService.AvailableServiceClient(channel);

//                // 创建连接消息
//                var characterMsg = new CharacterMsg
//                {
//                    CharacterId = playerId,
//                    TeamId = teamId,
//                    CharacterType = (CharacterType)characterType
//                };

//                // 连接到服务器
//                _responseStream = _client.AddCharacter(characterMsg);
//                _isConnected = true;

//                // 开始接收消息
//                _ = StartReceivingMessagesAsync();

//                _logger.LogInfo("成功连接到服务器");
//            }
//            catch (Exception ex)
//            {
//                _isConnected = false;
//                _logger.LogError($"连接服务器失败: {ex.Message}");

//                // 在UI中显示错误
//                await Dispatcher.UIThread.InvokeAsync(() => {
//                    _viewModel.GameLog = $"连接服务器错误: {ex.Message}";
//                });

//                throw;
//            }
//        }

//        public void StartPlaybackMode()
//        {
//            _isPlaybackMode = true;
//            string playbackFile = _config.Commands.PlaybackFile ?? "";
//            double playbackSpeed = _config.Commands.PlaybackSpeed;

//            _logger.LogInfo($"正在从{playbackFile}以{playbackSpeed}倍速开始回放模式");

//            // 这里可以实现回放逻辑，读取文件并模拟服务器消息
//            // 例如启动一个单独的任务来读取和处理文件
//            _ = Task.Run(async () => {
//                try
//                {
//                    // 简单的回放逻辑示例
//                    if (System.IO.File.Exists(playbackFile))
//                    {
//                        // 读取文件内容...
//                        _logger.LogInfo("回放文件存在，开始读取数据");

//                        // 这里是简化实现，实际应根据文件格式处理
//                        var fileBytes = System.IO.File.ReadAllBytes(playbackFile);

//                        // 通知UI
//                        await Dispatcher.UIThread.InvokeAsync(() => {
//                            _viewModel.GameLog = "正在回放...";
//                        });
//                    }
//                    else
//                    {
//                        _logger.LogError($"回放文件不存在: {playbackFile}");
//                        await Dispatcher.UIThread.InvokeAsync(() => {
//                            _viewModel.GameLog = "回放文件不存在!";
//                        });
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError($"回放过程中出错: {ex.Message}");
//                    await Dispatcher.UIThread.InvokeAsync(() => {
//                        _viewModel.GameLog = $"回放错误: {ex.Message}";
//                    });
//                }
//            });
//        }

//        private async Task StartReceivingMessagesAsync()
//        {
//            if (_responseStream == null)
//                return;

//            try
//            {
//                _logger.LogInfo("开始接收服务器消息");

//                while (await _responseStream.ResponseStream.MoveNext(default))
//                {
//                    var message = _responseStream.ResponseStream.Current;

//                    // 在UI线程上处理消息，以安全地更新UI
//                    await Dispatcher.UIThread.InvokeAsync(() => {
//                        ProcessServerMessage(message);
//                    });
//                }
//            }
//            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
//            {
//                _logger.LogInfo("服务器流已取消");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"接收服务器消息错误: {ex.Message}");

//                await Dispatcher.UIThread.InvokeAsync(() => {
//                    _viewModel.GameLog = $"连接错误: {ex.Message}";
//                });

//                // 尝试重新连接
//                await TryReconnectAsync();
//            }
//            finally
//            {
//                _isConnected = false;
//            }
//        }

//        private async Task TryReconnectAsync()
//        {
//            _logger.LogInfo("尝试重新连接服务器...");

//            // 等待一段时间后重试
//            await Task.Delay(5000);

//            if (!_isConnected && !_isPlaybackMode)
//            {
//                try
//                {
//                    await ConnectToServer();
//                }
//                catch
//                {
//                    // 重连失败，等待更长时间后再次尝试
//                    await Task.Delay(10000);
//                    _ = TryReconnectAsync();
//                }
//            }
//        }

//        private void ProcessServerMessage(MessageToClient message)
//        {
//            try
//            {
//                // 更新游戏状态
//                switch (message.GameState)
//                {
//                    case GameState.GameStart:
//                        _logger.LogInfo("游戏开始");
//                        _viewModel.GameLog = "游戏开始...";
//                        break;

//                    case GameState.GameRunning:
//                        // 处理运行中的游戏数据
//                        _viewModel.GameLog = "游戏运行中...";
//                        break;

//                    case GameState.GameEnd:
//                        _logger.LogInfo("游戏结束");
//                        _viewModel.GameLog = "游戏结束";
//                        break;
//                }

//                // 更新全局游戏信息
//                if (message.AllMessage != null)
//                {
//                    UpdateGlobalGameInfo(message.AllMessage);
//                }

//                // 处理单个对象消息
//                foreach (var obj in message.ObjMessage)
//                {
//                    ProcessObjectMessage(obj);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理服务器消息错误: {ex.Message}");
//            }
//        }

//        private void UpdateGlobalGameInfo(MessageOfAll allMessage)
//        {
//            try
//            {
//                // 更新游戏时间
//                _viewModel.CurrentTime = FormatGameTime(allMessage.GameTime);

//                // 更新队伍得分
//                _viewModel.RedScore = allMessage.BuddhistsTeamScore;
//                _viewModel.BlueScore = allMessage.MonstersTeamScore;

//                // 更新队伍经济/资源
//                _viewModel.BuddhistTeamEconomy = allMessage.BuddhistsTeamEconomy;
//                _viewModel.MonstersTeamEconomy = allMessage.MonstersTeamEconomy;

//                // 更新英雄HP (如果ViewModel中有对应属性)
//                if (typeof(MainWindowViewModel).GetProperty("BuddhistHeroHp") != null)
//                {
//                    _viewModel.BuddhistHeroHp = allMessage.BuddhistsHeroHp;
//                    _viewModel.MonstersHeroHp = allMessage.MonstersHeroHp;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新全局游戏信息失败: {ex.Message}");
//            }
//        }

//        private void ProcessObjectMessage(MessageOfObj objMessage)
//        {
//            try
//            {
//                switch (objMessage.MessageOfObjCase)
//                {
//                    case MessageOfObj.MessageOfObjOneofCase.CharacterMessage:
//                        UpdateCharacter(objMessage.CharacterMessage);
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.BarracksMessage:
//                        UpdateBuilding(objMessage.BarracksMessage, "兵营");
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.SpringMessage:
//                        UpdateBuilding(objMessage.SpringMessage, "泉水");
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.FarmMessage:
//                        UpdateBuilding(objMessage.FarmMessage, "农场");
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.TrapMessage:
//                        UpdateTrap(objMessage.TrapMessage);
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.EconomyResourceMessage:
//                        UpdateResource(objMessage.EconomyResourceMessage);
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.AdditionResourceMessage:
//                        UpdateAdditionResource(objMessage.AdditionResourceMessage);
//                        break;

//                    case MessageOfObj.MessageOfObjOneofCase.MapMessage:
//                        UpdateMap(objMessage.MapMessage);
//                        break;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"处理对象消息失败: {ex.Message} (类型: {objMessage.MessageOfObjCase})");
//            }
//        }

//        private void UpdateCharacter(MessageOfCharacter character)
//        {
//            try
//            {
//                // 在适当的团队集合中查找角色
//                var isRedTeam = character.TeamId == 1; // 1表示取经队
//                var collection = isRedTeam
//                    ? _viewModel.RedTeamCharacters
//                    : _viewModel.BlueTeamCharacters;

//                // 计算网格坐标
//                int gridX = character.X / 1000;
//                int gridY = character.Y / 1000;

//                // 尝试查找现有角色
//                var existingCharacter = collection.FirstOrDefault(c => c.CharacterId == character.PlayerId);

//                if (existingCharacter != null)
//                {
//                    // 更新现有角色
//                    existingCharacter.Name = GetCharacterTypeName(character.CharacterType);
//                    existingCharacter.Hp = character.Hp;
//                    existingCharacter.PosX = gridX;
//                    existingCharacter.PosY = gridY;

//                    // 更新主动状态
//                    existingCharacter.ActiveState = GetCharacterStateText(character.CharacterActiveState);

//                    // 更新被动状态
//                    existingCharacter.PassiveStates.Clear();

//                    // 根据需要添加被动状态
//                    if (character.BlindState != CharacterState.NullCharacterState)
//                        existingCharacter.PassiveStates.Add("致盲");
//                    if (character.StunnedState != CharacterState.NullCharacterState)
//                        existingCharacter.PassiveStates.Add("定身");
//                    if (character.InvisibleState != CharacterState.NullCharacterState)
//                        existingCharacter.PassiveStates.Add("隐身");
//                    if (character.BurnedState != CharacterState.NullCharacterState)
//                        existingCharacter.PassiveStates.Add("灼烧");

//                    // 更新装备
//                    existingCharacter.EquipmentInventory.Clear();
//                    if (character.ShieldEquipment > 0)
//                        existingCharacter.EquipmentInventory.Add(new EquipmentItem("护盾", character.ShieldEquipment));
//                    if (character.ShoesEquipment > 0)
//                        existingCharacter.EquipmentInventory.Add(new EquipmentItem("鞋子", 1));
//                    if (character.PurificationEquipmentTime > 0)
//                        existingCharacter.EquipmentInventory.Add(new EquipmentItem("净化药水", 1));

//                    // 通知UI更新显示的状态和装备文本
//                    existingCharacter.OnPropertyChanged(nameof(CharacterViewModel.DisplayStates));
//                    existingCharacter.OnPropertyChanged(nameof(CharacterViewModel.DisplayEquipments));
//                }
//                else
//                {
//                    // 创建新角色
//                    var newCharacter = new CharacterViewModel
//                    {
//                        CharacterId = character.PlayerId,
//                        Name = GetCharacterTypeName(character.CharacterType),
//                        Hp = character.Hp,
//                        PosX = gridX,
//                        PosY = gridY,
//                        ActiveState = GetCharacterStateText(character.CharacterActiveState)
//                    };

//                    // 添加被动状态
//                    if (character.BlindState != CharacterState.NullCharacterState)
//                        newCharacter.PassiveStates.Add("致盲");
//                    if (character.StunnedState != CharacterState.NullCharacterState)
//                        newCharacter.PassiveStates.Add("定身");
//                    if (character.InvisibleState != CharacterState.NullCharacterState)
//                        newCharacter.PassiveStates.Add("隐身");
//                    if (character.BurnedState != CharacterState.NullCharacterState)
//                        newCharacter.PassiveStates.Add("灼烧");

//                    // 添加装备
//                    if (character.ShieldEquipment > 0)
//                        newCharacter.EquipmentInventory.Add(new EquipmentItem("护盾", character.ShieldEquipment));
//                    if (character.ShoesEquipment > 0)
//                        newCharacter.EquipmentInventory.Add(new EquipmentItem("鞋子", 1));
//                    if (character.PurificationEquipmentTime > 0)
//                        newCharacter.EquipmentInventory.Add(new EquipmentItem("净化药水", 1));

//                    // 添加到集合
//                    collection.Add(newCharacter);
//                }

//                // 更新地图上的角色位置
//                _viewModel.MapVM.UpdateCharacterPosition(
//                    character.PlayerId,
//                    character.TeamId,
//                    gridX,
//                    gridY,
//                    GetCharacterTypeName(character.CharacterType)
//                );
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新角色失败: {ex.Message}");
//            }
//        }

//        private string GetCharacterTypeName(CharacterType type)
//        {
//            return type switch
//            {
//                CharacterType.TangSeng => "唐僧",
//                CharacterType.SunWukong => "孙悟空",
//                CharacterType.ZhuBajie => "猪八戒",
//                CharacterType.ShaWujing => "沙悟净",
//                CharacterType.BaiLongma => "白龙马",
//                CharacterType.Monkid => "小和尚",
//                CharacterType.JiuLing => "九灵元圣",
//                CharacterType.HongHaier => "红孩儿",
//                CharacterType.NiuMowang => "牛魔王",
//                CharacterType.TieShan => "铁扇公主",
//                CharacterType.ZhiZhujing => "蜘蛛精",
//                CharacterType.Pawn => "小妖",
//                _ => "未知角色"
//            };
//        }

//        private string GetCharacterStateText(CharacterState state)
//        {
//            return state switch
//            {
//                CharacterState.Idle => "空置",
//                CharacterState.Harvesting => "开采",
//                CharacterState.Attacking => "攻击",
//                CharacterState.SkillCasting => "释放技能",
//                CharacterState.Constructing => "建造",
//                CharacterState.Moving => "移动",
//                _ => "未知状态"
//            };
//        }

//        private void UpdateBuilding(MessageOfBarracks building, string buildingType)
//        {
//            try
//            {
//                // 转换为网格坐标
//                int gridX = building.X / 1000;
//                int gridY = building.Y / 1000;

//                // 确保坐标在有效范围内
//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    // 在地图上更新建筑
//                    _viewModel.MapVM.UpdateBuildingCell(
//                        gridX,
//                        gridY,
//                        building.TeamId == 1 ? "取经队" : "妖怪队",
//                        buildingType,
//                        building.Hp
//                    );

//                    // 更新建筑信息文本 (假设ViewModel有这些属性)
//                    string buildingInfo = $"{buildingType} 位置:({gridX},{gridY}) 血量:{building.Hp}";
//                    if (building.TeamId == 1) // 取经队
//                    {
//                        // 尝试更新建筑信息，如果属性存在的话
//                        if (typeof(MainWindowViewModel).GetProperty("SomeBuildingInfo") != null)
//                        {
//                            _viewModel.SomeBuildingInfo = buildingInfo;
//                        }
//                    }
//                    else // 妖怪队
//                    {
//                        if (typeof(MainWindowViewModel).GetProperty("AnotherBuildingInfo") != null)
//                        {
//                            _viewModel.AnotherBuildingInfo = buildingInfo;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新建筑(兵营)失败: {ex.Message}");
//            }
//        }

//        private void UpdateBuilding(MessageOfSpring building, string buildingType)
//        {
//            try
//            {
//                // 类似于UpdateBuilding(MessageOfBarracks...)的实现
//                int gridX = building.X / 1000;
//                int gridY = building.Y / 1000;

//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    _viewModel.MapVM.UpdateBuildingCell(
//                        gridX,
//                        gridY,
//                        building.TeamId == 1 ? "取经队" : "妖怪队",
//                        buildingType,
//                        building.Hp
//                    );

//                    string buildingInfo = $"{buildingType} 位置:({gridX},{gridY}) 血量:{building.Hp}";
//                    if (building.TeamId == 1 &&
//                        typeof(MainWindowViewModel).GetProperty("SomeBuildingInfo") != null)
//                    {
//                        _viewModel.SomeBuildingInfo += "\n" + buildingInfo;
//                    }
//                    else if (building.TeamId != 1 &&
//                            typeof(MainWindowViewModel).GetProperty("AnotherBuildingInfo") != null)
//                    {
//                        _viewModel.AnotherBuildingInfo += "\n" + buildingInfo;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新建筑(泉水)失败: {ex.Message}");
//            }
//        }

//        private void UpdateBuilding(MessageOfFarm building, string buildingType)
//        {
//            try
//            {
//                // 类似于其他建筑更新方法
//                int gridX = building.X / 1000;
//                int gridY = building.Y / 1000;

//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    _viewModel.MapVM.UpdateBuildingCell(
//                        gridX,
//                        gridY,
//                        building.TeamId == 1 ? "取经队" : "妖怪队",
//                        buildingType,
//                        building.Hp
//                    );

//                    string buildingInfo = $"{buildingType} 位置:({gridX},{gridY}) 血量:{building.Hp}";
//                    if (building.TeamId == 1 &&
//                        typeof(MainWindowViewModel).GetProperty("SomeBuildingInfo") != null)
//                    {
//                        _viewModel.SomeBuildingInfo += "\n" + buildingInfo;
//                    }
//                    else if (building.TeamId != 1 &&
//                            typeof(MainWindowViewModel).GetProperty("AnotherBuildingInfo") != null)
//                    {
//                        _viewModel.AnotherBuildingInfo += "\n" + buildingInfo;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新建筑(农场)失败: {ex.Message}");
//            }
//        }

//        private void UpdateTrap(MessageOfTrap trap)
//        {
//            try
//            {
//                int gridX = trap.X / 1000;
//                int gridY = trap.Y / 1000;

//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    string trapTypeName = trap.TrapType switch
//                    {
//                        TrapType.Hole => "陷阱坑",
//                        TrapType.Cage => "牢笼",
//                        _ => "未知陷阱"
//                    };

//                    _viewModel.MapVM.UpdateTrapCell(
//                        gridX,
//                        gridY,
//                        trap.TeamId == 1 ? "取经队" : "妖怪队",
//                        trapTypeName
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新陷阱失败: {ex.Message}");
//            }
//        }

//        private void UpdateResource(MessageOfEconomyResource resource)
//        {
//            try
//            {
//                int gridX = resource.X / 1000;
//                int gridY = resource.Y / 1000;

//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    string resourceTypeName = resource.EconomyResourceType switch
//                    {
//                        EconomyResourceType.SmallEconomyResource => "小经济资源",
//                        EconomyResourceType.MediumEconomyResource => "中经济资源",
//                        EconomyResourceType.LargeEconomyResource => "大经济资源",
//                        _ => "未知资源"
//                    };

//                    _viewModel.MapVM.UpdateResourceCell(
//                        gridX,
//                        gridY,
//                        resourceTypeName,
//                        resource.Process
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新经济资源失败: {ex.Message}");
//            }
//        }

//        private void UpdateAdditionResource(MessageOfAdditionResource resource)
//        {
//            try
//            {
//                int gridX = resource.X / 1000;
//                int gridY = resource.Y / 1000;

//                if (gridX >= 0 && gridX < 50 && gridY >= 0 && gridY < 50)
//                {
//                    string resourceName = GetAdditionResourceName(resource.AdditionResourceType);

//                    _viewModel.MapVM.UpdateAdditionResourceCell(
//                        gridX,
//                        gridY,
//                        resourceName,
//                        resource.Hp
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新附加资源失败: {ex.Message}");
//            }
//        }

//        private string GetAdditionResourceName(AdditionResourceType type)
//        {
//            return type switch
//            {
//                AdditionResourceType.LifePool1 => "生命池(小)",
//                AdditionResourceType.LifePool2 => "生命池(中)",
//                AdditionResourceType.LifePool3 => "生命池(大)",
//                AdditionResourceType.CrazyMan1 => "疯人(小)",
//                AdditionResourceType.CrazyMan2 => "疯人(中)",
//                AdditionResourceType.CrazyMan3 => "疯人(大)",
//                AdditionResourceType.QuickStep => "神行步",
//                AdditionResourceType.WideView => "千里眼",
//                _ => "未知加成资源"
//            };
//        }

//        private void UpdateMap(MessageOfMap map)
//        {
//            try
//            {
//                // 将地图数据转换为二维数组
//                int[,] mapData = new int[50, 50];

//                for (int i = 0; i < map.Rows.Count && i < 50; i++)
//                {
//                    for (int j = 0; j < map.Rows[i].Cols.Count && j < 50; j++)
//                    {
//                        mapData[i, j] = (int)map.Rows[i].Cols[j];
//                    }
//                }

//                // 更新地图视图模型
//                _viewModel.MapVM.UpdateMap(mapData);

//                _logger.LogInfo("地图数据已更新");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"更新地图数据时出错: {ex.Message}");
//            }
//        }

//        private string FormatGameTime(int milliseconds)
//        {
//            TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
//            return $"{time.Minutes:00}:{time.Seconds:00}";
//        }
//    }
//}