//CharacterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace debug_interface.ViewModels
{
    public partial class CharacterViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string name = "角色"; // 默认角色名，可由服务器更新

        [ObservableProperty]
        private int hp = 1000; // 血量，可由服务器动态更新

        // 主动状态（单一）
        // 可能值: "空置","开采","攻击","释放技能","建造","移动"
        [ObservableProperty]
        private string activeState = "空置";
        //public string ActiveState
        //{
        //    get => activeState;
        //    set
        //    {
        //        if (value == activeState) return;
        //        activeState = value;
        //        OnPropertyChanged(nameof(DisplayStates));
        //    }
        //}
        // 被动状态（可叠加）
        // 可能值: "致盲","击退","定身","隐身" 等，可由服务器控制增减
        public ObservableCollection<string> PassiveStates { get; }

        // 装备清单：用名称+数量表示。如 {"小血瓶":2, "鞋子":1, "大护盾":1, "净化药水":3}
        // 为了方便绑定，用 ObservableCollection 来存储装备条目，每个条目包含Name和Count
        public ObservableCollection<EquipmentItem> EquipmentInventory { get; }

        // 状态选项列表和装备选项列表已不再使用ComboBox选择，而是纯展示。
        // 如果需要仍保留可由服务器更新，但这里不会再用于交互。

        // 构造函数
        public CharacterViewModel()
        {
            PassiveStates = new ObservableCollection<string>();
            EquipmentInventory = new ObservableCollection<EquipmentItem>();
        }

        // 用于UI展示状态字符串
        // 格式: （主动：xx 被动：xx yy zz）
        public string DisplayStates
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("（主动：").Append(ActiveState).Append(" 被动：");
                if (PassiveStates.Count > 0)
                    sb.Append(string.Join(" ", PassiveStates));
                else
                    sb.Append("无");
                sb.Append("）");
                return sb.ToString();
            }
        }

        // 用于UI展示装备字符串
        // 格式: 装备： 小血瓶x2 鞋子x1 大护盾x1 净化药水x3
        public string DisplayEquipments
        {
            get
            {
                if (EquipmentInventory.Count == 0)
                {
                    return "装备：无";
                }
                var parts = EquipmentInventory.Select(e => $"{e.Name}x{e.Count}");
                return "装备：" + string.Join(" ", parts);
            }
        }

        // 当ActiveState或PassiveStates变化后，通知UI更新 DisplayStates
        partial void OnActiveStateChanged(string oldValue, string newValue)
        {
            OnPropertyChanged(nameof(DisplayStates));
        }

        // 如果被动状态列表更新，需要调用OnPropertyChanged(nameof(DisplayStates))
        // 可以在服务器更新逻辑中调用。

        // 同理，当EquipmentInventory变化时，需要更新DisplayEquipments
        // 可以在添加/移除装备后调用OnPropertyChanged(nameof(DisplayEquipments))
    }

    // 定义装备类
    public class EquipmentItem
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public EquipmentItem(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
