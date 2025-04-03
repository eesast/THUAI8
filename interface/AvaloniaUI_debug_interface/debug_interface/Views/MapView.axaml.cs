// MapView.axaml.cs - Fixed version
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using debug_interface.Controls;
using debug_interface.Models;
using debug_interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {

        private Canvas? characterCanvas;
        private Grid? mapGrid;
        private Dictionary<string, Control> characterElements = new Dictionary<string, Control>();
        private MainWindowViewModel? viewModel;

        public MapView()
        {
            InitializeComponent();

            this.AttachedToVisualTree += MapView_AttachedToVisualTree;
            this.DataContextChanged += MapView_DataContextChanged;
        }


        private void MapView_DataContextChanged(object? sender, EventArgs e)
        {
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel vm)
            {
                SetupViewModel(vm);
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapGrid = this.FindControl<Grid>("MapGrid"); // 修改这里，对应XAML

            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel vm)
            {
                SetupViewModel(vm);
            }
        }


        private void SetupViewModel(MainWindowViewModel vm)
        {
            if (viewModel != null)
            {
                viewModel.RedTeamCharacters.CollectionChanged -= RedTeamCharacters_CollectionChanged;
                viewModel.BlueTeamCharacters.CollectionChanged -= BlueTeamCharacters_CollectionChanged;
            }

            viewModel = vm;

            // 初始化地图网格
            if (mapGrid != null && viewModel.MapVM != null)
            {
                // 直接使用现有的mapGrid
                MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);

            }

            // 监听角色集合变化
            viewModel.RedTeamCharacters.CollectionChanged += RedTeamCharacters_CollectionChanged;
            viewModel.BlueTeamCharacters.CollectionChanged += BlueTeamCharacters_CollectionChanged;

            // 初始化角色
            RefreshCharacters();
            InitializeRandomPositions();

            // 监听地图单元格变化（如果模型提供了这种能力）
            if (viewModel.MapVM != null)
            {
                // 如果MapCell类型实现了INotifyPropertyChanged，您可以在这里监听属性变化
                foreach (var cell in viewModel.MapVM.MapCells)
                {
                    cell.PropertyChanged += (s, e) => {
                        if (s is MapCell mapCell)
                        {
                            if (e.PropertyName == nameof(MapCell.DisplayColor))
                            {
                                MapHelper.UpdateCellColor(mapCell.CellX, mapCell.CellY, mapCell.DisplayColor);
                            }
                            else if (e.PropertyName == nameof(MapCell.DisplayText))
                            {
                                MapHelper.UpdateCellText(mapCell.CellX, mapCell.CellY, mapCell.DisplayText);
                            }
                        }
                    };
                }
            }
        }

        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;

            characterCanvas.Children.Clear();
            characterElements.Clear();

            InitializeCharacters(viewModel.RedTeamCharacters, Colors.Red);
            InitializeCharacters(viewModel.BlueTeamCharacters, Colors.Blue);
        }

        private void InitializeRandomPositions()
        {
            if (viewModel == null) return;

            Random rnd = new Random();
            foreach (var character in viewModel.RedTeamCharacters)
            {
                // Only set position if it's still at default (0,0)
                if (character.PosX == 0 && character.PosY == 0)
                {
                    character.PosX = rnd.Next(1, 49);
                    character.PosY = rnd.Next(1, 49);
                }
            }

            foreach (var character in viewModel.BlueTeamCharacters)
            {
                // Only set position if it's still at default (0,0)
                if (character.PosX == 0 && character.PosY == 0)
                {
                    character.PosX = rnd.Next(1, 49);
                    character.PosY = rnd.Next(1, 49);
                }
            }
        }


        private void InitializeCharacters<T>(System.Collections.ObjectModel.ObservableCollection<T> characters, Color color) where T : CharacterViewModel
        {
            if (characterCanvas == null) return;

            //bool isRedTeam = color.Equals(Colors.Red);

            //// 形状选择 - 红队使用圆形，蓝队使用正方形
            //for (int i = 0; i < characters.Count; i++)
            //{
            //    var character = characters[i];

            //    // 创建容器 - 用于定位
            //    var container = new Canvas
            //    {
            //        Width = 16,
            //        Height = 16,
            //        Tag = character
            //    };

            //    Control characterShape;

            //    if (isRedTeam)
            //    {
            //        // 红队使用圆形
            //        characterShape = new Ellipse
            //        {
            //            Width = 14,
            //            Height = 14,
            //            Fill = new SolidColorBrush(color) { Opacity = 0.7 },
            //            Stroke = Brushes.White,
            //            StrokeThickness = 1
            //        };
            //    }
            //    else
            //    {
            //        // 蓝队使用正方形
            //        characterShape = new Rectangle
            //        {
            //            Width = 14,
            //            Height = 14,
            //            Fill = new SolidColorBrush(color) { Opacity = 0.7 },
            //            Stroke = Brushes.White,
            //            StrokeThickness = 1
            //        };
            //    }

            //    // 添加形状到容器
            //    Canvas.SetLeft(characterShape, 1);
            //    Canvas.SetTop(characterShape, 1);
            //    container.Children.Add(characterShape);

            //    // 添加标识符 - 使用不同的方式标识队伍内部的角色
            //    var identifier = new TextBlock
            //    {
            //        Text = (i + 1).ToString(),
            //        FontSize = 8,
            //        Foreground = Brushes.White,
            //        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            //        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            //    };

            //    // 为标识符添加背景以增强可读性
            //    var textContainer = new Border
            //    {
            //        Child = identifier,
            //        Width = 14,
            //        Height = 14,
            //        Background = Brushes.Transparent,
            //        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            //        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            //    };

            //    Canvas.SetLeft(textContainer, 1);
            //    Canvas.SetTop(textContainer, 1);
            //    container.Children.Add(textContainer);

            //    // 添加工具提示，显示详细信息
            //    var tooltip = new ToolTip
            //    {
            //        // 使用角色的泛型属性，确保可以访问
            //        Content = new TextBlock { Text = $"{(isRedTeam ? "红队" : "蓝队")} 角色 {i + 1}" }
            //    };
            //    ToolTip.SetTip(container, tooltip);

            //    // 将容器添加到画布
            //    characterCanvas.Children.Add(container);
            //    characterElements[character.Name] = container;

            //    // 初始定位 - 稍后会更新

            //    Canvas.SetLeft(container, i*i * i*i);
            //    Canvas.SetTop(container, i*i * i * i );
            //}
            /////////////////////////////////////
            /////////////////////////////////////
            ////////////////////////////////////
            for (int i = 0; i < characters.Count; i++)
            {
                {
                    var character = characters[i];
                    var id = color == Colors.Red ? $"red_{i}" : $"blue_{i}";

                    // 创建一个Grid作为容器，包含边框和文本/图标
                    var grid = new Grid
                    {
                        Width = 15,
                        Height = 15,
                    };

                    // 创建带颜色边框的圆形
                    var borderellipse = new Ellipse
                    {
                        Width = 15,
                        Height = 15,
                        Fill = new SolidColorBrush(Colors.White), // 白色背景
                        Stroke = new SolidColorBrush(color), // 队伍颜色边框
                        StrokeThickness = 2,
                        Tag = character.Name,
                    };

                    grid.Children.Add(borderellipse);

                    // ===== 选项1: 显示数字编号 =====
                    // 如果不需要数字编号，注释掉下面这段代码
                    var textBlock = new TextBlock
                    {
                        Text = (i + 1).ToString(), // 使用编号(从1开始)
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        FontSize = 8,
                        Foreground = new SolidColorBrush(color), // 文本颜色与队伍颜色一致
                        FontWeight = FontWeight.Bold,
                    };
                    //grid.Children.Add(textBlock);

                    // 设置提示信息
                    ToolTip.SetTip(grid, character.Name);

                    // 设置初始位置
                    Canvas.SetLeft(grid, character.PosY * 15);
                    Canvas.SetTop(grid, character.PosX * 15);

                    characterCanvas.Children.Add(grid);

                    // 存储Grid到字典中
                    characterElements[id] = grid;

                    // 设置属性更改处理器
                    character.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CharacterViewModel.PosX) || e.PropertyName == nameof(CharacterViewModel.PosY))
                        {
                            // 更新Grid的位置
                            UpdateCharacterPosition(grid, character.PosX, character.PosY);
                        }
                    };
                }
            }
        }

        // 修改位置更新方法，接受任何UIElement
        private void UpdateCharacterPosition(Control element, int x, int y)
        {
            // 转换网格位置为像素
            Canvas.SetLeft(element, y * 15);
            Canvas.SetTop(element, x * 15);
        }

       
        private void RedTeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // When collection changes, refresh all characters for simplicity
            RefreshCharacters();
        }

        private void BlueTeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // When collection changes, refresh all characters for simplicity
            RefreshCharacters();
        }


        public void UpdateCharacterPosition(long characterId, int x, int y, bool isRedTeam, string name)
        {
            // 查找现有角色标记或创建新标记
            var marker = FindCharacterMarker(characterId);

            if (marker == null)
            {
                // 创建新角色标记
                marker = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(isRedTeam ? Colors.Red : Colors.Blue),
                    Tag = characterId
                };

                // 添加文本标签
                var label1 = new TextBlock
                {
                    Text = name,
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                // 添加到画布
                CharacterCanvas.Children.Add(marker);
                CharacterCanvas.Children.Add(label1);
            }

            // 更新位置
            double cellWidth = CharacterCanvas.Bounds.Width / 50;
            double cellHeight = CharacterCanvas.Bounds.Height / 50;

            Canvas.SetLeft(marker, y * cellWidth + cellWidth / 2 - marker.Width / 2);
            Canvas.SetTop(marker, x * cellHeight + cellHeight / 2 - marker.Height / 2);

            // 更新标签位置
            var label = FindCharacterLabel(characterId);
            if (label != null)
            {
                Canvas.SetLeft(label, y * cellWidth + cellWidth / 2 - label.Bounds.Width / 2);
                Canvas.SetTop(label, x * cellHeight + cellHeight / 2 + marker.Height);
            }
        }

        private Ellipse FindCharacterMarker(long characterId)
        {
            foreach (var child in CharacterCanvas.Children)
            {
                if (child is Ellipse ellipse && (long?)ellipse.Tag == characterId)
                {
                    return ellipse;
                }
            }
            return null;
        }

        private TextBlock FindCharacterLabel(long characterId)
        {
            var marker = FindCharacterMarker(characterId);
            if (marker == null) return null;

            int index = CharacterCanvas.Children.IndexOf(marker);
            if (index >= 0 && index + 1 < CharacterCanvas.Children.Count &&
                CharacterCanvas.Children[index + 1] is TextBlock label)
            {
                return label;
            }
            return null;
        }
    }
}