using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using debug_interface.Models;

namespace debug_interface.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        private const int GridSize = 50;

        [ObservableProperty]
        private ObservableCollection<MapCell> mapCells = new();

        public MapViewModel()
        {
            // 初始化地图单元格
            InitializeMapCells();
        }

        private void InitializeMapCells()
        {
            MapCells.Clear();
            for (int i = 0; i < GridSize * GridSize; i++)
            {
                MapCells.Add(new MapCell
                {
                    Row = i / GridSize,
                    Col = i % GridSize,
                    CellType = MapCellType.Empty,
                    DisplayText = "",
                    DisplayColor = new SolidColorBrush(Colors.White),
                    BackgroundColor = new SolidColorBrush(Colors.LightGray)
                });
            }
        }

        // 更新整个地图
        public void UpdateMap(int[,] mapData)
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int index = i * GridSize + j;
                    if (index < MapCells.Count)
                    {
                        int cellType = mapData[i, j];
                        UpdateCellType(MapCells[index], cellType);
                    }
                }
            }

            OnPropertyChanged(nameof(MapCells));
        }

        private void UpdateCellType(MapCell cell, int cellType)
        {
            // 根据cellType设置单元格属性
            // 这里简化处理
            switch (cellType)
            {
                case 0: // 空地
                    cell.CellType = MapCellType.Empty;
                    cell.BackgroundColor = new SolidColorBrush(Colors.White);
                    break;
                case 1: // 障碍物
                    cell.CellType = MapCellType.Obstacle;
                    cell.BackgroundColor = new SolidColorBrush(Colors.DarkGray);
                    break;
                case 2: // 资源
                    cell.CellType = MapCellType.Resource;
                    cell.BackgroundColor = new SolidColorBrush(Colors.Green);
                    break;
                default:
                    cell.CellType = MapCellType.Empty;
                    cell.BackgroundColor = new SolidColorBrush(Colors.LightGray);
                    break;
            }
        }

        // 更新角色位置
        public void UpdateCharacterPosition(long characterId, long teamId, int x, int y, string name)
        {
            // 简化实现
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].DisplayText = name.Substring(0, 1);
                MapCells[index].DisplayColor = teamId == 1
                    ? new SolidColorBrush(Colors.Red)
                    : new SolidColorBrush(Colors.Blue);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新建筑
        public void UpdateBuildingCell(int x, int y, string team, string buildingType, int hp)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Building;
                MapCells[index].DisplayText = buildingType.Substring(0, 1);
                MapCells[index].DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.DarkRed)
                    : new SolidColorBrush(Colors.DarkBlue);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新陷阱
        public void UpdateTrapCell(int x, int y, string team, string trapType)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Obstacle;
                MapCells[index].DisplayText = trapType.Substring(0, 1);
                MapCells[index].DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.IndianRed)
                    : new SolidColorBrush(Colors.CornflowerBlue);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新资源
        public void UpdateResourceCell(int x, int y, string resourceType, int process)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Resource;
                MapCells[index].DisplayText = process.ToString();
                MapCells[index].DisplayColor = new SolidColorBrush(Colors.DarkGreen);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新额外资源
        public void UpdateAdditionResourceCell(int x, int y, string resourceName, int value)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Resource;
                MapCells[index].DisplayText = value.ToString();

                // 根据资源类型选择颜色
                if (resourceName.Contains("生命池"))
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGreen);
                }
                else if (resourceName.Contains("疯人"))
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.OrangeRed);
                }
                else
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Purple);
                }

                OnPropertyChanged(nameof(MapCells));
            }
        }
    }

    //public enum MapCellType
    //{
    //    Empty,
    //    Obstacle,
    //    Building,
    //    Resource,
    //    Character
    //}

    //public partial class MapCell : ViewModelBase
    //{
    //    [ObservableProperty]
    //    private int row;

    //    [ObservableProperty]
    //    private int col;

    //    [ObservableProperty]
    //    private MapCellType cellType;

    //    [ObservableProperty]
    //    private string displayText = "";

    //    [ObservableProperty]
    //    private IBrush displayColor = new SolidColorBrush(Colors.Black);

    //    [ObservableProperty]
    //    private IBrush backgroundColor = new SolidColorBrush(Colors.White);
    //}
}