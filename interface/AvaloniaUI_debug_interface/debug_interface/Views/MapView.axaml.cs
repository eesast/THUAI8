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

                // ===== 选项1: 显示数字编号 =====
                // 如果不需要数字编号，注释掉下面这段代码
                //var textBlock = new TextBlock
                //{
                //    Text = (i + 1).ToString(), // 使用编号(从1开始)
                //    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                //    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                //    FontSize = 8,
                //    Foreground = new SolidColorBrush(color), // 文本颜色与队伍颜色一致
                //    FontWeight = FontWeight.Bold,
                //};
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
    }
}