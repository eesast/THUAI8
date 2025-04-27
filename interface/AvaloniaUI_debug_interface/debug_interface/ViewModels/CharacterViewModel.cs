using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Protobuf;

namespace debug_interface.ViewModels
{
    public partial class CharacterViewModel : ViewModelBase
    {
        [ObservableProperty]
        private long guid; 

        [ObservableProperty]
        private long characterId;

        [ObservableProperty]
        private CharacterType characterType;

        [ObservableProperty]
        private long teamId; // *** 添加 TeamId 属性 *** (用于区分队伍)

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private int hp;

        [ObservableProperty]
        private int maxHp; // 新增: 最大血量

        [ObservableProperty]
        private int posX;

        [ObservableProperty]
        private int posY;

        [ObservableProperty]
        private string activeState = "";

        public ObservableCollection<string> StatusEffects { get; } = new();

        //public ObservableCollection<string> PassiveStates { get; } = new();
        public ObservableCollection<EquipmentItem> EquipmentInventory { get; } = new();// *** 保留用于计数装备的列表 (目前可能为空) ***
        //public string DisplayStates => $"{ActiveState} {(PassiveStates.Count > 0 ? $"[{string.Join(", ", PassiveStates)}]" : "")}";
        //public string DisplayEquipments => EquipmentInventory.Count > 0 ? string.Join(", ", EquipmentInventory) : "无";

        public string TeamName => TeamId switch
        {
            0 => "妖怪队",   // 服务器用 0 代表妖怪队
            1 => "取经队",   // 服务器用 1 代表取经队
            _ => "未知队伍"
        };
    }

    public class EquipmentItem
    {
        public string Name { get; }
        public int Count { get; }

        public EquipmentItem(string name, int count)
        {
            Name = name;
            Count = count;
        }

        public override string ToString()
        {
            return Count > 1 ? $"{Name}x{Count}" : Name;
        }


    }
}