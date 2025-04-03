//MapCell.cs
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debug_interface.Models
{
    //internal class MapCell
    //{
    //}
    // 定义地图单元格的类型，根据游戏规则可包含障碍物、空地、草丛、资源、建筑等
    public enum MapCellType
    {
        Obstacle,   // 障碍物（例如地图边界或特定阻挡物）
        OpenLand,   // 空地，角色可自由移动
        Grass,      // 草丛，可提供隐蔽效果（决战期后会消失）
        Resource,   // 资源点（用于开采经济资源）
        Building    // 建筑（预留，例如兵营、农场等）
    }

    // 地图单元类，继承自 ObservableObject 便于数据绑定更新
    public partial class MapCell : ObservableObject
    {
        // 地图中的行号（0~49）
        [ObservableProperty]
        private int cellX;

        // 地图中的列号（0~49）
        [ObservableProperty]
        private int cellY;

        // 当前单元格的类型
        [ObservableProperty]
        private MapCellType cellType;

        // 该单元格显示时使用的颜色
        [ObservableProperty]
        private SolidColorBrush displayColor;

        // 可选：显示的文字，例如建筑血量、资源进度等
        [ObservableProperty]
        private string displayText;
    }
}


