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
            characterElements.Clear(); // ʹ���µ��ֵ�����

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

                // ����һ��Grid��Ϊ�����������߿���ı�/ͼ��
                var grid = new Grid
                {
                    Width = 15,
                    Height = 15,
                };

                // ��������ɫ�߿��Բ��
                var borderellipse = new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = new SolidColorBrush(Colors.White), // ��ɫ����
                    Stroke = new SolidColorBrush(color), // ������ɫ�߿�
                    StrokeThickness = 2,
                    Tag = character.Name,
                };

                grid.Children.Add(borderellipse);

                // ===== ѡ��1: ��ʾ���ֱ�� =====
                // �������Ҫ���ֱ�ţ�ע�͵�������δ���
                //var textBlock = new TextBlock
                //{
                //    Text = (i + 1).ToString(), // ʹ�ñ��(��1��ʼ)
                //    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                //    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                //    FontSize = 8,
                //    Foreground = new SolidColorBrush(color), // �ı���ɫ�������ɫһ��
                //    FontWeight = FontWeight.Bold,
                //};
                //grid.Children.Add(textBlock);


                // ===== ѡ��2: ��ʾ��ɫͼ�� =====
                //�������Ҫͼ�꣬ע�͵�������δ���
                //��ȡ��ɫ���ͻ�ID������ѡ���Ӧ��ͼ��
                //string iconKey = character.Type ?? $"Character{i + 1}"; // ������Ҫ���ģ����������Type����

                //try
                //{
                //    // ������ԴURI������ͼƬ
                //    var uri = new Uri("avares://debug_interface/Assets/tangseng2.png");
                //    using var stream = AssetLoader.Open(uri);
                //    var originalBitmap = new Bitmap(stream);

                //    // ����������ʾͼƬ��Բ��
                //    var imageEllipse = new Ellipse
                //    {
                //        Width = 15,
                //        Height = 15,
                //    };

                //    // ʹ��ImageBrush���Բ��
                //    var imageBrush = new ImageBrush
                //    {
                //        Source = originalBitmap, // ֱ��ʹ��ԭʼBitmap
                //        Stretch = Stretch.UniformToFill,
                //        // ����ͨ����������������"ģ��"�ü�Ч��
                //        // ���磬ֻ��ʾͼ��Ķ���
                //        SourceRect = new RelativeRect(0, 0, 1, 0.7, RelativeUnit.Relative), // ֻ��ʾ����1/3
                //        AlignmentX = AlignmentX.Center,
                //        AlignmentY = AlignmentY.Top
                //    };
                //    imageEllipse.Fill = imageBrush;

                //    // ��ӵ�Grid
                //    grid.Children.Add(imageEllipse);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"ͼƬ�����쳣: {ex.Message}");
                //    Console.WriteLine($"��ջ����: {ex.StackTrace}");
                //    System.Diagnostics.Debug.WriteLine($"ͼƬ�����쳣: {ex.Message}");
                //    System.Diagnostics.Debug.WriteLine($"��ջ����: {ex.StackTrace}");
                //}
                // ===== ѡ��2���� =====

                // ������ʾ��Ϣ
                ToolTip.SetTip(grid, character.Name);

                // ���ó�ʼλ��
                Canvas.SetLeft(grid, character.PosY * 15);
                Canvas.SetTop(grid, character.PosX * 15);

                characterCanvas.Children.Add(grid);

                // �洢Grid���ֵ���
                characterElements[id] = grid;

                // �������Ը��Ĵ�����
                character.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CharacterViewModel.PosX) || e.PropertyName == nameof(CharacterViewModel.PosY))
                    {
                        // ����Grid��λ��
                        UpdateCharacterPosition(grid, character.PosX, character.PosY);
                    }
                };
            }
        }

        // �޸�λ�ø��·����������κ�UIElement
        private void UpdateCharacterPosition(Control element, int x, int y)
        {
            // ת������λ��Ϊ����
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