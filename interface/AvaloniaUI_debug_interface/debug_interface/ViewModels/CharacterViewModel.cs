using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace debug_interface.ViewModels
{
    public partial class CharacterViewModel : ViewModelBase
    {
        [ObservableProperty]
        private long characterId;

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private int hp;

        [ObservableProperty]
        private int posX;

        [ObservableProperty]
        private int posY;

        [ObservableProperty]
        private string activeState = "";

        public ObservableCollection<string> PassiveStates { get; } = new();

        public ObservableCollection<EquipmentItem> EquipmentInventory { get; } = new();

        // 格式化状态文本的属性
        public string DisplayStates =>
            $"{ActiveState} {(PassiveStates.Count > 0 ? $"[{string.Join(", ", PassiveStates)}]" : "")}";

        // 格式化装备文本的属性
        public string DisplayEquipments =>
            EquipmentInventory.Count > 0 ? string.Join(", ", EquipmentInventory) : "无";
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