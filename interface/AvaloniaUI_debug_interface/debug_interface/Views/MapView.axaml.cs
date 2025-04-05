// MapView.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;

using debug_interface.Controls; // MapHelper
using debug_interface.Models;
using debug_interface.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq; // For Linq
using System.Reactive.Linq;
using System.Reactive.Subjects;



namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {
        private Canvas? characterCanvas;
        private Grid? mapGrid;
        private Dictionary<long, Control> characterElements = new Dictionary<long, Control>();
        private MainWindowViewModel? viewModel;
        private bool isMapInitialized = false; // 添加一个标志位

        //public MapView()
        //{
        //    InitializeComponent();
        //    this.AttachedToVisualTree += MapView_AttachedToVisualTree;
        //    this.DataContextChanged += MapView_DataContextChanged;
        //    // 监听 Canvas 尺寸变化以更新缩放因子
        //    // Use a lambda expression here:
        //    this.GetObservable(BoundsProperty).Subscribe(bounds => UpdateCharacterScaling((Rect)bounds)); // Pass bounds explicitly

        //}

        public MapView()
        {
            InitializeComponent();
            this.AttachedToVisualTree += MapView_AttachedToVisualTree;
            this.DataContextChanged += MapView_DataContextChanged;

            BoundsProperty.Changed
                .Where(args => args.Sender == this)
                .Select(args => args.NewValue.GetValueOrDefault()) // 返回 Rect
                .Subscribe(newBounds => // newBounds 是 Rect 类型
                {
                    // 直接使用 newBounds，不再需要检查 HasValue 或访问 Value
                    // Rect currentBounds = newBounds; // 这行是多余的

                    // 使用之前的 IsEmpty 替代方案
                    bool isEffectivelyEmpty = newBounds.Width <= 0 || newBounds.Height <= 0;

                    if (isEffectivelyEmpty)
                    {
                        // Console.WriteLine("Bounds are effectively empty, skipping scaling.");
                    }
                    else
                    {
                        // 直接传递 newBounds
                        Dispatcher.UIThread.InvokeAsync(() => UpdateCharacterScaling(newBounds));
                    }
                });
        }

        // 当 Canvas 尺寸变化时，重新计算所有角色的位置
        private void UpdateCharacterScaling(Rect bounds)
        {
            // if (viewModel == null || characterCanvas == null || bounds.IsEmpty == true) return; // 暂时移除检查
            if (viewModel == null || characterCanvas == null) return; // 保留这两个检查

            // 这里的代码现在保证在 UI 线程执行
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                UpdateCharacterVisual(character); // 重用更新逻辑来重新定位
            }
        }

        private void MapView_DataContextChanged(object? sender, EventArgs e)
        {
            // 当 DataContext 变化时，尝试设置 ViewModel 并初始化地图
            if (this.DataContext is MainWindowViewModel vm)
            {
                viewModel = vm; // 获取 ViewModel
                TryInitializeMap(); // 尝试初始化
            }
            else
            {
                // DataContext 被清空，清理资源
                CleanupViewModel();
                viewModel = null;
                isMapInitialized = false; // 重置标志位
                mapGrid?.Children.Clear(); // 清空地图Grid内容
                characterCanvas?.Children.Clear(); // 清空角色Canvas内容
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // 当附加到可视化树时，查找控件并尝试初始化地图
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapGrid = this.FindControl<Grid>("MapGrid");
            TryInitializeMap(); // 尝试初始化
        }


        // 尝试初始化地图和角色（核心逻辑）
        private void TryInitializeMap()
        {
            // 只有当 ViewModel 和 MapGrid 都准备好，并且地图尚未初始化时才执行
            if (viewModel != null && mapGrid != null && !isMapInitialized)
            {
                // 检查 MapVM 是否也准备好了
                if (viewModel.MapVM != null)
                {
                    Console.WriteLine("MapView: Initializing Map Grid and Characters..."); // 添加日志
                    CleanupViewModel(); // 清理旧的订阅（如果之前有的话）

                    // 设置 ViewModel 的事件监听等
                    SetupViewModelSubscriptions();

                    // 初始化地图 Grid
                    MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);

                    // 初始化角色 Canvas
                    RefreshCharacters();

                    isMapInitialized = true; // 标记地图已初始化
                }
                else
                {
                    Console.WriteLine("MapView: ViewModel is ready, but MapVM is null."); // 日志
                }
            }
            // else
            // {
            // 可以加日志说明为什么没初始化
            // Console.WriteLine($"MapView: Initialize skipped. ViewModel: {viewModel != null}, MapGrid: {mapGrid != null}, Initialized: {isMapInitialized}");
            // }
        }
        private void CleanupViewModel()
        {
            if (viewModel != null)
            {
                // 取消之前的集合和属性变更订阅
                viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
                {
                    character.PropertyChanged -= Character_PropertyChanged;
                }
                // 注意：这里不清空 viewModel 变量，DataContextChanged 会处理
            }
            // characterElements 和 characterCanvas 的清理移到 RefreshCharacters 或 DataContext 被清空时
            // characterElements.Clear();
            // characterCanvas?.Children.Clear();
        }

        private void SetupViewModel(MainWindowViewModel vm)
        {
            // 防止重复设置或处理旧 ViewModel
            if (vm == viewModel) return;

            CleanupViewModel(); // 清理旧的 ViewModel 订阅

            viewModel = vm;

            // 初始化地图网格 (如果尚未初始化或 ViewModel 改变)
            if (viewModel.MapVM != null)
            {
                if (mapGrid != null) MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);
            }

            // 监听角色集合变化 (添加/删除 - 虽然我们预实例化了，但以防万一)
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // 初始化角色 UI 元素并监听属性变化
            RefreshCharacters();
            // InitializeRandomPositions(); // 不再需要随机位置，依赖 ViewModel 的 PosX/PosY

            // 地图单元格的 PropertyChanged 已在 MapHelper 中处理
        }

        // 设置与 ViewModel 相关的订阅
        private void SetupViewModelSubscriptions()
        {
            if (viewModel == null) return;

            // 监听角色集合变化
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // 监听所有现有角色的属性变化
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                // 确保只添加一次监听器
                character.PropertyChanged -= Character_PropertyChanged; // 先移除，防止重复添加
                character.PropertyChanged += Character_PropertyChanged;
            }
        }

        private void TeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 集合发生变化时（理论上不应频繁发生，因为是占位符），刷新整个角色UI
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(RefreshCharacters);
        }


        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;

            // 清理旧的 UI 元素和事件监听器
            foreach (var kvp in characterElements)
            {
                characterCanvas.Children.Remove(kvp.Value);
                // 理论上应该在 CleanupViewModel 中统一处理旧 ViewModel 的事件注销
            }
            characterElements.Clear();

            // 为 ViewModel 中的所有角色（包括占位符）创建或更新 UI 元素
            InitializeTeamCharacters(viewModel.BuddhistsTeamCharacters, Colors.Red);
            InitializeTeamCharacters(viewModel.MonstersTeamCharacters, Colors.Blue);

            // 更新缩放以确保初始位置正确
            UpdateCharacterScaling(this.Bounds);
        }

        // 重命名方法以反映其处理单个队伍
        private void InitializeTeamCharacters(System.Collections.ObjectModel.ObservableCollection<CharacterViewModel> characters, Color teamColor)
        {
            if (characterCanvas == null || viewModel == null) return;

            foreach (var character in characters)
            {
                // 如果该角色的 UI 元素已存在，跳过创建，只需确保事件监听器已附加
                if (characterElements.ContainsKey(character.CharacterId)) continue;

                // 创建角色视觉元素 (例如一个 Grid 包含 Ellipse 和 TextBlock)
                var characterVisual = CreateCharacterVisual(character, teamColor);

                // 添加到 Canvas
                characterCanvas.Children.Add(characterVisual);
                characterElements[character.CharacterId] = characterVisual; // 存入字典

                // 监听角色属性变化
                character.PropertyChanged += Character_PropertyChanged;

                // 设置初始位置和可见性
                UpdateCharacterVisual(character);
            }
        }

        // 创建单个角色的视觉元素
        private Control CreateCharacterVisual(CharacterViewModel character, Color teamColor)
        {
            var grid = new Grid
            {
                Width = 12, // 调整大小
                Height = 12,
                Tag = character.CharacterId // 存储 ID 以便查找
            };

            var ellipse = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(teamColor),
                StrokeThickness = 2,
            };
            grid.Children.Add(ellipse);

            // 可以添加一个小的 TextBlock 显示编号或类型首字母
            var textBlock = new TextBlock
            {
                //Text = character.CharacterId.ToString(), // 或者用名字首字母
                Text = GetCharacterInitial(character.Name),
                FontSize = 7,
                Foreground = new SolidColorBrush(teamColor),
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            grid.Children.Add(textBlock);


            ToolTip.SetTip(grid, $"{character.Name} (ID: {character.CharacterId})\nHP: {character.Hp}\n状态: {character.ActiveState}"); // 初始 Tooltip

            return grid;
        }

        // 获取角色名称首字母或标识符
        private string GetCharacterInitial(string name)
        {
            if (string.IsNullOrEmpty(name) || name.EndsWith("?")) return "?";
            if (name == "唐僧") return "T";
            if (name == "孙悟空") return "S";
            if (name == "猪八戒") return "Z";
            if (name == "沙悟净") return "W";
            if (name == "白龙马") return "B";
            if (name == "猴子猴孙") return "h";
            if (name == "九头元圣") return "J";
            if (name == "红孩儿") return "H";
            if (name == "牛魔王") return "N";
            if (name == "铁扇公主") return "F";
            if (name == "蜘蛛精") return "P";
            if (name == "无名小妖") return "y";
            return name.Length > 0 ? name.Substring(0, 1) : "?";
        }


        // 角色属性变化时的处理
        private void Character_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is CharacterViewModel character)
            {
                // 在 UI 线程上更新视觉元素
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => UpdateCharacterVisual(character));
            }
        }

        // 更新单个角色的视觉状态（位置、可见性、Tooltip）
        private void UpdateCharacterVisual(CharacterViewModel character)
        {
            if (characterElements.TryGetValue(character.CharacterId, out var element))
            {
                // 更新 Tooltip
                ToolTip.SetTip(element, $"{character.Name} (ID: {character.CharacterId})\nHP: {character.Hp}\n状态: {character.ActiveState}\n位置: ({character.PosX},{character.PosY})");

                // 检查角色是否有效且应显示在地图上
                bool shouldBeVisible = character.Hp > 0 && // 活着
                                      !character.Name.EndsWith("?") && // 不是纯占位符
                                      !character.PassiveStates.Contains("已死亡") && // 没有死亡状态
                                      character.PosX >= 0 && character.PosX < 50 && // 在地图网格内
                                      character.PosY >= 0 && character.PosY < 50;

                element.IsVisible = shouldBeVisible;

                if (shouldBeVisible)
                {
                    // 计算缩放后的像素位置
                    double cellWidth = characterCanvas?.Bounds.Width / 50.0 ?? 0;
                    double cellHeight = characterCanvas?.Bounds.Height / 50.0 ?? 0;
                    double left = character.PosY * cellWidth + (cellWidth / 2.0) - (element.Bounds.Width / 2.0);
                    double top = character.PosX * cellHeight + (cellHeight / 2.0) - (element.Bounds.Height / 2.0);

                    Canvas.SetLeft(element, left);
                    Canvas.SetTop(element, top);
                }
            }
        }



        // --- 旧的或不再需要的方法 ---
        // private void InitializeRandomPositions() { ... } // 不需要了
        // private void InitializeCharacters<T>(...) { ... } // 已被 InitializeTeamCharacters 替代
        // private void UpdateCharacterPosition(Control element, int x, int y) { ... } // 已合并到 UpdateCharacterVisual
        // public void UpdateCharacterPosition(long characterId, int x, int y, bool isRedTeam, string name) { ... } // 不再需要，由 ViewModel 驱动
        // private Ellipse FindCharacterMarker(long characterId) { ... } // 不再需要
        // private TextBlock FindCharacterLabel(long characterId) { ... } // 不再需要

    }
}