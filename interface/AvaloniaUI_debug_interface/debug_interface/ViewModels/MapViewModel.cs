//MapViewModel.cs
using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Avalonia.Media;
using Avalonia.Threading;

using Grpc.Core;

using installer.Data;
using installer.Model;

using debug_interface.Models;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Google.Protobuf;
using System.Linq; // 解决Concat错误
using Protobuf;

namespace debug_interface.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        private const int GridSize = 50;

        [ObservableProperty]
        private ObservableCollection<MapCell> mapCells = new();

        //// 添加缺失的字段
        //private Canvas? characterCanvas;
        //private Dictionary<string, Grid> characterElements = new Dictionary<string, Grid>();
        //private MainWindowViewModel? viewModel;

        public MapViewModel()
        {
            // 初始化地图单元格
            InitializeMapCells();
        }

        // 设置Canvas和ViewModel引用的方法
        //public void SetCanvasAndViewModel(Canvas canvas, MainWindowViewModel vm)
        //{
        //    characterCanvas = canvas;
        //    viewModel = vm;
        //    RefreshCharacters();
        //}

        // 更新角色位置的方法
        //private void UpdateCharacterPosition(Grid element, int x, int y)
        //{
        //    if (characterCanvas == null) return;

        //    // 更新位置
        //    Canvas.SetLeft(element, x);
        //    Canvas.SetTop(element, y);
        //}

        //private void InitializeCharacters(IEnumerable<CharacterViewModel> characters, Color color)
        //{
        //    if (characterCanvas == null || viewModel == null) return;

        //    foreach (var character in characters)
        //    {
        //        var id = $"{color.ToString()}_{character.Name}";
        //        if (!characterElements.ContainsKey(id))
        //        {
        //            var grid = new Grid { Width = 15, Height = 15 };
        //            var ellipse = new Ellipse
        //            {
        //                Width = 15,
        //                Height = 15,
        //                Fill = new SolidColorBrush(Colors.White),
        //                Stroke = new SolidColorBrush(color),
        //                StrokeThickness = 2,
        //                Tag = character.Name
        //            };
        //            grid.Children.Add(ellipse);
        //            ToolTip.SetTip(grid, character.Name);
        //            characterCanvas.Children.Add(grid);
        //            characterElements[id] = grid;
        //        }

        //        var element = characterElements[id];
        //        UpdateCharacterPosition(element, character.PosX, character.PosY);

        //        character.PropertyChanged += (s, e) =>
        //        {
        //            if (e.PropertyName == nameof(CharacterViewModel.PosX) || e.PropertyName == nameof(CharacterViewModel.PosY))
        //            {
        //                UpdateCharacterPosition(element, character.PosX, character.PosY);
        //            }
        //        };
        //    }
        //}

        //初始化地图单元格
        public void InitializeMapCells()
        {
            MapCells.Clear();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    MapCells.Add(new MapCell
                    {
                        CellX = i,
                        CellY = j,
                        CellType = MapCellType.OpenLand,
                        DisplayColor = new SolidColorBrush(Colors.White),
                        DisplayText = ""
                    });
                }
            }
        }

        // 更新整个地图
        public void UpdateMap(MessageOfMap mapMessage)
        {
            if (mapMessage == null || mapMessage.Rows == null) return; // 添加空检查

            // 先重置所有非障碍物格子的基础状态，防止旧的建筑/资源信息残留
            // （或者在 UpdateCellType 中处理，如果 PlaceType 是 Space/Bush 就覆盖旧信息）
            // 也可以选择在 MainWindowViewModel 更新元素前调用一个 ClearDynamicElements 方法

            for (int i = 0; i < mapMessage.Rows.Count && i < GridSize; i++)
            {
                var row = mapMessage.Rows[i];
                if (row == null || row.Cols == null) continue;

                for (int j = 0; j < row.Cols.Count && j < GridSize; j++)
                {
                    int index = i * GridSize + j;
                    if (index >= 0 && index < MapCells.Count) // 增加 index >= 0 检查
                    {
                        UpdateCellType(i, j, row.Cols[j]);
                    }
                }
            }
        }

        private void UpdateCellType(int x, int y, PlaceType placeType)
        {
            int index = x * GridSize + y;
            // index 检查已在外层 UpdateMap 完成，此处不再重复
            var cell = MapCells[index];

            // !!! 关键: 只有当地形是基础类型时才重置文本和Tooltip
            // 如果是建筑/资源等，它们的专用更新方法会设置文本和Tooltip
            bool isBaseTerrain = placeType == PlaceType.Space ||
                                 placeType == PlaceType.Barrier ||
                                 placeType == PlaceType.Bush ||
                                 placeType == PlaceType.Home; // Home 可能被建筑覆盖，但可以先设个基础

            if (isBaseTerrain)
            {
                cell.DisplayText = ""; // 重置文本

            }

            // 根据地形类型设置单元格属性
            switch (placeType)
            {
                case PlaceType.Home:
                    cell.CellType = MapCellType.Building; // 假设家也是一种建筑区域
                    cell.DisplayColor = new SolidColorBrush(Colors.Cyan); // 特殊颜色标记家园

                    break;
                case PlaceType.Space:
                    cell.CellType = MapCellType.OpenLand;
                    cell.DisplayColor = new SolidColorBrush(Colors.White);

                    break;
                case PlaceType.Barrier:
                    cell.CellType = MapCellType.Obstacle;
                    cell.DisplayColor = new SolidColorBrush(Colors.DarkGray);

                    break;
                case PlaceType.Bush:
                    cell.CellType = MapCellType.Grass;
                    cell.DisplayColor = new SolidColorBrush(Colors.LightGreen);

                    break;
                // 其他类型 (Resource, Construction, Trap) 由特定方法处理颜色、文本、提示
                // 不在此处设置默认值，以免覆盖后续的详细信息
                case PlaceType.EconomyResource:
                case PlaceType.AdditionResource:
                case PlaceType.Construction:
                case PlaceType.Trap:
                    // 这里可以设置一个临时的 CellType，如果需要的话
                    // cell.CellType = MapCellType.Resource; // or Building or Trap
                    // 但颜色、文本、提示由 UpdateXXXCell 设置
                    break;
                default: // 未知类型
                    cell.CellType = MapCellType.OpenLand; // 视为OpenLand
                    cell.DisplayColor = new SolidColorBrush(Colors.LightGray); // 用浅灰标记未知区域
                    cell.DisplayText = "?"; // 可以显示一个问号
                    break;
            }
        }

        //private void UpdateCellType(int x, int y, PlaceType placeType)
        //{
        //    int index = x * GridSize + y;
        //    if (index >= MapCells.Count) return;

        //    // 根据地形类型设置单元格属性
        //    switch (placeType)
        //    {
        //        case PlaceType.Home:
        //            MapCells[index].CellType = MapCellType.Building;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Gold);
        //            break;
        //        case PlaceType.Space:
        //            MapCells[index].CellType = MapCellType.OpenLand;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.White);
        //            break;
        //        case PlaceType.Barrier:
        //            MapCells[index].CellType = MapCellType.Obstacle;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.DarkGray);
        //            break;
        //        case PlaceType.Bush:
        //            MapCells[index].CellType = MapCellType.Grass;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGreen);
        //            break;
        //        case PlaceType.EconomyResource:
        //            MapCells[index].CellType = MapCellType.Resource;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Gold);
        //            break;
        //        case PlaceType.AdditionResource:
        //            MapCells[index].CellType = MapCellType.Resource;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Purple);
        //            break;
        //        case PlaceType.Construction:
        //            MapCells[index].CellType = MapCellType.Building;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Brown);
        //            break;
        //        case PlaceType.Trap:
        //            MapCells[index].CellType = MapCellType.Obstacle;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Red);
        //            break;
        //        default:
        //            MapCells[index].CellType = MapCellType.OpenLand;
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGray);
        //            break;
        //    }
        //}

        //public void UpdateCharacterPositions(IEnumerable<CharacterViewModel> buddhistsTeam, IEnumerable<CharacterViewModel> monstersTeam)
        //{
        //    // 使用System.Linq的Concat扩展方法
        //    var allCharacters = buddhistsTeam.Concat(monstersTeam);

        //    foreach (var character in allCharacters)
        //    {
        //        int index = (int)(character.PosX * GridSize + character.PosY);
        //        if (index >= 0 && index < MapCells.Count)
        //        {
        //            MapCells[index].DisplayText = character.Name;
        //        }
        //    }
        //}


        // 更新建筑
        public void UpdateBuildingCell(int x, int y, string team, string buildingType, int hp)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Building;
                //cell.DisplayText = buildingType.Substring(0, 1); // 先用文字代替，后面修改为显示HP
                cell.DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.DarkRed)
                    : new SolidColorBrush(Colors.DarkBlue);

                // 更新HP和Tooltip
                // 注意：游戏规则中未明确建筑的最大血量，这里用传入的hp作为当前值，假设需要一个最大值才能显示 current/max
                // 暂时只显示类型和当前HP
                // TODO: 获取建筑的最大HP (可能需要从游戏规则或新消息字段获取)
                int maxHp = GetBuildingMaxHp(buildingType); // 需要实现这个辅助方法
                cell.DisplayText = $"{hp}/{maxHp}";
                //cell.ToolTipText = $"类型: {buildingType}\n队伍: {team}\n血量: {hp}/{maxHp}";

                // 触发UI更新 (如果MapCell的属性变更没有自动通知)
                // OnPropertyChanged(nameof(MapCells)); // ObservableCollection通常会自动处理子项变更通知，但如果直接修改子项属性，有时需要手动触发
            }
        }

        //public void UpdateBuildingCell(int x, int y, string team, string buildingType, int hp)
        //{
        //    int index = x * GridSize + y;
        //    if (index >= 0 && index < MapCells.Count)
        //    {
        //        MapCells[index].CellType = MapCellType.Building;
        //        MapCells[index].DisplayText = buildingType.Substring(0, 1);
        //        MapCells[index].DisplayColor = team == "取经队"
        //            ? new SolidColorBrush(Colors.DarkRed)
        //            : new SolidColorBrush(Colors.DarkBlue);

        //        OnPropertyChanged(nameof(MapCells));
        //    }
        //}

        // 更新陷阱
        public void UpdateTrapCell(int x, int y, string team, string trapType)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Trap; // 使用新的类型
                //cell.DisplayText = trapType.Substring(0, 1);
                cell.DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.IndianRed) // 可以为不同陷阱设置不同颜色
                    : new SolidColorBrush(Colors.CornflowerBlue);

                // 更新Tooltip (陷阱没有血量)
                cell.DisplayText = ""; // 陷阱通常不显示血量文本
                //cell.ToolTipText = $"类型: {trapType}\n队伍: {team}";

                // OnPropertyChanged(nameof(MapCells));
            }
        }

        //public void UpdateTrapCell(int x, int y, string team, string trapType)
        //{
        //    int index = x * GridSize + y;
        //    if (index >= 0 && index < MapCells.Count)
        //    {
        //        MapCells[index].CellType = MapCellType.Obstacle;
        //        MapCells[index].DisplayText = trapType.Substring(0, 1);
        //        MapCells[index].DisplayColor = team == "取经队"
        //            ? new SolidColorBrush(Colors.IndianRed)
        //            : new SolidColorBrush(Colors.CornflowerBlue);

        //        OnPropertyChanged(nameof(MapCells));
        //    }
        //}

        // 更新资源
        public void UpdateResourceCell(int x, int y, string resourceType, int process) // process 是采集进度/剩余量
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Resource;
                // cell.DisplayText = process.ToString(); // 显示剩余量
                cell.DisplayColor = new SolidColorBrush(Colors.Gold); // 经济资源用金色

                // 更新HP和Tooltip
                int maxResource = 10000; // 根据规则，经济资源上限1w
                cell.DisplayText = $"{process}/{maxResource}";
                //cell.ToolTipText = $"类型: {resourceType}\n剩余量: {process}/{maxResource}";


                // OnPropertyChanged(nameof(MapCells));
            }
        }
        //public void UpdateResourceCell(int x, int y, string resourceType, int process)
        //{
        //    int index = x * GridSize + y;
        //    if (index >= 0 && index < MapCells.Count)
        //    {
        //        MapCells[index].CellType = MapCellType.Resource;
        //        MapCells[index].DisplayText = process.ToString();
        //        MapCells[index].DisplayColor = new SolidColorBrush(Colors.DarkGreen);

        //        OnPropertyChanged(nameof(MapCells));
        //    }
        //}

        // 更新额外资源
        public void UpdateAdditionResourceCell(int x, int y, string resourceName, int hp) // hp 是 Boss 血量
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Resource; // 仍然是资源类型
                // cell.DisplayText = hp.ToString(); // 显示Boss血量

                // 根据资源类型选择颜色
                if (resourceName.Contains("生命")) // 规则是生命之泉
                {
                    cell.DisplayColor = new SolidColorBrush(Colors.LightPink); // 淡粉色
                }
                else if (resourceName.Contains("狂战") || resourceName.Contains("疯人")) // 规则是狂战士之力
                {
                    cell.DisplayColor = new SolidColorBrush(Colors.OrangeRed); // 橙红色
                }
                else if (resourceName.Contains("疾步") || resourceName.Contains("快步")) // 规则是疾步之灵
                {
                    cell.DisplayColor = new SolidColorBrush(Colors.LightSkyBlue); // 天蓝色
                }
                else if (resourceName.Contains("视野") || resourceName.Contains("广视")) // 规则是视野之灵
                {
                    cell.DisplayColor = new SolidColorBrush(Colors.MediumPurple); // 紫色
                }
                else
                {
                    cell.DisplayColor = new SolidColorBrush(Colors.Purple); // 未知用紫色
                }

                // 更新HP和Tooltip
                // TODO: 获取加成资源Boss的最大HP (可能需要从游戏规则或新消息字段获取)
                int maxHp = GetAdditionResourceMaxHp(resourceName); // 需要实现此辅助方法
                cell.DisplayText = $"{hp}/{maxHp}";
                //cell.ToolTipText = $"类型: {resourceName} (Boss)\n血量: {hp}/{maxHp}";

                // OnPropertyChanged(nameof(MapCells));
            }
        }

        //public void UpdateAdditionResourceCell(int x, int y, string resourceName, int value)
        //{
        //    int index = x * GridSize + y;
        //    if (index >= 0 && index < MapCells.Count)
        //    {
        //        MapCells[index].CellType = MapCellType.Resource;
        //        MapCells[index].DisplayText = value.ToString();

        //        // 根据资源类型选择颜色
        //        if (resourceName.Contains("生命池"))
        //        {
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGreen);
        //        }
        //        else if (resourceName.Contains("疯人"))
        //        {
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.OrangeRed);
        //        }
        //        else
        //        {
        //            MapCells[index].DisplayColor = new SolidColorBrush(Colors.Purple);
        //        }

        //        OnPropertyChanged(nameof(MapCells));
        //    }
        //}

        //private void RefreshCharacters()
        //{
        //    if (characterCanvas == null || viewModel == null) return;

        //    characterCanvas.Children.Clear();
        //    characterElements.Clear();

        //    InitializeCharacters(viewModel.BuddhistsTeamCharacters, Colors.Red);
        //    InitializeCharacters(viewModel.MonstersTeamCharacters, Colors.Blue);
        //}




    // 辅助方法：根据建筑类型获取最大HP(根据游戏规则文档)
    public int GetBuildingMaxHp(string buildingType)
        {
            return buildingType switch
            {
                "兵营" => 600,
                "泉水" => 300,
                "农场" => 400,
                _ => 0 // 其他或未知类型
            };
        }

        // 辅助方法：根据资源名称获取Boss最大HP (根据游戏规则文档)
        // 注意：Boss血量会变化，这里可能需要根据当前游戏阶段判断，暂时用一个代表值
        public int GetAdditionResourceMaxHp(string resourceName)
        {
            // 简化处理，取最大阶段的值或一个固定值
            if (resourceName.Contains("生命")) return 400;
            if (resourceName.Contains("狂战")) return 600;
            if (resourceName.Contains("疾步")) return 300;
            if (resourceName.Contains("视野")) return 300;
            return 0; // 未知
        }

    }
}