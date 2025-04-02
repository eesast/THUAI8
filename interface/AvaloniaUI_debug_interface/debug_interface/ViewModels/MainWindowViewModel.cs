using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using installer.Model;
using installer.Data;

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
        public ObservableCollection<CharacterViewModel> RedTeamCharacters { get; } = new();
        public ObservableCollection<CharacterViewModel> BlueTeamCharacters { get; } = new();

        // 构造函数
        public MainWindowViewModel(Logger logger, ConfigData config) : base(logger)
        {
            // 初始化MapViewModel
            MapVM = new MapViewModel();

            // 如果在设计模式下，可以添加一些测试数据
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                InitializeDesignTimeData();
            }

            // 启动UI更新定时器
            StartUiUpdateTimer();
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
                RedTeamCharacters.Add(new CharacterViewModel
                {
                    CharacterId = i + 1,
                    Name = $"取经队角色{i + 1}",
                    Hp = 1000,
                    ActiveState = "空置"
                });

                BlueTeamCharacters.Add(new CharacterViewModel
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
    }
}