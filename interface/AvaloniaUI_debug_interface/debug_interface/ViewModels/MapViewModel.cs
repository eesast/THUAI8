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
                        CellType = MapCellType.Space,
                        DisplayColor = new SolidColorBrush(Colors.White),
                        DisplayText = ""
                    });
                }
            }
        }

        private IBrush GetTextColorBasedOnBackground(IBrush background)
        {
            if (background is SolidColorBrush solidColor)
            {
                var color = solidColor.Color;
                // 简单的亮度计算 (YIQ 公式近似)
                double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
                // 如果背景亮度大于 0.5 (亮色)，用黑色文本，否则用白色文本
                return luminance > 0.2 ? Brushes.Black : Brushes.White;
            }
            // 对于非纯色背景，默认返回黑色
            return Brushes.Black;
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
            var cell = MapCells[index];

            // 重置非动态元素的文本
            bool isBaseTerrainOrHome = placeType == PlaceType.Space ||
                                       placeType == PlaceType.Barrier ||
                                       placeType == PlaceType.Bush ||
                                       placeType == PlaceType.Home;

            if (isBaseTerrainOrHome)
            {
                cell.DisplayText = ""; // 重置文本
            }

            // 根据地形类型设置单元格属性和 *初始* 颜色
            switch (placeType)
            {
                case PlaceType.Home: // 家园/出生点 (也是一种建筑区域)
                    cell.CellType = MapCellType.Home; // 可以用特定的 Home 类型
                    cell.DisplayColor = new SolidColorBrush(Colors.Cyan); // 家园用青色
                    break;
                case PlaceType.Space:
                    cell.CellType = MapCellType.Space;
                    cell.DisplayColor = new SolidColorBrush(Colors.White);
                    break;
                case PlaceType.Barrier:
                    cell.CellType = MapCellType.Barrier;

                    cell.DisplayColor = new SolidColorBrush(Colors.DarkGray);
                    break;
                case PlaceType.Bush:
                    cell.CellType = MapCellType.Bush;
                    cell.DisplayColor =  new SolidColorBrush(Colors.LightGreen);
                    break;
                case PlaceType.Construction: // 通用建筑点位
                    cell.CellType = MapCellType.Construction;
                    cell.DisplayColor = new SolidColorBrush(Colors.Brown); // 初始显示为棕色，会被 UpdateBuildingCell 覆盖
                    cell.DisplayText = "建"; // 清空文本，等待 UpdateBuildingCell 设置HP
                    break;
                case PlaceType.EconomyResource:
                    cell.CellType = MapCellType.Economic_Resource;
                    cell.DisplayColor = new SolidColorBrush(Colors.Gold); // 经济资源固定金色
                    cell.DisplayText = "ER"; // 清空文本，等待 UpdateResourceCell 设置量
                    break;
                case PlaceType.AdditionResource:
                    cell.CellType = MapCellType.Additional_Resource;
                    cell.DisplayColor = new SolidColorBrush(Colors.Purple); // 加成资源初始紫色，会被 UpdateAdditionResourceCell 覆盖
                    cell.DisplayText = "AR"; // 清空文本，等待 UpdateAdditionResourceCell 设置HP
                    break;
                case PlaceType.Trap:
                    cell.CellType = MapCellType.Trap;
                    cell.DisplayColor = new SolidColorBrush(Colors.LightGray); // 陷阱初始灰色，会被 UpdateTrapCell 覆盖
                    cell.DisplayText = "Trap"; // 陷阱通常无文本
                    break;
                default: // 未知类型
                    cell.CellType = MapCellType.Space; // 视为 Space
                    cell.DisplayColor = new SolidColorBrush(Colors.Gainsboro); // 用更浅的灰色标记未知
                    cell.DisplayText = "?";
                    break;
            }
            cell.ForegroundColor = GetTextColorBasedOnBackground(cell.DisplayColor);
        }




        // 更新建筑
        public void UpdateBuildingCell(int x, int y, string team, string buildingType, int hp)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                //myLogger?.LogDebug($"--- UpdateBuildingCell called for ({x},{y}), Type: {buildingType}, HP: {hp} ---");
                var cell = MapCells[index];

                // *** 仅更新颜色和文本，不改变 CellType ***
                // CellType 应该已由 UpdateCellType 根据 MapMessage 设置为 Construction 或 Home

                // 设置队伍颜色 (假设 Team 0=取经队=红色系, Team 1=妖怪队=蓝色系)
                cell.DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.DarkRed)  // 取经队建筑颜色
                    : new SolidColorBrush(Colors.DarkBlue); // 妖怪队建筑颜色

                // 设置血量文本


                //int maxHp = GetBuildingMaxHp(buildingType);
                cell.DisplayText = $"{hp}";
                cell.ForegroundColor = GetTextColorBasedOnBackground(cell.DisplayColor);
                myLogger?.LogDebug($"UpdateBuildingCell at ({x},{y}): Set DisplayText to '{cell.DisplayText}', Foreground to '{cell.ForegroundColor}'");
                //myLogger?.LogInfo($"UpdateBuildingCell at ({x},{y}): Set DisplayText to '{cell.DisplayText}'"); // *** 添加日志 ***
                
            }
        }



        // 更新陷阱
        public void UpdateTrapCell(int x, int y, string team, string trapType)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Trap; // 使用新的类型
                //cell.DisplayText = trapType.Substring(0, 1);
                // 根据队伍和陷阱类型设置颜色
                if (team == "取经队")
                {
                    if (trapType.Contains("坑洞")) // 或者 trapType == "陷阱(坑洞)"
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.IndianRed); // 取经队坑洞颜色
                    }
                    else if (trapType.Contains("牢笼")) // 或者 trapType == "陷阱(牢笼)"
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.Tomato); // 为取经队牢笼选择一个新颜色，例如 Tomato
                    }
                    else
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.DarkSalmon); // 默认取经队陷阱颜色
                    }
                }
                else // 妖怪队
                {
                    if (trapType.Contains("坑洞"))
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.CornflowerBlue); // 妖怪队坑洞颜色
                    }
                    else if (trapType.Contains("牢笼"))
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.SteelBlue); // 为妖怪队牢笼选择一个新颜色，例如 SteelBlue
                    }
                    else
                    {
                        cell.DisplayColor = new SolidColorBrush(Colors.LightSteelBlue); // 默认妖怪队陷阱颜色
                    }
                }

                // 更新Tooltip (陷阱没有血量)
                cell.DisplayText = ""; // 陷阱通常不显示血量文本
                //cell.ToolTipText = $"类型: {trapType}\n队伍: {team}";

                cell.ForegroundColor = GetTextColorBasedOnBackground(cell.DisplayColor);

            }
        }



        // 更新资源
        public void UpdateResourceCell(int x, int y, string resourceType, int process) // process 是采集进度/剩余量
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Economic_Resource;
                cell.DisplayText = process.ToString(); // 显示剩余量
                cell.DisplayColor = new SolidColorBrush(Colors.Gold); // 经济资源金色

                // 更新HP和Tooltip
                int maxResource = 10000; // 根据规则，经济资源上限1w


                cell.ForegroundColor = GetTextColorBasedOnBackground(cell.DisplayColor);
                // OnPropertyChanged(nameof(MapCells));
            }
        }


        // 更新额外资源
        public void UpdateAdditionResourceCell(int x, int y, string resourceName, int hp) // hp 是 Boss 血量
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                var cell = MapCells[index];
                cell.CellType = MapCellType.Additional_Resource; // 仍然是资源类型
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
                //int maxHp = GetAdditionResourceMaxHp(resourceName); // 需要实现此辅助方法
                cell.DisplayText = $"{hp}";
                //cell.ToolTipText = $"类型: {resourceName} (Boss)\n血量: {hp}/{maxHp}";

                // OnPropertyChanged(nameof(MapCells));
            }
        }



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