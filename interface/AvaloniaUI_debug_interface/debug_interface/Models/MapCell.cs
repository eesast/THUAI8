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
        Null_space_type,    //`未定义`
        Home, //`家`
        Space, //`空地`
        Barrier, //`障碍物`
        Bush, //`树林`
        Economic_Resource, //`经济资源`
        Additional_Resource, //`附加资源`
        Construction, //`建筑`
        Trap, //`陷阱`
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


    }
}