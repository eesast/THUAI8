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
                // 修改这里，使用正确的集合名称
                viewModel.BuddhistsTeamCharacters.CollectionChanged -= BuddhistsTeamCharacters_CollectionChanged;
                viewModel.MonstersTeamCharacters.CollectionChanged -= MonstersTeamCharacters_CollectionChanged;
            }

            viewModel = vm;

            // 初始化地图网格
            if (mapGrid != null && viewModel.MapVM != null)
            {
                // 直接使用现有的mapGrid
                MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);
            }

            // 监听角色集合变化
            // 修改这里，使用正确的集合名称
            viewModel.BuddhistsTeamCharacters.CollectionChanged += BuddhistsTeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged += MonstersTeamCharacters_CollectionChanged;

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

            // 修改这里，使用正确的集合名称
            InitializeCharacters(viewModel.BuddhistsTeamCharacters, Colors.Red);
            InitializeCharacters(viewModel.MonstersTeamCharacters, Colors.Blue);
        }

        private void InitializeRandomPositions()
        {
            if (viewModel == null) return;

            Random rnd = new Random();
            // 修改这里，使用正确的集合名称
            foreach (var character in viewModel.BuddhistsTeamCharacters)
            {
                // Only set position if it's still at default (0,0)
                if (character.PosX == 0 && character.PosY == 0)
                {
                    character.PosX = rnd.Next(1, 49);
                    character.PosY = rnd.Next(1, 49);
                }
            }

            // 修改这里，使用正确的集合名称
            foreach (var character in viewModel.MonstersTeamCharacters)
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

            // 使用简化版本的角色创建
            for (int i = 0; i < characters.Count; i++)
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

                // 可选: 添加文本标签
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

        // 修改位置更新方法，接受任何UIElement
        private void UpdateCharacterPosition(Control element, int x, int y)
        {
            // 转换网格位置为像素
            Canvas.SetLeft(element, y * 15);
            Canvas.SetTop(element, x * 15);
        }

        // 修改事件处理程序名称
        private void BuddhistsTeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // When collection changes, refresh all characters for simplicity
            RefreshCharacters();
        }

        private void MonstersTeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // When collection changes, refresh all characters for simplicity
            RefreshCharacters();
        }

        // 辅助方法 - 可能不需要了，但保留以确保没有被引用的代码块
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