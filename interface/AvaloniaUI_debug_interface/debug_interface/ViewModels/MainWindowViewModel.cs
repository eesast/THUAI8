// ViewModels/MainWindowViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System;
using System.Timers;

namespace debug_interface.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<CharacterViewModel> RedTeamCharacters { get; }
        public ObservableCollection<CharacterViewModel> BlueTeamCharacters { get; }

        [ObservableProperty]
        private string someBuildingInfo = "红方建筑信息...";

        [ObservableProperty]
        private string anotherBuildingInfo = "蓝方建筑信息...";

        [ObservableProperty]
        private string currentTime = DateTime.Now.ToString("HH:mm:ss");

        [ObservableProperty]
        private int redScore = 0;

        [ObservableProperty]
        private int blueScore = 0;

        [ObservableProperty]
        private bool isBlueView = true;

        public bool IsRedView
        {
            get => !IsBlueView;
            set => IsBlueView = !value;
        }

        [ObservableProperty]
        private string gameLog = "地图...";

        private Timer _timer;

        public MainWindowViewModel()
        {
            RedTeamCharacters = new ObservableCollection<CharacterViewModel>();
            BlueTeamCharacters = new ObservableCollection<CharacterViewModel>();

            // 初始化角色，给每个角色不同的数据以示例可变性
            for (int i = 0; i < 6; i++)
            {
                var redChar = new CharacterViewModel()
                {
                    Name = "红方角色" + (i + 1),
                    Hp = 1000 * (i + 1),
                    ActiveState = i % 2 == 0 ? "攻击" : "移动",
                };
                redChar.PassiveStates.Add("致盲");
                if (i % 3 == 0) redChar.PassiveStates.Add("定身");
                redChar.EquipmentInventory.Add(new EquipmentItem("小血瓶", 2));
                if (i % 2 == 0)
                    redChar.EquipmentInventory.Add(new EquipmentItem("大护盾", 1));

                RedTeamCharacters.Add(redChar);

                var blueChar = new CharacterViewModel()
                {
                    Name = "蓝方角色" + (i + 1),
                    Hp = 1500 + i * 500,
                    ActiveState = "空置",
                };
                blueChar.PassiveStates.Add("隐身");
                //blueChar.PassiveStates.Remove("致盲"); 
                blueChar.EquipmentInventory.Add(new EquipmentItem("净化药水", 3));
                if (i % 2 == 1)
                    blueChar.EquipmentInventory.Add(new EquipmentItem("鞋子", 1));

                BlueTeamCharacters.Add(blueChar);
            }

            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                OnPropertyChanged(nameof(CurrentTime));
            });
        }
    }
}
