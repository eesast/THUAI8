// MapHelper.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using debug_interface.Models;
using debug_interface.ViewModels;
using System;
using System.Collections.Generic;

namespace debug_interface.Controls
{
    public static class MapHelper
    {
        // CellRectangles 存储矩形引用
        private static Dictionary<int, Rectangle> cellRectangles = new Dictionary<int, Rectangle>();
        // CellTextBlocks 存储文本引用
        private static Dictionary<int, TextBlock> cellTextBlocks = new Dictionary<int, TextBlock>();
        private static Grid? gridContainer; // Grid 容器引用

        /// <summary>
        /// 初始化地图网格
        /// </summary>
        public static Grid CreateMapGrid(MapViewModel mapViewModel)
        {
            // 清空现有记录
            cellRectangles.Clear();

            // 创建Grid容器，设置为50x50的网格
            var grid = new Grid();
            gridContainer = grid;

            // 定义列
            for (int i = 0; i < 50; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // 定义行
            for (int i = 0; i < 50; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // 绘制单元格
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    int index = i * 50 + j;

                    if (index < mapViewModel.MapCells.Count)
                    {
                        var cell = mapViewModel.MapCells[index];

                        // 创建矩形单元格
                        var rectangle = new Rectangle
                        {
                            Fill = cell.DisplayColor,
                            Margin = new Thickness(0),
                            [Grid.RowProperty] = i,
                            [Grid.ColumnProperty] = j,
                            IsHitTestVisible = true, // 确保它参与命中测试
                            ZIndex = 0 // 确保它在 Z 轴上有明确的层级 (低于 TextBlock 和网格线)
                        };

                        // 为单元格添加文本（如果有）
                        if (!string.IsNullOrEmpty(cell.DisplayText))
                        {
                            var textBlock = new TextBlock
                            {
                                Text = cell.DisplayText,
                                FontSize = 6,
                                TextAlignment = TextAlignment.Center,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                [Grid.RowProperty] = i,
                                [Grid.ColumnProperty] = j,
                                ZIndex = 1 // 确保文本在矩形上方
                            };
                            grid.Children.Add(textBlock);
                        }

                        // 存储矩形引用以便后续更新
                        cellRectangles[index] = rectangle;
                        grid.Children.Add(rectangle);
                    }
                }
            }

            // 添加网格线（在单元格上方）
            //AddGridLines(grid);

            return grid;
        }
        /// <summary>
        /// 初始化地图网格，包括单元格、文本和 Tooltip
        /// </summary>
        public static void InitializeMapGrid(Grid grid, MapViewModel mapViewModel)
        {
            // 清空所有现有内容
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            //cellRectangles.Clear();
            //cellTextBlocks.Clear(); // 清空文本引用
            gridContainer = grid;

            // 定义列和行 (50x50)
            for (int i = 0; i < 50; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            //for (int i = 0; i < 50; i++)
            //{
            //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            //    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            //}

            // 绘制单元格、文本和设置Tooltip
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    int index = i * 50 + j;

                    if (index < mapViewModel.MapCells.Count)
                    {
                        var cell = mapViewModel.MapCells[index];

                        // Create Rectangle
                        var rectangle = new Rectangle
                        {
                            Margin = new Thickness(0),
                            [Grid.RowProperty] = i,
                            [Grid.ColumnProperty] = j,
                            Tag = index,
                            IsHitTestVisible = true,
                            ZIndex = 0
                        };

                        // *** Bind Fill property ***
                        rectangle[!Shape.FillProperty] = new Binding(nameof(MapCell.DisplayColor))
                        {
                            Source = cell,
                            Mode = BindingMode.OneWay
                        };

                        // Create TextBlock
                        var textBlock = new TextBlock
                        {
                            FontSize = 8,
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Foreground = Brushes.Black, // Consider binding this too if needed
                            [Grid.RowProperty] = i,
                            [Grid.ColumnProperty] = j,
                            IsHitTestVisible = false,
                            ZIndex = 1
                        };

                        // *** Bind Text property ***
                        textBlock[!TextBlock.TextProperty] = new Binding(nameof(MapCell.DisplayText))
                        {
                            Source = cell,
                            Mode = BindingMode.OneWay
                        };

                        // Add to Grid
                        grid.Children.Add(rectangle);
                        grid.Children.Add(textBlock);

                        // --- *** REMOVE PropertyChanged Handler Attachment *** ---
                        // cell.PropertyChanged -= Cell_PropertyChanged; // Ensure removed if re-initializing
                        // cell.PropertyChanged += Cell_PropertyChanged; // Remove this line!

                        // Optional: Store refs if still needed, but binding makes them less necessary for updates
                        // cellRectangles[index] = rectangle;
                        // cellTextBlocks[index] = textBlock;
                    }
                }
            }

