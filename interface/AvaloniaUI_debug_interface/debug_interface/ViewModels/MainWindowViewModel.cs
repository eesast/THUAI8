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
using Avalonia;
using Avalonia.Threading;


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


        [ObservableProperty]
        private int buddhistTeamEconomy = 0;

        [ObservableProperty]
        private int monstersTeamEconomy = 0;

        [ObservableProperty]
        private MapViewModel mapVM;
        [ObservableProperty]
        private LogConsoleViewModel logConsoleVM;

        // 团队角色集合
        public ObservableCollection<CharacterViewModel> BuddhistsTeamCharacters { get; } = new(); // 取经队 TeamID = 0
        public ObservableCollection<CharacterViewModel> MonstersTeamCharacters { get; } = new();  // 妖怪队 TeamID = 1

        //图例项集合
        public ObservableCollection<LegendItem> MapLegendItems { get; } = new();

        // 默认构造函数
        public MainWindowViewModel() : base() // 调用基类构造函数
        {
            MapVM = new MapViewModel();
            InitializeMapLegend(); // 初始化图例
            LogConsoleVM = new LogConsoleViewModel();

            // 设计模式数据 (如果需要，可以手动添加几个用于预览)
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                // 可以添加一些设计时数据到集合中，如果需要预览效果
                BuddhistsTeamCharacters.Add(new CharacterViewModel { Guid = -1, Name = "唐僧(设计)", Hp = 1000, MaxHp = 1000, PosX = 5000, PosY = 5000, TeamId = 0 });
                MonstersTeamCharacters.Add(new CharacterViewModel { Guid = -7, Name = "九头(设计)", Hp = 1000, MaxHp = 1000, PosX = 45000, PosY = 45000, TeamId = 1 });
            }
        }



        // 设计数据
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

        public void UpdateCharacters()
        {
            var currentFrameGuids = new HashSet<long>(); // 存储本帧出现的所有角色 Guid

            lock (drawPicLock) // 确保线程安全
            {
                // myLogger?.LogDebug($"开始更新角色视图模型，服务器原始数据数量: {listOfCharacters.Count}");

                // 1. 更新或添加角色
                foreach (var data in listOfCharacters) // listOfCharacters 来自 ViewModelBase
                {
                    currentFrameGuids.Add(data.Guid); // 记录本帧出现的 Guid

                    ObservableCollection<CharacterViewModel>? targetList = null;
                    CharacterViewModel? existingCharacter = null;

                    if (data.TeamId == 0) // Team 0 = 取经队 (Buddhists)
                    {
                        targetList = BuddhistsTeamCharacters;
                    }
                    else if (data.TeamId == 1) // Team 1 = 妖怪队 (Monsters)
                    {
                        targetList = MonstersTeamCharacters;
                    }
                    else
                    {
                        myLogger?.LogWarning($"收到未知 TeamID: {data.TeamId} 的角色消息 (Guid: {data.Guid})");
                        continue; // 跳过未知队伍
                    }

                    // 在目标列表中查找现有角色
                    existingCharacter = targetList.FirstOrDefault(c => c.Guid == data.Guid);

                    if (existingCharacter == null) // 处理添加新角色
                    {
                        var newCharacter = new CharacterViewModel();
                        newCharacter.Guid = data.Guid;
                        UpdateCharacterViewModel(newCharacter, data); // 使用辅助方法填充数据

                        myLogger?.LogInfo($"添加新角色到列表: Guid={newCharacter.Guid}, Name='{newCharacter.Name}', TeamId={newCharacter.TeamId} ({(data.TeamId == 0 ? "Buddhists" : "Monsters")})"); // 添加队伍名日志

                        Dispatcher.UIThread.InvokeAsync(() => targetList.Add(newCharacter));
                    }
                    else // 更新现有角色
                    {
                        UpdateCharacterViewModel(existingCharacter, data);
                    }
                }

                // 2. 处理消失的角色 (从两个列表中移除)
                RemoveUnseenCharacters(BuddhistsTeamCharacters, currentFrameGuids);
                RemoveUnseenCharacters(MonstersTeamCharacters, currentFrameGuids);

                // myLogger?.LogDebug("角色视图模型更新循环结束。");
            } // lock 结束
        }

        private void UpdateCharacterViewModel(CharacterViewModel vm, MessageOfCharacter data)
        {

            vm.CharacterId = data.PlayerId; // 可以保留 PlayerId
            vm.TeamId = data.TeamId;
            vm.CharacterType = data.CharacterType;
            vm.Name = GetCharacterName(data.CharacterType);
            //vm.MaxHp = GetCharacterMaxHp(data.CharacterType);
            //vm.MaxHp = 2000; // 假设最大血量
            vm.Hp = data.Hp-390;
            vm.PosX = data.X; // 存储原始 X
            vm.PosY = data.Y; // 存储原始 Y
            vm.PosY = data.Y; // 存储原始 Y


            CharacterState previousActiveState = vm.ActiveState == "空闲/未知"
                ? CharacterState.NullCharacterState
                : Enum.TryParse<CharacterState>(vm.ActiveState, out var state) ? state : CharacterState.NullCharacterState;
            //vm.ActiveState = data.CharacterActiveState.ToString();


            // 更新主动状态
            if (data.CharacterActiveState == CharacterState.NullCharacterState)
            {
                vm.ActiveState = "空闲/未知"; // 或者更合适的描述
            }
            else
            {
                vm.ActiveState = data.CharacterActiveState.ToString();
            }

            if (data.CharacterActiveState == CharacterState.SkillCasting && previousActiveState != CharacterState.SkillCasting)
            {
                string logMessage = $"{vm.Name} (Guid: {vm.Guid}) 在 ({vm.PosX / 1000},{vm.PosY / 1000}) 释放了技能";
                // Console.WriteLine($"[技能释放] {logMessage}"); // 可以保留或移除 Console 输出
                //myLogger?.LogInfo($"[技能释放] {logMessage}"); // 记录到文件
                LogConsoleVM.AddLog(logMessage, "SKILL"); // *** 添加到界面 Console ***
            }


            vm.StatusEffects.Clear();

            // 更新被动状态 (省略重复代码)
            if (data.IsBlind && data.BlindTime < long.MaxValue) vm.StatusEffects.Add($"致盲 ({data.BlindTime}ms)");
            if (data.IsStunned && data.StunnedTime < long.MaxValue) vm.StatusEffects.Add($"眩晕 ({data.StunnedTime}ms)"); // 合并显示
            if (data.IsInvisible && data.InvisibleTime < long.MaxValue) vm.StatusEffects.Add($"隐身 ({data.InvisibleTime}ms)");
            if (data.IsBurned && data.BurnedTime < long.MaxValue) vm.StatusEffects.Add($"燃烧 ({data.BurnedTime}ms)");


            if (data.CharacterPassiveState == CharacterState.KnockedBack) vm.StatusEffects.Add("被击退"); // 更明确的描述

            // **死亡状态**
            if (vm.Hp <= 0)
            {
                vm.StatusEffects.Add("已死亡");
            }

            // **护盾**
            if (data.ShieldEquipment > 0)
            {
                vm.StatusEffects.Add($"护盾剩余: {data.ShieldEquipment}");
            }


            // **装备/Buff 效果 (带时间)**
            if (data.ShoesTime > 0 && data.ShoesTime < long.MaxValue) vm.StatusEffects.Add($"加速 ({data.ShoesTime / 1000}s)");
            if (data.IsPurified && data.PurifiedTime < long.MaxValue) vm.StatusEffects.Add($"净化效果 ({data.PurifiedTime / 1000}s)");
            if (data.IsBerserk && data.BerserkTime < long.MaxValue) vm.StatusEffects.Add($"狂暴效果 ({data.BerserkTime / 1000}s)");
            if (data.AttackBuffTime > 0 && data.AttackBuffTime < long.MaxValue) vm.StatusEffects.Add($"攻击Buff({data.AttackBuffNum}) ({data.AttackBuffTime / 1000}s)");
            if (data.SpeedBuffTime > 0 && data.SpeedBuffTime < long.MaxValue) vm.StatusEffects.Add($"移速Buff ({data.SpeedBuffTime / 1000}s)");
            if (data.VisionBuffTime > 0 && data.VisionBuffTime < long.MaxValue) vm.StatusEffects.Add($"视野Buff ({data.VisionBuffTime / 1000}s)");

            vm.EquipmentInventory.Clear();
        }


        // 重置未出现在当前帧消息中的角色状态
        private void RemoveUnseenCharacters(ObservableCollection<CharacterViewModel> characterList, HashSet<long> seenGuids)
        {
            // 使用 ToList() 创建副本进行迭代，因为不能在迭代时修改集合
            var charactersToRemove = characterList.Where(c => !seenGuids.Contains(c.Guid)).ToList();

            if (charactersToRemove.Any())
            {
                // *** 确保在 UI 线程上移除 ***
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var character in charactersToRemove)
                    {
                        myLogger?.LogInfo($"移除消失的角色: Guid={character.Guid}, Name='{character.Name}'");
                        characterList.Remove(character);
                        // MapView 中的 CollectionChanged 事件会处理 UI 元素的移除
                    }
                });
            }
        }


        // 获取角色名称的辅助方法 (根据 Proto MessageType.proto)
        private string GetCharacterName(CharacterType type)
        {
            return type switch
            {
                CharacterType.TangSeng => "唐僧111",
                CharacterType.SunWukong => "孙悟空",
                CharacterType.ZhuBajie => "猪八戒",
                CharacterType.ShaWujing => "沙悟净",
                CharacterType.BaiLongma => "白龙马",
                CharacterType.Monkid => "猴子猴孙", // 
                CharacterType.JiuLing => "九灵元圣", // 
                CharacterType.HongHaier => "红孩儿",
                CharacterType.NiuMowang => "牛魔王",
                CharacterType.TieShan => "铁扇公主", // 
                CharacterType.ZhiZhujing => "蜘蛛精",
                CharacterType.Pawn => "无名小妖",   // 
                CharacterType.NullCharacterType => "未知类型",
                _ => $"未知 ({type})" // 处理枚举未定义的值
            };
        }

        private int GetCharacterMaxHp(CharacterType type)
        {
            return type switch
            {
                CharacterType.TangSeng => 1000,
                CharacterType.SunWukong => 200,
                CharacterType.ZhuBajie => 300,
                CharacterType.ShaWujing => 150,
                CharacterType.BaiLongma => 150,
                CharacterType.Monkid => 50, // 猴子猴孙
                CharacterType.JiuLing => 1000, // 九头元圣
                CharacterType.HongHaier => 200,
                CharacterType.NiuMowang => 300,
                CharacterType.TieShan => 150, // 铁扇公主
                CharacterType.ZhiZhujing => 150, // 蜘蛛精 (规则未明确，暂定150)
                CharacterType.Pawn => 50,     // 无名小妖
                _ => 1 // 默认为1防止除零错误或无效进度条
            };
        }


        // 地图元素更新方法
        public void UpdateMapElements()
        {
            // 先清除地图上旧的角色标记 (如果 MapViewModel 没有自动处理)
            // MapVM.ClearCharacterDisplay(); // 需要在 MapViewModel 实现此方法
            myLogger?.LogInfo("--- UpdateMapElements called ---"); // 添加日志确认方法被调用
            lock (drawPicLock)
            {
                myLogger?.LogDebug($"UpdateMapElements: listOfBarracks.Count = {listOfBarracks.Count}");
                myLogger?.LogDebug($"UpdateMapElements: listOfSprings.Count = {listOfSprings.Count}");
                myLogger?.LogDebug($"UpdateMapElements: listOfFarms.Count = {listOfFarms.Count}");
                myLogger?.LogDebug($"UpdateMapElements: listOfTraps.Count = {listOfTraps.Count}");
                myLogger?.LogDebug($"UpdateMapElements: listOfEconomyResources.Count = {listOfEconomyResources.Count}");
                myLogger?.LogDebug($"UpdateMapElements: listOfAdditionResources.Count = {listOfAdditionResources.Count}");
                // 更新地图地形 (如果需要，基于 MapMessage)
                //MapVM.UpdateMap(MapMessage); // 假设 receivedMapMessage 在某处获得

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
                        trap.TrapType == TrapType.Hole ? "陷阱（坑洞）" : "陷阱（牢笼）" // 区分类型
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
                EconomyResourceType.SmallEconomyResource => "经济资源",
                EconomyResourceType.MediumEconomyResource => "经济资源",
                EconomyResourceType.LargeEconomyResource => "经济资源",
                _ => "经济资源"
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
             var buddhistTraps = listOfTraps.Count(t => t.TeamId == (int)PlayerTeam.BuddhistsTeam);
             var monsterTraps = listOfTraps.Count(t => t.TeamId == (int)PlayerTeam.MonstersTeam);
             BuddhistTeamBuildingInfo += $", 陷阱: {buddhistTraps}";
             MonstersTeamBuildingInfo += $", 陷阱: {monsterTraps}";
        }

        // 填充图例数据的方法
        private void InitializeMapLegend()
        {
            MapLegendItems.Clear();

            // 顺序按照你的要求
            MapLegendItems.Add(new LegendItem(Brushes.Cyan, "兵营/家园")); // 使用 UpdateBuildingCell 中的家园颜色 Cyan (兵营也是家)
            MapLegendItems.Add(new LegendItem(Brushes.White, "空地", Brushes.LightGray, new Thickness(1)));
            MapLegendItems.Add(new LegendItem(Brushes.LightGreen, "草丛"));
            MapLegendItems.Add(new LegendItem(Brushes.DarkGray, "障碍物"));

            MapLegendItems.Add(new LegendItem(Brushes.Gold, "经济")); // 与 UpdateResourceCell 一致


            MapLegendItems.Add(new LegendItem(Brushes.LightPink, "生命泉")); // 与 UpdateAdditionResourceCell 一致
            MapLegendItems.Add(new LegendItem(Brushes.OrangeRed, "狂战士")); // 与 UpdateAdditionResourceCell 一致
            MapLegendItems.Add(new LegendItem(Brushes.LightSkyBlue, "疾步灵")); // 与 UpdateAdditionResourceCell 一致
            MapLegendItems.Add(new LegendItem(Brushes.MediumPurple, "视野灵")); // 与 UpdateAdditionResourceCell 一致

            // 农场是建筑，颜色根据队伍区分
            // Team 0 = 取经队 = DarkRed
            // Team 1 = 妖怪队 = DarkBlue
            MapLegendItems.Add(new LegendItem(Brushes.DarkRed, "取经队农场")); // 对应 UpdateBuildingCell Team 0 颜色
            MapLegendItems.Add(new LegendItem(Brushes.DarkBlue, "妖怪队农场")); // 对应 UpdateBuildingCell Team 1 颜色

            // 陷阱颜色根据队伍区分
            // Team 0 = 取经队 = IndianRed
            // Team 1 = 妖怪队 = CornflowerBlue
            MapLegendItems.Add(new LegendItem(Brushes.IndianRed, "取经队坑洞")); // 对应 UpdateTrapCell Team 0 颜色
            MapLegendItems.Add(new LegendItem(Brushes.CornflowerBlue, "妖怪队坑洞")); // 对应 UpdateTrapCell Team 1 颜色
            MapLegendItems.Add(new LegendItem(Brushes.Tomato, "取经队牢笼")); // 牢笼颜色与坑洞相同
            MapLegendItems.Add(new LegendItem(Brushes.SteelBlue, "妖怪队牢笼")); // 牢笼颜色与坑洞相同

            // (可选) 添加通用建筑颜色 (如果 MapMessage 中只有 CONSTRUCTION)
            MapLegendItems.Add(new LegendItem(Brushes.Brown, "建筑点位 (未指定类型)"));
            // (可选) 添加未知区域颜色
            MapLegendItems.Add(new LegendItem(Brushes.Gainsboro, "未知区域"));
        }

    }
}