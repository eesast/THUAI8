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

            //bool isRedTeam = color.Equals(Colors.Red);

            //// ��״ѡ�� - ���ʹ��Բ�Σ�����ʹ��������
            //for (int i = 0; i < characters.Count; i++)
            //{
            //    var character = characters[i];

            //    // �������� - ���ڶ�λ
            //    var container = new Canvas
            //    {
            //        Width = 16,
            //        Height = 16,
            //        Tag = character
            //    };

            //    Control characterShape;

            //    if (isRedTeam)
            //    {
            //        // ���ʹ��Բ��
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
            //        // ����ʹ��������
            //        characterShape = new Rectangle
            //        {
            //            Width = 14,
            //            Height = 14,
            //            Fill = new SolidColorBrush(color) { Opacity = 0.7 },
            //            Stroke = Brushes.White,
            //            StrokeThickness = 1
            //        };
            //    }

            //    // �����״������
            //    Canvas.SetLeft(characterShape, 1);
            //    Canvas.SetTop(characterShape, 1);
            //    container.Children.Add(characterShape);

            //    // ��ӱ�ʶ�� - ʹ�ò�ͬ�ķ�ʽ��ʶ�����ڲ��Ľ�ɫ
            //    var identifier = new TextBlock
            //    {
            //        Text = (i + 1).ToString(),
            //        FontSize = 8,
            //        Foreground = Brushes.White,
            //        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            //        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            //    };

            //    // Ϊ��ʶ����ӱ�������ǿ�ɶ���
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

            //    // ��ӹ�����ʾ����ʾ��ϸ��Ϣ
            //    var tooltip = new ToolTip
            //    {
            //        // ʹ�ý�ɫ�ķ������ԣ�ȷ�����Է���
            //        Content = new TextBlock { Text = $"{(isRedTeam ? "���" : "����")} ��ɫ {i + 1}" }
            //    };
            //    ToolTip.SetTip(container, tooltip);

            //    // ��������ӵ�����
            //    characterCanvas.Children.Add(container);
            //    characterElements[character.Name] = container;

            //    // ��ʼ��λ - �Ժ�����

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
                    var textBlock = new TextBlock
                    {
                        Text = (i + 1).ToString(), // ʹ�ñ��(��1��ʼ)
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        FontSize = 8,
                        Foreground = new SolidColorBrush(color), // �ı���ɫ�������ɫһ��
                        FontWeight = FontWeight.Bold,
                    };
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


        public void UpdateCharacterPosition(long characterId, int x, int y, bool isRedTeam, string name)
        {
            // �������н�ɫ��ǻ򴴽��±��
            var marker = FindCharacterMarker(characterId);

            if (marker == null)
            {
                // �����½�ɫ���
                marker = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(isRedTeam ? Colors.Red : Colors.Blue),
                    Tag = characterId
                };

                // ����ı���ǩ
                var label1 = new TextBlock
                {
                    Text = name,
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                // ��ӵ�����
                CharacterCanvas.Children.Add(marker);
                CharacterCanvas.Children.Add(label1);
            }

            // ����λ��
            double cellWidth = CharacterCanvas.Bounds.Width / 50;
            double cellHeight = CharacterCanvas.Bounds.Height / 50;

            Canvas.SetLeft(marker, y * cellWidth + cellWidth / 2 - marker.Width / 2);
            Canvas.SetTop(marker, x * cellHeight + cellHeight / 2 - marker.Height / 2);

            // ���±�ǩλ��
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