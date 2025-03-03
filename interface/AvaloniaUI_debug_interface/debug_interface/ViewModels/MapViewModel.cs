using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using debug_interface.Models;
using Avalonia.Media;

namespace debug_interface.ViewModels
{
    // 地图视图模型，管理整个 50×50 的地图数据
    public partial class MapViewModel : ObservableObject
    {
        // 定义地图网格大小为 50（行）× 50（列）
        public const int GridSize = 50;

        // ObservableCollection 存放 2500 个 MapCell 对象，
        // 数据绑定到视图后，界面会显示出整个地图
        public ObservableCollection<MapCell> MapCells { get; }

        public MapViewModel()
        {
            MapCells = new ObservableCollection<MapCell>();
            InitializeDefaultMap();
        }

        // 初始化默认地图数据，用于测试效果
        private void InitializeDefaultMap()
        {
            // 遍历所有行和列
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    // 创建一个 MapCell 对象，并设置行列信息
                    var cell = new MapCell
                    {
                        CellX = i,
                        CellY = j,
                        // 根据坐标决定默认单元格类型
                        CellType = GetDefaultCellType(i, j),
                        DisplayText = "" // 默认无文本显示，可后续根据需要设置
                    };
                    // 设置显示颜色，根据单元格类型获取颜色
                    cell.DisplayColor = new SolidColorBrush(GetColorForCellType(cell.CellType));

                    MapCells.Add(cell);
                }
            }
        }

        // 根据单元格在地图中的坐标决定默认类型
        private MapCellType GetDefaultCellType(int i, int j)
        {
            // 1. 边界（第一行、最后一行、第一列、最后一列）均设为障碍物
            if (i == 0 || i == GridSize - 1 || j == 0 || j == GridSize - 1)
            {
                return MapCellType.Obstacle;
            }
            // 2. 在特定位置放置资源点（测试用）
            if ((i == 10 && j == 10) || (i == 20 && j == 20) || (i == 30 && j == 30))
            {
                return MapCellType.Resource;
            }
            // 3. 在特定位置放置草丛（测试用）
            if ((i == 15 && j == 35) || (i == 35 && j == 15))
            {
                return MapCellType.Grass;
            }
            // 4. 可预留一个建筑点，例如 (25,25)
            if (i == 25 && j == 25)
            {
                return MapCellType.Building;
            }
            // 5. 其他区域默认为空地
            return MapCellType.OpenLand;
        }

        // 根据单元格类型返回对应的颜色（可根据需要调整颜色）
        private Color GetColorForCellType(MapCellType type)
        {
            switch (type)
            {
                case MapCellType.Obstacle:
                    return Colors.DarkGray;
                case MapCellType.OpenLand:
                    return Colors.LightGreen;
                case MapCellType.Grass:
                    return Colors.Green;
                case MapCellType.Resource:
                    return Colors.Yellow;
                case MapCellType.Building:
                    return Colors.Orange;
                default:
                    return Colors.Black;
            }
        }

        // 用于更新地图数据的方法，
        // newMapData 是一个 50x50 的二维整型数组，数值应能映射为 MapCellType 枚举值
        public void UpdateMap(int[,] newMapData)
        {
            if (newMapData.GetLength(0) != GridSize || newMapData.GetLength(1) != GridSize)
                throw new ArgumentException("地图数据尺寸不正确");

            // 遍历二维数组更新每个 MapCell 对象
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    int index = i * GridSize + j;
                    // 将整型转换为 MapCellType 枚举
                    MapCellType newType = (MapCellType)newMapData[i, j];
                    var cell = MapCells[index];
                    cell.CellType = newType;
                    // 更新颜色
                    cell.DisplayColor = new SolidColorBrush(GetColorForCellType(newType));
                    // 如果需要，可以根据新数据更新文本（例如建筑血量、资源剩余）
                    cell.DisplayText = "123"; // 这里留空，可扩展
                }
            }
        }
    }
}
