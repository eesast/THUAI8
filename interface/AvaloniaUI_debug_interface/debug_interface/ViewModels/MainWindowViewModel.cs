using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using installer.Model;
using installer.Data;
using Protobuf;
using Google.Protobuf;
using System.Linq;

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

        [ObservableProperty]
        private string someBuildingInfo = "";

        [ObservableProperty]
        private string anotherBuildingInfo = "";

        [ObservableProperty]
        private int buddhistTeamEconomy = 0;

        [ObservableProperty]
        private int monstersTeamEconomy = 0;

        [ObservableProperty]
        private MapViewModel mapVM;

        // 团队角色集合
        public ObservableCollection<CharacterViewModel> BuddhistsTeamCharacters { get; } = new();
        public ObservableCollection<CharacterViewModel> MonstersTeamCharacters { get; } = new();

        // 默认构造函数
        public MainWindowViewModel() : base()
        {
            // 初始化MapViewModel
            MapVM = new MapViewModel();

            // 如果在设计模式下，可以添加一些测试数据
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                InitializeDesignTimeData();
            }
        }

        // 带参数的构造函数
        public MainWindowViewModel(Logger? logger, ConfigData? config) : base()
        {
            if (logger != null)
                myLogger = logger;

            // 初始化MapViewModel
            MapVM = new MapViewModel();

            // 如果在设计模式下，可以添加一些测试数据
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                InitializeDesignTimeData();
            }
        }

        // 设计时数据
        private void InitializeDesignTimeData()
        {
            GameLog = "设计模式 - 模拟数据";
            CurrentTime = "12:34";
            RedScore = 50;
            BlueScore = 30;

            // 添加一些测试角色
            for (int i = 0; i < 3; i++)
            {
                BuddhistsTeamCharacters.Add(new CharacterViewModel
                {
                    CharacterId = i + 1,
                    Name = $"取经队角色{i + 1}",
                    Hp = 1000,
                    ActiveState = "空置"
                });

                MonstersTeamCharacters.Add(new CharacterViewModel
                {
                    CharacterId = i + 101,
                    Name = $"妖怪队角色{i + 1}",
                    Hp = 1200,
                    ActiveState = "移动"
                });
            }
        }

        // 定时器更新方法
        protected override void OnTimerTick(object? sender, EventArgs e)
        {
            // 更新当前时间显示
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }

        public void UpdateCharacters()
        {
            lock (drawPicLock)
            {
                foreach (var data in listOfCharacters)
                {
                    // 创建新的角色视图模型
                    var character = new CharacterViewModel
                    {
                        CharacterId = data.PlayerId,
                        Name = GetCharacterName(data.CharacterType),
                        Hp = data.Hp,
                        PosX = data.X,  // 根据您的坐标系统可能需要调整
                        PosY = data.Y,
                        ActiveState = data.CharacterActiveState.ToString()
                    };

                    // 检查被动状态
                    if (data.BlindState != CharacterState.NullCharacterState)
                        character.PassiveStates.Add("致盲");
                    if (data.StunnedState != CharacterState.NullCharacterState)
                        character.PassiveStates.Add("眩晕");
                    if (data.InvisibleState != CharacterState.NullCharacterState)
                        character.PassiveStates.Add("隐身");
                    if (data.BurnedState != CharacterState.NullCharacterState)
                        character.PassiveStates.Add("燃烧");

                    // 检查装备
                    if (data.ShieldEquipment > 0)
                        character.EquipmentInventory.Add(new EquipmentItem("护盾", data.ShieldEquipment));
                    if (data.ShoesEquipment > 0)
                        character.EquipmentInventory.Add(new EquipmentItem("鞋子", 1));

                    // 根据团队添加到相应列表
                    if (data.TeamId == (int)PlayerTeam.BuddhistsTeam)
                    {
                        UpdateOrAddCharacter(BuddhistsTeamCharacters, character);
                    }
                    else if (data.TeamId == (int)PlayerTeam.MonstersTeam)
                    {
                        UpdateOrAddCharacter(MonstersTeamCharacters, character);
                    }
                }
            }
        }

        private void UpdateOrAddCharacter(ObservableCollection<CharacterViewModel> list, CharacterViewModel newCharacter)
        {
            // 尝试找到现有角色并更新
            var existing = list.FirstOrDefault(c => c.CharacterId == newCharacter.CharacterId);
            if (existing != null)
            {
                existing.Hp = newCharacter.Hp;
                existing.PosX = newCharacter.PosX;
                existing.PosY = newCharacter.PosY;
                existing.ActiveState = newCharacter.ActiveState;
                existing.PassiveStates.Clear();
                foreach (var state in newCharacter.PassiveStates)
                    existing.PassiveStates.Add(state);
                existing.EquipmentInventory.Clear();
                foreach (var equip in newCharacter.EquipmentInventory)
                    existing.EquipmentInventory.Add(equip);
            }
            else
            {
                // 添加新角色
                list.Add(newCharacter);
            }
        }

        private string GetCharacterName(CharacterType type)
        {
            return type switch
            {
                CharacterType.TangSeng => "唐僧",
                CharacterType.SunWukong => "孙悟空",
                CharacterType.ZhuBajie => "猪八戒",
                CharacterType.ShaWujing => "沙悟净",
                CharacterType.BaiLongma => "白龙马",
                CharacterType.Monkid => "小僧",
                CharacterType.JiuLing => "九灵",
                CharacterType.HongHaier => "红孩儿",
                CharacterType.NiuMowang => "牛魔王",
                CharacterType.TieShan => "铁扇",
                CharacterType.ZhiZhujing => "蜘蛛精",
                CharacterType.Pawn => "小妖",
                _ => "未知角色"
            };
        }

        // 地图元素更新方法
        public void UpdateMapElements()
        {
            lock (drawPicLock)
            {
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
                        trap.TrapType == TrapType.Hole ? "坑洞" : "牢笼"
                    );
                }

                // 更新经济资源
                foreach (var resource in listOfEconomyResources)
                {
                    MapVM.UpdateResourceCell(
                        resource.X / 1000,
                        resource.Y / 1000,
                        GetEconomyResourceType(resource.EconomyResourceType),
                        resource.Process
                    );
                }

                // 更新加成资源
                foreach (var resource in listOfAdditionResources)
                {
                    MapVM.UpdateAdditionResourceCell(
                        resource.X / 1000,
                        resource.Y / 1000,
                        GetAdditionResourceType(resource.AdditionResourceType),
                        resource.Hp
                    );
                }

                // 更新地图上的角色位置
                MapVM.UpdateCharacterPositions(BuddhistsTeamCharacters, MonstersTeamCharacters);
            }
        }

        private string GetEconomyResourceType(EconomyResourceType type)
        {
            return type switch
            {
                EconomyResourceType.SmallEconomyResource => "小资源",
                EconomyResourceType.MediumEconomyResource => "中资源",
                EconomyResourceType.LargeEconomyResource => "大资源",
                _ => "未知资源"
            };
        }

        private string GetAdditionResourceType(AdditionResourceType type)
        {
            return type switch
            {
                AdditionResourceType.LifePool1 => "生命池1",
                AdditionResourceType.LifePool2 => "生命池2",
                AdditionResourceType.LifePool3 => "生命池3",
                AdditionResourceType.CrazyMan1 => "疯人1",
                AdditionResourceType.CrazyMan2 => "疯人2",
                AdditionResourceType.CrazyMan3 => "疯人3",
                AdditionResourceType.QuickStep => "快步",
                AdditionResourceType.WideView => "广视",
                _ => "未知加成"
            };
        }

        public void UpdateGameStatus()
        {
            if (listOfAll.Count > 0)
            {
                var data = listOfAll[0];
                CurrentTime = FormatGameTime(data.GameTime);
                RedScore = data.BuddhistsTeamScore;
                BlueScore = data.MonstersTeamScore;
                BuddhistTeamEconomy = data.BuddhistsTeamEconomy;
                MonstersTeamEconomy = data.MonstersTeamEconomy;

                // 可以添加更多游戏状态信息
                SomeBuildingInfo = $"取经队经济: {data.BuddhistsTeamEconomy}, 英雄血量: {data.BuddhistsHeroHp}";
                AnotherBuildingInfo = $"妖怪队经济: {data.MonstersTeamEconomy}, 英雄血量: {data.MonstersHeroHp}";
            }
        }

        private string FormatGameTime(int gameTimeInMilliseconds)
        {
            int seconds = gameTimeInMilliseconds / 1000;
            int minutes = seconds / 60;
            seconds %= 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}