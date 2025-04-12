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
        /// 初始化地图网格，包括单元格、文本和 Tooltip
        /// </summary>
        public static void InitializeMapGrid(Grid grid, MapViewModel mapViewModel)
        {
            // 清空所有现有内容
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            // gridContainer = grid; // 如果 gridContainer 还有其他用处，保留此行

            // 定义列和行 (50x50)
            for (int i = 0; i < 50; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // 绘制单元格、文本并设置绑定
            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    int index = i * 50 + j;

                    if (index < mapViewModel.MapCells.Count)
                    {
                        var cell = mapViewModel.MapCells[index];

                        // 创建矩形 (Rectangle)
                        var rectangle = new Rectangle
                        {
                            Margin = new Thickness(0),
                            [Grid.RowProperty] = i,
                            [Grid.ColumnProperty] = j,
                            // Tag = index, // Tag 可能不再需要，因为我们依赖绑定
                            IsHitTestVisible = true, // 允许命中测试（如果需要 Tooltip 或点击）
                            ZIndex = 0 // 在文本下方
                        };

                        // *** 绑定 Fill 属性到 MapCell.DisplayColor ***
                        rectangle[!Shape.FillProperty] = new Binding(nameof(MapCell.DisplayColor))
                        {
                            Source = cell,
                            Mode = BindingMode.OneWay // 通常颜色是单向绑定
                        };

                        // 创建文本块 (TextBlock)
                        var textBlock = new TextBlock
                        {
                            FontSize = 8, // 或者你测试后确定的最佳字号
                            // Foreground = Brushes.Black, // 可以设置默认色，或绑定颜色
                            Foreground = GetTextColorBasedOnBackground(cell.DisplayColor), // *** 尝试动态文本颜色 ***
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            [Grid.RowProperty] = i,
                            [Grid.ColumnProperty] = j,
                            IsHitTestVisible = false, // 文本块本身通常不需要交互
                            ZIndex = 1 // 在矩形上方
                        };

                        // *** 绑定 Text 属性到 MapCell.DisplayText ***
                        textBlock[!TextBlock.TextProperty] = new Binding(nameof(MapCell.DisplayText))
                        {
                            Source = cell,
                            Mode = BindingMode.OneWay // 文本通常也是单向绑定
                        };

                        // *** (可选) 绑定文本颜色到背景色，通过转换器决定用深色还是浅色 ***
                        // 需要创建一个 IValueConverter (e.g., BackgroundToForegroundConverter)
                        // textBlock[!TextBlock.ForegroundProperty] = new Binding(nameof(MapCell.DisplayColor))
                        // {
                        //     Source = cell,
                        //     Mode = BindingMode.OneWay,
                        //     Converter = (IValueConverter)Application.Current.FindResource("BackgroundToForegroundConverter") // 假设转换器已在 App.axaml 定义
                        // };


                        // 添加到 Grid
                        grid.Children.Add(rectangle);
                        grid.Children.Add(textBlock);
                    }
                }
            }

            // 添加网格线（如果需要）
            AddGridLines(grid);
        }

        // *** 新增：根据背景色决定文本颜色的辅助方法 ***
        private static IBrush GetTextColorBasedOnBackground(IBrush background)
        {
            if (background is SolidColorBrush solidColor)
            {
                var color = solidColor.Color;
                // 简单的亮度计算 (YIQ 公式近似)
                double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
                // 如果背景亮度大于 0.5 (亮色)，用黑色文本，否则用白色文本
                return luminance > 0.5 ? Brushes.Black : Brushes.White;
            }
            // 对于非纯色背景，默认返回黑色
            return Brushes.Black;
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






       
    }
}