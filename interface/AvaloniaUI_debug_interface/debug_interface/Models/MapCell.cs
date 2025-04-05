// MapCell.cs
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using debug_interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debug_interface.Models
{
    public enum MapCellType
    {
        Obstacle,   // 障碍物
        OpenLand,   // 空地
        Grass,      // 草丛
        Resource,   // 资源点
        Building,   // 建筑
        Trap        // 陷阱 
    }

    public partial class MapCell : ObservableObject
    {
        [ObservableProperty]
        private int cellX;

        [ObservableProperty]
        private int cellY;

        [ObservableProperty]
        private MapCellType cellType;

        [ObservableProperty]
        private SolidColorBrush displayColor = new SolidColorBrush(Colors.White); // 默认白色

        [ObservableProperty]
        private string displayText = ""; // 用于显示血量等

        [ObservableProperty]
        private string toolTipText = ""; // 用于鼠标悬浮提示

        // 可以选择性地添加血量信息，如果需要更复杂的绑定
        // [ObservableProperty]
        // private int currentHp;
        // [ObservableProperty]
        // private int maxHp;
    }
}