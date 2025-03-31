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
            mapGrid = this.FindControl<Grid>("MapGrid"); // �޸������ӦXAML

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

            // ��ʼ����ͼ����
            if (mapGrid != null && viewModel.MapVM != null)
            {
                // ֱ��ʹ�����е�mapGrid
                MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);

            }

            // ������ɫ���ϱ仯
            viewModel.RedTeamCharacters.CollectionChanged += RedTeamCharacters_CollectionChanged;
            viewModel.BlueTeamCharacters.CollectionChanged += BlueTeamCharacters_CollectionChanged;

            // ��ʼ����ɫ
            RefreshCharacters();
            InitializeRandomPositions();

            // ������ͼ��Ԫ��仯�����ģ���ṩ������������
            if (viewModel.MapVM != null)
            {
                // ���MapCell����ʵ����INotifyPropertyChanged��������������������Ա仯
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