//MapCell.cs
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
    //internal class MapCell
    //{
    //}
    // 定义地图单元格的类型，根据游戏规则可包含障碍物、空地、草丛、资源、建筑等
    public enum MapCellType
    {
        Empty,
        Obstacle,
        Building,
        Resource,
        Character
    }

    public partial class MapCell : ViewModelBase
    {
        [ObservableProperty]
        private int row;

        [ObservableProperty]
        private int col;

        public int CellX
        {
            get => Col;
            set => Col = value;
        }

        public int CellY
        {
            get => Row;
            set => Row = value;
        }


        [ObservableProperty]
        private MapCellType cellType;

        [ObservableProperty]
        private string displayText = "";

        [ObservableProperty]
        private IBrush displayColor = new SolidColorBrush(Colors.Black);

        [ObservableProperty]
        private IBrush backgroundColor = new SolidColorBrush(Colors.White);
    }
}


