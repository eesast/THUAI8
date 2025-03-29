// MapView.axaml.cs - Fixed version
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using debug_interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Media.Imaging;
using Avalonia.Platform;


namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {
        private Canvas? characterCanvas;
        private ItemsControl? mapItemsControl;
        //private Dictionary<string, Ellipse> characterEllipses = new Dictionary<string, Ellipse>();
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
            // When the data context changes, get the parent window's DataContext
            // which should be the MainWindowViewModel
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel vm)
            {
                SetupViewModel(vm);
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapItemsControl = this.FindControl<ItemsControl>("MapItemsControl");

            // Get the MainWindowViewModel from the parent MainWindow
            var mainWindow = this.FindAncestorOfType<MainWindow>();
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel vm)
            {
                SetupViewModel(vm);
            }
        }

        private void SetupViewModel(MainWindowViewModel vm)
        {
            // Clean up previous handlers if any
            if (viewModel != null)
            {
                viewModel.RedTeamCharacters.CollectionChanged -= RedTeamCharacters_CollectionChanged;
                viewModel.BlueTeamCharacters.CollectionChanged -= BlueTeamCharacters_CollectionChanged;
            }

            viewModel = vm;

            // Set the ItemsSource programmatically
            if (mapItemsControl != null && viewModel.MapVM != null)
            {
                mapItemsControl.ItemsSource = viewModel.MapVM.MapCells;
            }

            // Listen for changes to character collections
            viewModel.RedTeamCharacters.CollectionChanged += RedTeamCharacters_CollectionChanged;
            viewModel.BlueTeamCharacters.CollectionChanged += BlueTeamCharacters_CollectionChanged;

            // Initialize existing characters
            RefreshCharacters();

            // Initialize random positions if not set
            InitializeRandomPositions();
        }

        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;

            // Clear existing characters
            characterCanvas.Children.Clear();
            characterElements.Clear(); // 使用新的字典名称

            // Re-add all characters
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


                // ===== 选项2: 显示角色图标 =====
                //如果不需要图标，注释掉下面这段代码
                //获取角色类型或ID，用于选择对应的图标
                //string iconKey = character.Type ?? $"Character{i + 1}"; // 根据需要更改，这里假设有Type属性

                //try
                //{
                //    // 创建资源URI并加载图片
                //    var uri = new Uri("avares://debug_interface/Assets/tangseng2.png");
                //    using var stream = AssetLoader.Open(uri);
                //    var originalBitmap = new Bitmap(stream);

                //    // 创建用于显示图片的圆形
                //    var imageEllipse = new Ellipse
                //    {
                //        Width = 15,
                //        Height = 15,
                //    };

                //    // 使用ImageBrush填充圆形
                //    var imageBrush = new ImageBrush
                //    {
                //        Source = originalBitmap, // 直接使用原始Bitmap
                //        Stretch = Stretch.UniformToFill,
                //        // 可以通过调整以下属性来"模拟"裁剪效果
                //        // 例如，只显示图像的顶部
                //        SourceRect = new RelativeRect(0, 0, 1, 0.7, RelativeUnit.Relative), // 只显示顶部1/3
                //        AlignmentX = AlignmentX.Center,
                //        AlignmentY = AlignmentY.Top
                //    };
                //    imageEllipse.Fill = imageBrush;

                //    // 添加到Grid
                //    grid.Children.Add(imageEllipse);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"图片加载异常: {ex.Message}");
                //    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                //    System.Diagnostics.Debug.WriteLine($"图片加载异常: {ex.Message}");
                //    System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                //}
                // ===== 选项2结束 =====

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