using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using installer.Model;
using installer.Data;
using Protobuf;
using Google.Protobuf;
using System.Linq;
using System.Collections.Generic;
using debug_interface.Models;
using Avalonia.Media;


namespace debug_interface.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // 属性定义
        [ObservableProperty]
        private string gameLog = "等待连接...";

        [ObservableProperty]
        private string currentTime = "00:00";

        [ObservableProperty]
        private int redScore = 0;

        [ObservableProperty]
        private int blueScore = 0;

        // --- 用于显示建筑摘要的属性 ---
        [ObservableProperty]
        private string buddhistTeamBuildingInfo = "取经队建筑: 无";

        [ObservableProperty]
        private string monstersTeamBuildingInfo = "妖怪队建筑: 无";

        //[ObservableProperty]
        //private string someBuildingInfo = "";

        //[ObservableProperty]
        //private string anotherBuildingInfo = "";

        [ObservableProperty]
        private int buddhistTeamEconomy = 0;

        [ObservableProperty]
        private int monstersTeamEconomy = 0;

        [ObservableProperty]
        private MapViewModel mapVM;

        // 团队角色集合
        public ObservableCollection<CharacterViewModel> BuddhistsTeamCharacters { get; } = new();
        public ObservableCollection<CharacterViewModel> MonstersTeamCharacters { get; } = new();

        //图例项集合
        public ObservableCollection<LegendItem> MapLegendItems { get; } = new();

        // 默认构造函数
        public MainWindowViewModel() : base() // 调用基类构造函数
        {
            // 初始化MapViewModel
            MapVM = new MapViewModel();

            // 初始化占位角色
            InitializePlaceholderCharacters();

            InitializeMapLegend(); // <--- 调用填充图例的方法


            // 如果在设计模式下，可以添加一些测试数据覆盖占位符
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                InitializeDesignTimeData(); // 设计时数据可以覆盖部分占位符
            }
        }

        // 带参数的构造函数 (如果使用)
        // public MainWindowViewModel(Logger? logger, ConfigData? config) : base() // 调用基类构造函数
        // {
        // if (logger != null)
        //     myLogger = logger;

        // // 初始化MapViewModel
        // MapVM = new MapViewModel();

        // // 初始化占位角色
        // InitializePlaceholderCharacters();

        // // 如果在设计模式下，可以添加一些测试数据
        // if (Avalonia.Controls.Design.IsDesignMode)
        // {
        //     InitializeDesignTimeData();
        // }
        // }

        // 初始化占位符角色
        private void InitializePlaceholderCharacters()
        {
            BuddhistsTeamCharacters.Clear();
            MonstersTeamCharacters.Clear();

            // PlayerID 0-5 为取经， 7-12 为妖怪？ (规则说 PlayerID=0 是核心)
            // 假设ID 0 是核心，1-5 是队员
            // 或者根据 CharacterType 来创建？ 规则里 CharacterType 定义了角色
            // 更好的方法是创建固定数量的占位符，然后用服务器数据填充

            // 取经队 (唐僧 + 5个队员/猴子)
            BuddhistsTeamCharacters.Add(CreatePlaceholderCharacter(0, "唐僧?", Protobuf.CharacterType.TangSeng)); // 核心
            for (int i = 1; i <= 5; i++)
            {
                BuddhistsTeamCharacters.Add(CreatePlaceholderCharacter(i, $"取经队员{i}?", Protobuf.CharacterType.NullCharacterType));
            }


            // 妖怪队 (九头元圣 + 5个队员/小妖)
            MonstersTeamCharacters.Add(CreatePlaceholderCharacter(7, "九头元圣?", Protobuf.CharacterType.JiuLing)); // 核心 (假设ID=7?)
            for (int i = 8; i <= 12; i++) // 假设ID 8-12
            {
                MonstersTeamCharacters.Add(CreatePlaceholderCharacter(i, $"妖怪队员{i}?", Protobuf.CharacterType.NullCharacterType));
            }
        }

        // 创建单个占位符角色的辅助方法
        private CharacterViewModel CreatePlaceholderCharacter(long id, string defaultName, Protobuf.CharacterType type = Protobuf.CharacterType.NullCharacterType)
        {
            return new CharacterViewModel
            {
                CharacterId = id, // 临时ID或标识符
                Name = defaultName,
                Hp = 0,
                PosX = -1, // 初始位置无效
                PosY = -1,
                ActiveState = "未知",
                // PassiveStates 和 EquipmentInventory 默认为空
            };
        }




        // 设计时数据
        private void InitializeDesignTimeData()
        {
            GameLog = "设计模式 - 模拟数据";
            CurrentTime = "12:34";
            RedScore = 50;
            BlueScore = 30;
            BuddhistTeamEconomy = 6000;
            MonstersTeamEconomy = 10000;

            // 更新部分占位符作为示例
            if (BuddhistsTeamCharacters.Count > 0)
            {
                var ts = BuddhistsTeamCharacters[0]; // 唐僧
                ts.Name = "唐僧";
                ts.Hp = 1000; // 假设最大血量
                ts.ActiveState = "空置";
                ts.PosX = 5; ts.PosY = 5; // 示例位置
            }
            if (BuddhistsTeamCharacters.Count > 1)
            {
                var swk = BuddhistsTeamCharacters[1];
                swk.Name = "孙悟空";
                swk.Hp = 200;
                swk.ActiveState = "移动";
                swk.PosX = 10; swk.PosY = 10;
            }
            if (MonstersTeamCharacters.Count > 0)
            {
                var jls = MonstersTeamCharacters[0]; // 九头
                jls.Name = "九头元圣";
                jls.Hp = 1000;
                jls.ActiveState = "攻击";
                jls.PosX = 40; jls.PosY = 40;
            }
            // ... 可以添加更多设计时数据
        }

        //private void InitializeDesignTimeData()
        //{
        //    GameLog = "设计模式 - 模拟数据";
        //    CurrentTime = "12:34";
        //    RedScore = 50;
        //    BlueScore = 30;

        //    // 添加一些测试角色
        //    for (int i = 0; i < 3; i++)
        //    {
        //        BuddhistsTeamCharacters.Add(new CharacterViewModel
        //        {
        //            CharacterId = i + 1,
        //            Name = $"取经队角色{i + 1}",
        //            Hp = 1000,
        //            ActiveState = "空置"
        //        });

        //        MonstersTeamCharacters.Add(new CharacterViewModel
        //        {
        //            CharacterId = i + 101,
        //            Name = $"妖怪队角色{i + 1}",
        //            Hp = 1200,
        //            ActiveState = "移动"
        //        });
        //    }
        //}

        // 定时器更新方法
        //protected override void OnTimerTick(object? sender, EventArgs e)
        //{
        //    // 更新当前时间显示
        //    CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        //}
        // // 更新当前时间显示 - 这个逻辑应该在 UpdateGameStatus 里基于服务器时间更新
        // // CurrentTime = DateTime.Now.ToString("HH:mm:ss"); // 不再需要

        public void UpdateCharacters()
        {
            // 记录已更新的角色ID，以便处理未在消息中出现的角色（可能死亡或离开视野）
            var updatedBuddhistIds = new HashSet<long>();
            var updatedMonsterIds = new HashSet<long>();

            lock (drawPicLock) // 确保线程安全
            {
                foreach (var data in listOfCharacters) // listOfCharacters 来自 ViewModelBase
                {
                    CharacterViewModel? targetCharacter = null;
                    ObservableCollection<CharacterViewModel>? targetList = null;

                    // 根据 TeamId 选择列表并查找角色
                    if (data.TeamId == (int)PlayerTeam.BuddhistsTeam)
                    {
                        targetList = BuddhistsTeamCharacters;
                        targetCharacter = targetList.FirstOrDefault(c => c.CharacterId == data.PlayerId);
                        updatedBuddhistIds.Add(data.PlayerId);
                    }
                    else if (data.TeamId == (int)PlayerTeam.MonstersTeam)
                    {
                        targetList = MonstersTeamCharacters;
                        targetCharacter = targetList.FirstOrDefault(c => c.CharacterId == data.PlayerId);
                        updatedMonsterIds.Add(data.PlayerId);
                    }

                    // 如果找到了对应的角色ViewModel (应该总能找到，因为有占位符)
                    if (targetCharacter != null)
                    {
                        // 更新角色信息
                        targetCharacter.Name = GetCharacterName(data.CharacterType); // 更新名字
                        targetCharacter.Hp = data.Hp;
                        targetCharacter.PosX = data.X / 1000; // 转换为网格坐标 X
                        targetCharacter.PosY = data.Y / 1000; // 转换为网格坐标 Y
                        targetCharacter.ActiveState = data.CharacterActiveState.ToString(); // 主动状态

                        // 清空并更新被动状态
                        targetCharacter.PassiveStates.Clear();
                        if (data.BlindState != CharacterState.NullCharacterState)
                            targetCharacter.PassiveStates.Add($"致盲({data.BlindTime}ms)"); // 显示时间
                        if (data.StunnedState != CharacterState.NullCharacterState)
                            targetCharacter.PassiveStates.Add($"眩晕({data.StunnedTime}ms)");
                        if (data.InvisibleState != CharacterState.NullCharacterState)
                            targetCharacter.PassiveStates.Add($"隐身({data.InvisibleTime}ms)");
                        if (data.BurnedState != CharacterState.NullCharacterState)
                            targetCharacter.PassiveStates.Add($"燃烧({data.BurnedTime}ms)");
                        // 添加其他状态，如击退、定身、死亡等
                        if (data.CharacterPassiveState == CharacterState.KnockedBack) // 使用 CharacterPassiveState
                            targetCharacter.PassiveStates.Add("击退"); // 击退通常是瞬时的，没有时间
                        if (data.CharacterPassiveState == CharacterState.Stunned) // 确认 Stunned 是用 StunnedState 还是 PassiveState
                            targetCharacter.PassiveStates.Add($"定身({data.StunnedTime}ms)"); // 假设 Stunned 对应定身
                        if (data.DeceasedState != CharacterState.NullCharacterState) // 死亡状态
                            targetCharacter.PassiveStates.Add("已死亡");


                        // 清空并更新装备

                        targetCharacter.EquipmentInventory.Clear();
                        if (data.ShieldEquipment > 0) // 护盾显示剩余值
                            targetCharacter.EquipmentInventory.Add(new EquipmentItem("护盾", data.ShieldEquipment));
                        if (data.ShoesEquipment > 0) // 鞋子是状态，显示剩余时间
                            targetCharacter.PassiveStates.Add($"鞋子({~(data.ShoesEquipmentTime / 1000)}s)"); // 显示秒
                        // 其他装备如净化、隐身、狂暴是 Buff 或一次性效果，看是否需要在装备栏显示
                        if (data.PurificationEquipmentTime > 0)
                            targetCharacter.PassiveStates.Add($"净化({~(data.PurificationEquipmentTime / 1000)}s)"); // 显示秒
                        if (data.AttackBuffTime > 0)
                            targetCharacter.PassiveStates.Add($"攻击Buff({~(data.AttackBuffTime / 1000)}s)");
                        if (data.SpeedBuffTime > 0)
                            targetCharacter.PassiveStates.Add($"移速Buff({~(data.SpeedBuffTime / 1000)}s)");
                        if (data.VisionBuffTime > 0)
                            targetCharacter.PassiveStates.Add($"视野Buff({~(data.VisionBuffTime / 1000)}s)");


                        // 触发 PropertyChanged 以更新UI绑定 (虽然 ObservableObject 会自动做，但显式调用一下没坏处)(不行嘻嘻)
                        //targetCharacter.OnPropertyChanged(nameof(CharacterViewModel.DisplayStates));
                        //targetCharacter.OnPropertyChanged(nameof(CharacterViewModel.DisplayEquipments));
                    }
                    // else
                    // {
                    //     // 如果没找到，可能是新的 PlayerID 或逻辑错误，可以记录日志
                    //     myLogger?.LogWarning($"未找到 PlayerID {data.PlayerId} (Team {data.TeamId}) 的占位符 ViewModel。");
                    // }
                }

                // 处理未在消息中出现的角色（可能死亡、离开视野或未出生）
                // 我们可以选择将他们标记为“未知”或“死亡”（如果之前是活的）
                ResetUnseenCharacters(BuddhistsTeamCharacters, updatedBuddhistIds);
                ResetUnseenCharacters(MonstersTeamCharacters, updatedMonsterIds);
            }
        }

        // 重置未出现在当前帧消息中的角色状态
        private void ResetUnseenCharacters(ObservableCollection<CharacterViewModel> teamList, HashSet<long> seenIds)
        {
            foreach (var character in teamList)
            {
                if (!seenIds.Contains(character.CharacterId))
                {
                    // 如果角色之前是“活”的（例如有HP，不在地图外），现在可能死亡或离开视野
                    // if (character.Hp > 0 && character.PosX >= 0)
                    // {
                    //     // 这里可以根据游戏逻辑判断是标记为“未知”、“离开视野”还是保留最后状态
                    //     // 简单起见，我们先重置部分状态，或者标记为未知
                    //     character.ActiveState = "未知/离线";
                    //     character.Hp = 0; // 或者保持最后血量？
                    //     character.PosX = -1; // 移出地图
                    //     character.PosY = -1;
                    //     character.PassiveStates.Clear();
                    //     character.EquipmentInventory.Clear();
                    // }
                    // 或者，如果角色本来就是占位符，则保持占位符状态
                    if (character.Name.EndsWith("?")) // 检查是否是初始占位符
                    {
                        // 保持占位符状态不变
                    }
                    else // 如果是之前更新过的角色，现在看不到了
                    {
                        // 决定如何处理，例如标记为死亡或未知
                        if (!character.PassiveStates.Contains("已死亡")) // 如果之前没死
                        {
                            character.ActiveState = "视野丢失/死亡?";
                            // character.PosX = -1; // 不再地图上显示
                            // character.PosY = -1;
                            // 可以不清空血量，显示最后状态
                        }
                    }
                }
            }
        }

        //不再需要这个方法，更新逻辑合并到 UpdateCharacters
        //private void UpdateOrAddCharacter(ObservableCollection<CharacterViewModel> list, CharacterViewModel newCharacter)
        //{
        //    // 尝试找到现有角色并更新
        //    var existing = list.FirstOrDefault(c => c.CharacterId == newCharacter.CharacterId);
        //    if (existing != null)
        //    {
        //        existing.Hp = newCharacter.Hp;
        //        existing.PosX = newCharacter.PosX;
        //        existing.PosY = newCharacter.PosY;
        //        existing.ActiveState = newCharacter.ActiveState;
        //        existing.PassiveStates.Clear();
        //        foreach (var state in newCharacter.PassiveStates)
        //            existing.PassiveStates.Add(state);
        //        existing.EquipmentInventory.Clear();
        //        foreach (var equip in newCharacter.EquipmentInventory)
        //            existing.EquipmentInventory.Add(equip);
        //    }
        //    else
        //    {
        //        // 添加新角色
        //        list.Add(newCharacter);
        //    }
        //}


        // 获取角色名称的辅助方法 (根据 Proto MessageType.proto)
        private string GetCharacterName(CharacterType type)
        {
            return type switch
            {
                CharacterType.TangSeng => "唐僧",
                CharacterType.SunWukong => "孙悟空",
                CharacterType.ZhuBajie => "猪八戒",
                CharacterType.ShaWujing => "沙悟净",
                CharacterType.BaiLongma => "白龙马",
                CharacterType.Monkid => "猴子猴孙", // 
                CharacterType.JiuLing => "九头元圣", // 
                CharacterType.HongHaier => "红孩儿",
                CharacterType.NiuMowang => "牛魔王",
                CharacterType.TieShan => "铁扇公主", // 
                CharacterType.ZhiZhujing => "蜘蛛精",
                CharacterType.Pawn => "无名小妖",   // 
                CharacterType.NullCharacterType => "未知类型",
                _ => $"未知 ({type})" // 处理枚举未定义的值
            };
        }

        //private string GetCharacterName(CharacterType type)
        //{
        //    return type switch
        //    {
        //        CharacterType.TangSeng => "唐僧",
        //        CharacterType.SunWukong => "孙悟空",
        //        CharacterType.ZhuBajie => "猪八戒",
        //        CharacterType.ShaWujing => "沙悟净",
        //        CharacterType.BaiLongma => "白龙马",
        //        CharacterType.Monkid => "小僧",
        //        CharacterType.JiuLing => "九灵",
        //        CharacterType.HongHaier => "红孩儿",
        //        CharacterType.NiuMowang => "牛魔王",
        //        CharacterType.TieShan => "铁扇",
        //        CharacterType.ZhiZhujing => "蜘蛛精",
        //        CharacterType.Pawn => "小妖",
        //        _ => "未知角色"
        //    };
        //}


        // 地图元素更新方法
        public void UpdateMapElements()
        {
            // 先清除地图上旧的角色标记 (如果 MapViewModel 没有自动处理)
            // MapVM.ClearCharacterDisplay(); // 需要在 MapViewModel 实现此方法

            lock (drawPicLock)
            {
                // 更新地图地形 (如果需要，基于 MapMessage)
                // MapVM.UpdateMap(receivedMapMessage); // 假设 receivedMapMessage 在某处获得

                // 清除动态元素的旧状态 (例如，一个格子之前是建筑，现在不是了)
                // 最好在 MapViewModel 中处理：在更新前重置所有动态格子的状态为基础地形

                // 更新兵营
                foreach (var barracks in listOfBarracks)
                {
                    MapVM.UpdateBuildingCell(
                        barracks.X / 1000,
                        barracks.Y / 1000,
                        barracks.TeamId == (int)PlayerTeam.BuddhistsTeam ? "取经队" : "妖怪队",
                        "兵营",
                        barracks.Hp
                    );
                }

                // 更新春泉
                foreach (var spring in listOfSprings)
                {
                    MapVM.UpdateBuildingCell(
                        spring.X / 1000,
                        spring.Y / 1000,
                        spring.TeamId == (int)PlayerTeam.BuddhistsTeam ? "取经队" : "妖怪队",
                        "泉水",
                        spring.Hp
                    );
                }

                // 更新农场
                foreach (var farm in listOfFarms)
                {
                    MapVM.UpdateBuildingCell(
                        farm.X / 1000,
                        farm.Y / 1000,
                        farm.TeamId == (int)PlayerTeam.BuddhistsTeam ? "取经队" : "妖怪队",
                        "农场",
                        farm.Hp
                    );
                }

                // 更新陷阱
                foreach (var trap in listOfTraps)
                {
                    MapVM.UpdateTrapCell(
                        trap.X / 1000,
                        trap.Y / 1000,
                        trap.TeamId == (int)PlayerTeam.BuddhistsTeam ? "取经队" : "妖怪队",
                        trap.TrapType == TrapType.Hole ? "陷阱(坑洞)" : "陷阱(牢笼)" // 区分类型
                    );
                }

                // 更新经济资源
                foreach (var resource in listOfEconomyResources)
                {
                    MapVM.UpdateResourceCell(
                        resource.X / 1000,
                        resource.Y / 1000,
                        GetEconomyResourceType(resource.EconomyResourceType),
                        resource.Process // 传入剩余量
                    );
                }

                // 更新加成资源
                foreach (var resource in listOfAdditionResources)
                {
                    MapVM.UpdateAdditionResourceCell(
                        resource.X / 1000,
                        resource.Y / 1000,
                        GetAdditionResourceType(resource.AdditionResourceType),
                        resource.Hp // 传入Boss血量
                    );
                }

                // 更新地图上的角色位置标记 (现在由 MapView 直接处理)
                // MapVM.UpdateCharacterPositions(BuddhistsTeamCharacters, MonstersTeamCharacters); // 不再需要 MapViewModel 处理这个
            }
        }

        // 获取经济资源类型名称 (根据 Proto)
        private string GetEconomyResourceType(EconomyResourceType type)
        {
            // 规则中没有区分大小，但 Proto 里有
            return type switch
            {
                EconomyResourceType.SmallEconomyResource => "经济资源(小)",
                EconomyResourceType.MediumEconomyResource => "经济资源(中)",
                EconomyResourceType.LargeEconomyResource => "经济资源(大)",
                _ => "经济资源(未知)"
            };
        }

        // 获取加成资源类型名称
        private string GetAdditionResourceType(AdditionResourceType type)
        {
            return type switch
            {
                AdditionResourceType.LifePool1 => "生命之泉(1)",
                AdditionResourceType.LifePool2 => "生命之泉(2)",
                AdditionResourceType.LifePool3 => "生命之泉(3)",
                AdditionResourceType.CrazyMan1 => "狂战士之力(1)",
                AdditionResourceType.CrazyMan2 => "狂战士之力(2)",
                AdditionResourceType.CrazyMan3 => "狂战士之力(3)",
                AdditionResourceType.QuickStep => "疾步之灵",
                AdditionResourceType.WideView => "视野之灵",
                _ => "加成资源(未知)"
            };
        }

        public void UpdateGameStatus()
        {
            // 清空旧的建筑信息，准备重新生成
            BuddhistTeamBuildingInfo = "取经队建筑: ";
            MonstersTeamBuildingInfo = "妖怪队建筑: ";

            lock (drawPicLock) // 确保访问列表时线程安全
            {
                if (listOfAll.Count > 0)
                {
                    var data = listOfAll[0]; // 全局状态信息
                    CurrentTime = FormatGameTime(data.GameTime); // 使用服务器时间
                    RedScore = data.BuddhistsTeamScore;
                    BlueScore = data.MonstersTeamScore;
                    BuddhistTeamEconomy = data.BuddhistsTeamEconomy;
                    MonstersTeamEconomy = data.MonstersTeamEconomy;

                    // 更新建筑摘要信息
                    UpdateBuildingSummary(); // 调用新的方法生成摘要

                    // // 移除旧的、不准确的建筑信息显示
                    // SomeBuildingInfo = $"取经队经济: {data.BuddhistsTeamEconomy}, 英雄血量: {data.BuddhistsHeroHp}"; // 这个信息不准确，唐僧没有HP字段
                    // AnotherBuildingInfo = $"妖怪队经济: {data.MonstersTeamEconomy}, 英雄血量: {data.MonstersHeroHp}"; // 九头也没有
                }
                else
                {
                    // 如果没有全局信息，可能需要显示默认值或“等待数据”
                    CurrentTime = "00:00";
                    RedScore = 0;
                    BlueScore = 0;
                    BuddhistTeamEconomy = 0;
                    MonstersTeamEconomy = 0;
                    BuddhistTeamBuildingInfo += "无";
                    MonstersTeamBuildingInfo += "无";
                }
            }
        }

        // 格式化游戏时间 (毫秒 -> mm:ss)
        private string FormatGameTime(int gameTimeInMilliseconds)
        {
            // ... (保持不变)
            int totalSeconds = gameTimeInMilliseconds / 1000;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        // 辅助方法：根据建筑类型获取最大HP(根据游戏规则文档)
        public int GetBuildingMaxHp(string buildingType)
        {
            return buildingType switch
            {
                "兵营" => 600,
                "泉水" => 300,
                "农场" => 400,
                _ => 0 // 其他或未知类型
            };
        }

        // 更新建筑摘要信息的方法
        private void UpdateBuildingSummary()
        {
            // 使用 Linq 对建筑列表进行分组和计数
            var buddhistBuildings = listOfBarracks.Where(b => b.TeamId == (int)PlayerTeam.BuddhistsTeam).Select(b => $"兵营({b.Hp}/{GetBuildingMaxHp("兵营")})")
                .Concat(listOfSprings.Where(s => s.TeamId == (int)PlayerTeam.BuddhistsTeam).Select(s => $"泉水({s.Hp}/{GetBuildingMaxHp("泉水")})"))
                .Concat(listOfFarms.Where(f => f.TeamId == (int)PlayerTeam.BuddhistsTeam).Select(f => $"农场({f.Hp}/{GetBuildingMaxHp("农场")})"));

            var monsterBuildings = listOfBarracks.Where(b => b.TeamId == (int)PlayerTeam.MonstersTeam).Select(b => $"兵营({b.Hp}/{GetBuildingMaxHp("兵营")})")
                .Concat(listOfSprings.Where(s => s.TeamId == (int)PlayerTeam.MonstersTeam).Select(s => $"泉水({s.Hp}/{GetBuildingMaxHp("泉水")})"))
                .Concat(listOfFarms.Where(f => f.TeamId == (int)PlayerTeam.MonstersTeam).Select(f => $"农场({f.Hp}/{GetBuildingMaxHp("农场")})"));

            BuddhistTeamBuildingInfo = "取经队建筑: " + (buddhistBuildings.Any() ? string.Join(", ", buddhistBuildings) : "无");
            MonstersTeamBuildingInfo = "妖怪队建筑: " + (monsterBuildings.Any() ? string.Join(", ", monsterBuildings) : "无");

            // 可以在这里加入陷阱的统计信息（如果需要）
            // var buddhistTraps = listOfTraps.Count(t => t.TeamId == (int)PlayerTeam.BuddhistsTeam);
            // var monsterTraps = listOfTraps.Count(t => t.TeamId == (int)PlayerTeam.MonstersTeam);
            // BuddhistTeamBuildingInfo += $", 陷阱: {buddhistTraps}";
            // MonstersTeamBuildingInfo += $", 陷阱: {monsterTraps}";
        }

        // 填充图例数据的方法
        private void InitializeMapLegend()
        {
            MapLegendItems.Clear(); // 清空旧数据

            // 添加基础地形
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.Cyan), "家园"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.White), "空地", Brushes.LightGray, 1)); // 白色加边框
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.DarkGray), "障碍物"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.LightGreen), "草丛"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.LightGray), "未知区域", Brushes.DimGray, 1));

            // 添加建筑 (队伍)
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.DarkRed), "取经队建筑"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.DarkBlue), "妖怪队建筑"));

            // 添加陷阱 (队伍)
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.IndianRed), "取经队陷阱"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.CornflowerBlue), "妖怪队陷阱"));

            // 添加经济资源
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.Gold), "经济资源"));

            // 添加加成资源 (根据 MapViewModel 中的颜色)
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.LightPink), "加成 (生命泉)"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.OrangeRed), "加成 (狂战士)"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.LightSkyBlue), "加成 (疾步灵)"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.MediumPurple), "加成 (视野灵)"));
            MapLegendItems.Add(new LegendItem(new SolidColorBrush(Colors.Purple), "加成 (未知)"));

            // 根据需要添加更多条目
        }

    }
}