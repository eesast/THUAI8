using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Avalonia.Media;
using Avalonia.Threading;

using Grpc.Core;

using installer.Data;
using installer.Model;

using debug_interface.Models;
using System.Collections.Generic;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Google.Protobuf;
using System.Linq; // 解决Concat错误
using Protobuf;

namespace debug_interface.ViewModels
{
    public partial class MapViewModel : ViewModelBase
    {
        private const int GridSize = 50;

        [ObservableProperty]
        private ObservableCollection<MapCell> mapCells = new();

        // 添加缺失的字段
        private Canvas? characterCanvas;
        private Dictionary<string, Grid> characterElements = new Dictionary<string, Grid>();
        private MainWindowViewModel? viewModel;

        public MapViewModel()
        {
            // 初始化地图单元格
            InitializeMapCells();
        }

        // 设置Canvas和ViewModel引用的方法
        public void SetCanvasAndViewModel(Canvas canvas, MainWindowViewModel vm)
        {
            characterCanvas = canvas;
            viewModel = vm;
            RefreshCharacters();
        }

        // 更新角色位置的方法
        private void UpdateCharacterPosition(Grid element, int x, int y)
        {
            if (characterCanvas == null) return;

            // 更新位置
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
        }

        private void InitializeCharacters(IEnumerable<CharacterViewModel> characters, Color color)
        {
            if (characterCanvas == null || viewModel == null) return;

            foreach (var character in characters)
            {
                var id = $"{color.ToString()}_{character.Name}";
                if (!characterElements.ContainsKey(id))
                {
                    var grid = new Grid { Width = 15, Height = 15 };
                    var ellipse = new Ellipse
                    {
                        Width = 15,
                        Height = 15,
                        Fill = new SolidColorBrush(Colors.White),
                        Stroke = new SolidColorBrush(color),
                        StrokeThickness = 2,
                        Tag = character.Name
                    };
                    grid.Children.Add(ellipse);
                    ToolTip.SetTip(grid, character.Name);
                    characterCanvas.Children.Add(grid);
                    characterElements[id] = grid;
                }

                var element = characterElements[id];
                UpdateCharacterPosition(element, character.PosX, character.PosY);

                character.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CharacterViewModel.PosX) || e.PropertyName == nameof(CharacterViewModel.PosY))
                    {
                        UpdateCharacterPosition(element, character.PosX, character.PosY);
                    }
                };
            }
        }

        //初始化地图单元格
        public void InitializeMapCells()
        {
            MapCells.Clear();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    MapCells.Add(new MapCell
                    {
                        CellX = i,
                        CellY = j,
                        CellType = MapCellType.OpenLand,
                        DisplayColor = new SolidColorBrush(Colors.White),
                        DisplayText = ""
                    });
                }
            }
        }

        // 更新整个地图
        public void UpdateMap(MessageOfMap mapMessage)
        {
            for (int i = 0; i < mapMessage.Rows.Count && i < GridSize; i++)
            {
                for (int j = 0; j < mapMessage.Rows[i].Cols.Count && j < GridSize; j++)
                {
                    int index = i * GridSize + j;
                    if (index < MapCells.Count)
                    {
                        UpdateCellType(i, j, mapMessage.Rows[i].Cols[j]);
                    }
                }
            }
        }

        private void UpdateCellType(int x, int y, PlaceType placeType)
        {
            int index = x * GridSize + y;
            if (index >= MapCells.Count) return;

            // 根据地形类型设置单元格属性
            switch (placeType)
            {
                case PlaceType.Home:
                    MapCells[index].CellType = MapCellType.Building;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Gold);
                    break;
                case PlaceType.Space:
                    MapCells[index].CellType = MapCellType.OpenLand;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.White);
                    break;
                case PlaceType.Barrier:
                    MapCells[index].CellType = MapCellType.Obstacle;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.DarkGray);
                    break;
                case PlaceType.Bush:
                    MapCells[index].CellType = MapCellType.Grass;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGreen);
                    break;
                case PlaceType.EconomyResource:
                    MapCells[index].CellType = MapCellType.Resource;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Gold);
                    break;
                case PlaceType.AdditionResource:
                    MapCells[index].CellType = MapCellType.Resource;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Purple);
                    break;
                case PlaceType.Construction:
                    MapCells[index].CellType = MapCellType.Building;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Brown);
                    break;
                case PlaceType.Trap:
                    MapCells[index].CellType = MapCellType.Obstacle;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    MapCells[index].CellType = MapCellType.OpenLand;
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGray);
                    break;
            }
        }

        public void UpdateCharacterPositions(IEnumerable<CharacterViewModel> buddhistsTeam, IEnumerable<CharacterViewModel> monstersTeam)
        {
            // 使用System.Linq的Concat扩展方法
            var allCharacters = buddhistsTeam.Concat(monstersTeam);

            foreach (var character in allCharacters)
            {
                int index = (int)(character.PosX * GridSize + character.PosY);
                if (index >= 0 && index < MapCells.Count)
                {
                    MapCells[index].DisplayText = character.Name;
                }
            }
        }

        // 更新建筑
        public void UpdateBuildingCell(int x, int y, string team, string buildingType, int hp)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Building;
                MapCells[index].DisplayText = buildingType.Substring(0, 1);
                MapCells[index].DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.DarkRed)
                    : new SolidColorBrush(Colors.DarkBlue);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新陷阱
        public void UpdateTrapCell(int x, int y, string team, string trapType)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Obstacle;
                MapCells[index].DisplayText = trapType.Substring(0, 1);
                MapCells[index].DisplayColor = team == "取经队"
                    ? new SolidColorBrush(Colors.IndianRed)
                    : new SolidColorBrush(Colors.CornflowerBlue);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新资源
        public void UpdateResourceCell(int x, int y, string resourceType, int process)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Resource;
                MapCells[index].DisplayText = process.ToString();
                MapCells[index].DisplayColor = new SolidColorBrush(Colors.DarkGreen);

                OnPropertyChanged(nameof(MapCells));
            }
        }

        // 更新额外资源
        public void UpdateAdditionResourceCell(int x, int y, string resourceName, int value)
        {
            int index = x * GridSize + y;
            if (index >= 0 && index < MapCells.Count)
            {
                MapCells[index].CellType = MapCellType.Resource;
                MapCells[index].DisplayText = value.ToString();

                // 根据资源类型选择颜色
                if (resourceName.Contains("生命池"))
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.LightGreen);
                }
                else if (resourceName.Contains("疯人"))
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.OrangeRed);
                }
                else
                {
                    MapCells[index].DisplayColor = new SolidColorBrush(Colors.Purple);
                }

                OnPropertyChanged(nameof(MapCells));
            }
        }

        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;

            characterCanvas.Children.Clear();
            characterElements.Clear();

            InitializeCharacters(viewModel.BuddhistsTeamCharacters, Colors.Red);
            InitializeCharacters(viewModel.MonstersTeamCharacters, Colors.Blue);
        }
    }
}