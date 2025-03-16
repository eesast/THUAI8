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

namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {
        private Canvas? characterCanvas;
        private ItemsControl? mapItemsControl;
        private Dictionary<string, Ellipse> characterEllipses = new Dictionary<string, Ellipse>();
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
            characterEllipses.Clear();

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

                var ellipse = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(color),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                    Tag = character.Name,
                };

                // Set tooltip
                ToolTip.SetTip(ellipse, character.Name);

                // Set initial position
                Canvas.SetLeft(ellipse, character.PosY * 15);
                Canvas.SetTop(ellipse, character.PosX * 15);

                characterCanvas.Children.Add(ellipse);
                characterEllipses[id] = ellipse;

                // Set up property changed handlers
                character.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CharacterViewModel.PosX) || e.PropertyName == nameof(CharacterViewModel.PosY))
                    {
                        UpdateCharacterPosition(ellipse, character.PosX, character.PosY);
                    }
                };
            }
        }

        private void UpdateCharacterPosition(Ellipse ellipse, int x, int y)
        {
            // Convert grid position to pixels
            Canvas.SetLeft(ellipse, y * 15);
            Canvas.SetTop(ellipse, x * 15);
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