            // 添加网格线（在单元格和文本上方）
            //AddGridLines(grid);
        }

        /// <summary>
        /// 添加网格线
        /// </summary>
        private static void AddGridLines(Grid grid)
        {
            // 添加水平网格线
            for (int i = 0; i <= 50; i++)
            {
                var line = new Line
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 0),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5, // 可以细一点
                    Stretch = Stretch.Fill,
                    ZIndex = 2 // 确保网格线在最上层
                };

                if (i < 50) // 行线画在单元格底部
                {
                    line.SetValue(Grid.RowProperty, i);
                    line.SetValue(Grid.ColumnSpanProperty, 50);
                    line.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                    grid.Children.Add(line);
                }
                // (可选) 画最后一条边框线
                // else if (i == 50) { ... }
            }


            // 添加垂直网格线
            for (int j = 0; j <= 50; j++)
            {
                var line = new Line
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5, // 可以细一点
                    Stretch = Stretch.Fill,
                    ZIndex = 2 // 确保网格线在最上层
                };

                if (j < 50) // 列线画在单元格右侧
                {
                    line.SetValue(Grid.ColumnProperty, j);
                    line.SetValue(Grid.RowSpanProperty, 50);
                    line.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                    grid.Children.Add(line);
                }
                // (可选) 画最后一条边框线
                // else if (j == 50) { ... }
            }
        }

        // (可选) 辅助方法判断背景色是否需要深色文本
        private static bool NeedsDarkText(IBrush background)
        {
            if (background is SolidColorBrush solidColor)
            {
                var color = solidColor.Color;
                // 基于亮度简单判断 (这是一个简化的亮度计算)
                double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                return luminance > 0.5; // 如果背景亮，用深色文本
            }
            return true; // 默认用深色
        }




        /// <summary>
        /// 更新单元格颜色
        /// </summary>
        //public static void UpdateCellColor(int x, int y, IBrush color)
        //{
        //    int index = x * 50 + y;
        //    if (cellRectangles.ContainsKey(index))
        //    {
        //        cellRectangles[index].Fill = color;
        //    }
        //}

        /// <summary>
        /// 更新单元格文本
        /// </summary>
        //public static void UpdateCellText(int x, int y, string text)
        //{
        //    if (gridContainer == null) return;

        //    // 查找对应位置的TextBlock并更新
        //    foreach (var child in gridContainer.Children)
        //    {
        //        if (child is TextBlock textBlock &&
        //            Grid.GetRow(textBlock) == x &&
        //            Grid.GetColumn(textBlock) == y)
        //        {
        //            textBlock.Text = text;
        //            return;
        //        }
        //    }

        //    // 如果没有找到现有的TextBlock，创建新的
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        var textBlock = new TextBlock
        //        {
        //            Text = text,
        //            FontSize = 6,
        //            TextAlignment = TextAlignment.Center,
        //            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        //            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        //            [Grid.RowProperty] = x,
        //            [Grid.ColumnProperty] = y,
        //            ZIndex = 1
        //        };
        //        gridContainer.Children.Add(textBlock);
        //    }
        //}
    }
}