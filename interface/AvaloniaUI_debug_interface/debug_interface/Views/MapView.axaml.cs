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
using installer.Model;
using System.ComponentModel;



namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {
        private Canvas? characterCanvas;
        private Grid? mapGrid;
        private Dictionary<long, Control> characterElements = new Dictionary<long, Control>(); // Key 是 Guid
        private MainWindowViewModel? viewModel;
        private bool isMapInitialized = false;
        private IDisposable? _boundsSubscription;
        private Logger? myLogger; // Logger 引用


        public MapView()
        {
            InitializeComponent();
            // *** 确保 Canvas 背景色便于观察 ***
            var canvas = this.FindControl<Canvas>("CharacterCanvas");
            if (canvas != null)
            {
                 //canvas.Background = Brushes.LightPink; // 取消或保留此行以观察 Canvas 区域
                // *** 添加 ZIndex ***
                canvas.SetValue(Panel.ZIndexProperty, 1);
            }

            this.DataContextChanged += MapView_DataContextChanged;
            this.AttachedToVisualTree += MapView_AttachedToVisualTree;
            this.DataContextChanged += (s, e) => { /* ... Logger 获取逻辑 ... */ };
        }


        // 当 Canvas 尺寸变化时，重新计算所有角色的位置
        private void UpdateCharacterScaling(Rect bounds)
        {
            // if (viewModel == null || characterCanvas == null || bounds.IsEmpty == true) return; // 暂时移除检查
            if (viewModel == null || characterCanvas == null) return; // 保留这两个检查

            myLogger?.LogInfo($"MapView: UpdateCharacterScaling - Canvas Bounds: {bounds}");

            // 遍历所有 *有 Guid* 的角色来更新视觉（包括位置和可能的缩放调整）
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                if (character.Guid > 0) // 只处理已分配 Guid 的角色
                {
                    // myLogger?.LogTrace($"  -> UpdateCharacterScaling: 调用 UpdateCharacterVisual for Guid={character.Guid}");
                    UpdateCharacterVisual(character); // 重用更新逻辑来重新定位/缩放
                }
            }
        }

        private void MapView_DataContextChanged(object? sender, EventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                // myLogger?.LogDebug("MapView: DataContext 变为 MainWindowViewModel，尝试初始化...");
                viewModel = vm;
                TryInitializeMap(); // 尝试初始化或重新初始化
            }
            else
            {
                myLogger?.LogDebug("MapView: DataContext 清空或类型不匹配，清理...");
                CleanupViewModel(); // 清理旧订阅
                viewModel = null;
                isMapInitialized = false;
                mapGrid?.Children.Clear();
                characterCanvas?.Children.Clear();
                characterElements.Clear();
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
             myLogger?.LogDebug("MapView: 已附加到可视化树。");
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapGrid = this.FindControl<Grid>("MapGrid");
            TryInitializeMap();
        }


        // 尝试初始化地图和角色（核心逻辑）
        private void TryInitializeMap()
        {
            if (viewModel != null && mapGrid != null && characterCanvas != null && !isMapInitialized)
            {
                if (viewModel.MapVM != null)
                {
                    myLogger?.LogDebug("MapView: 开始初始化地图网格和角色...");
                    CleanupViewModel(); // 清理可能存在的旧订阅

                    // 设置 ViewModel 订阅 (包括 CollectionChanged)
                    SetupViewModelSubscriptions();
                    myLogger?.LogDebug("  -> ViewModel 订阅已设置。");

                    // 初始化地图格子 (只做一次)
                    MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);
                    myLogger?.LogDebug("  -> 地图网格已初始化。");

                    // *** 不再需要 RefreshCharacters() 在这里调用，由 CollectionChanged 处理初始添加 ***
                    // RefreshCharacters(); // 移除

                    // 订阅 Bounds 变化 (保持)
                    SubscribeToBoundsChanges();
                    myLogger?.LogDebug("  -> Canvas 尺寸变化订阅已设置。");

                    isMapInitialized = true;
                    myLogger?.LogDebug("MapView: 初始化完成。");
                }
                else
                {
                    myLogger?.LogWarning("MapView: ViewModel 已就绪, 但 MapVM 为 null。等待 MapVM...");
                }
            }
            // else block for logging why skipped (optional)
        }



        private void SubscribeToBoundsChanges()
        {
            _boundsSubscription?.Dispose();
            if (characterCanvas == null) return;
            _boundsSubscription = characterCanvas.GetObservable(BoundsProperty)
                 .Select(bounds => bounds)
                 .Where(bounds => bounds.Width > 0 && bounds.Height > 0)
                 .DistinctUntilChanged()
                 .Throttle(TimeSpan.FromMilliseconds(100))
                 .Subscribe(newBounds =>
                 {
                     Dispatcher.UIThread.InvokeAsync(() =>
                     {
                         if (this.IsAttachedToVisualTree() && characterCanvas != null)
                         {
                             // myLogger?.LogDebug($"MapView: Canvas Bounds 变化: {newBounds}");
                             UpdateCharacterScaling(newBounds);
                         }
                     });
                 }, ex => myLogger?.LogError($"MapView: Bounds 订阅出错: {ex}"));
        }
        private bool IsAttachedToVisualTree() => this.Parent != null || (this.VisualRoot != null);






        private void CleanupViewModel()
        {
            _boundsSubscription?.Dispose();
            _boundsSubscription = null;

            if (viewModel != null)
            {
                // 取消集合变化订阅
                viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;

                // 取消所有现有角色的属性变化订阅 (重要！)
                foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
                {
                    character.PropertyChanged -= Character_PropertyChanged;
                }
                myLogger?.LogDebug("MapView: 清理 ViewModel 订阅完成。");
            }

            // 清理 Canvas 和字典
            characterCanvas?.Children.Clear();
            characterElements.Clear();
            isMapInitialized = false; // 允许重新初始化
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

            // --- 订阅集合变化 ---
            viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged; // 先移除确保不重复
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged; // 先移除确保不重复
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // --- 处理当前已存在的角色 (如果 ViewModel 非空启动) ---
            // (这个逻辑现在由 CollectionChanged 首次触发 Add 来处理)
            // 也可以在这里手动调用一次处理逻辑，确保启动时就有角色
            ProcessNewCollection(viewModel.BuddhistsTeamCharacters);
            ProcessNewCollection(viewModel.MonstersTeamCharacters);
        }

        private void TeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => // 确保在 UI 线程操作 Canvas
            {
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (CharacterViewModel character in e.NewItems)
                    {
                        AddCharacterVisual(character);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
                {
                    foreach (CharacterViewModel character in e.OldItems)
                    {
                        RemoveCharacterVisual(character);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset) // 处理列表清空的情况
                {
                    myLogger?.LogWarning("MapView: 角色集合被重置 (Reset)，清理所有角色视觉元素。");
                    characterCanvas?.Children.Clear();
                    characterElements.Clear();
                    // 可能需要重新订阅所有角色的 PropertyChanged，或者重新初始化
                }
                // 可以根据需要处理 Replace, Move 等其他 Action
            });
        }

        private void ProcessNewCollection(IEnumerable<CharacterViewModel> characters)
        {
            foreach (var character in characters)
            {
                AddCharacterVisual(character);
            }
        }

        private void AddCharacterVisual(CharacterViewModel character)
        {
            if (characterCanvas == null || character.Guid <= 0 || characterElements.ContainsKey(character.Guid)) return;

            Color teamColor = character.TeamId == 0 ? Colors.Red : Colors.Blue; // Team 0=取经(红), Team 1=妖怪(蓝)
            var characterVisual = CreateCharacterVisual(character, teamColor);

            characterCanvas.Children.Add(characterVisual);
            characterElements[character.Guid] = characterVisual;

            // 订阅属性变化
            character.PropertyChanged -= Character_PropertyChanged; // 防重复
            character.PropertyChanged += Character_PropertyChanged;

            myLogger?.LogDebug($"MapView: Added visual element and subscribed PropertyChanged for Guid={character.Guid}, Name='{character.Name}'");

            // 初始调用 UpdateCharacterVisual 以设置位置/可见性
            UpdateCharacterVisual(character);
        }

        private void RemoveCharacterVisual(CharacterViewModel character)
        {
            if (characterCanvas == null || character.Guid <= 0 || !characterElements.TryGetValue(character.Guid, out var element))
            {
                // myLogger?.LogTrace($"MapView: 跳过移除视觉元素 Guid={character.Guid} (未找到或无效)");
                return; // 元素不存在
            }

            // 取消属性变化订阅 (非常重要，防止内存泄漏)
            character.PropertyChanged -= Character_PropertyChanged;

            characterCanvas.Children.Remove(element);
            characterElements.Remove(character.Guid);

            myLogger?.LogDebug($"MapView: 移除视觉元素 Guid={character.Guid}, Name='{character.Name}'");
        }


        // 刷新角色时，确保先清理旧的
        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;
            myLogger?.LogDebug("MapView: RefreshCharacters - 开始执行...");

            // 1. 清理 Canvas 和字典 (以防万一有残留)
            characterCanvas.Children.Clear();
            characterElements.Clear();
            myLogger?.LogDebug("  -> Canvas 和字典已清理。");


            // 2. 遍历 ViewModel 中的所有角色，触发 UpdateCharacterVisual
            // 这将为 Guid > 0 的角色创建或更新视觉元素
            int createdCount = 0;
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                UpdateCharacterVisual(character); // 让这个方法处理创建/更新/隐藏
                if (character.Guid > 0 && characterElements.ContainsKey(character.Guid)) createdCount++;
            }
            myLogger?.LogDebug($"  -> UpdateCharacterVisual 已为所有 ViewModel 角色调用。当前有效角色元素数: {createdCount}");


            // 3. 应用缩放 (如果 Canvas 已有尺寸)
            if (characterCanvas.Bounds.Width > 0 && characterCanvas.Bounds.Height > 0)
            {
                UpdateCharacterScaling(characterCanvas.Bounds);
                myLogger?.LogDebug($"  -> 初始缩放应用完成 (基于Canvas Bounds: {characterCanvas.Bounds})。");

            }
            else
            {
                myLogger?.LogDebug("  -> Canvas 初始尺寸无效，稍后 Bounds 变化时再应用缩放。");
            }
            myLogger?.LogDebug("MapView: RefreshCharacters - 执行完毕。");

        }

        // 重命名方法以反映其处理单个队伍
        private void InitializeTeamCharacters(System.Collections.ObjectModel.ObservableCollection<CharacterViewModel> characters, Color teamColor)
        {
            if (characterCanvas == null || viewModel == null) return;

            foreach (var character in characters)
            {
                if (character.Guid <= 0) continue; // 跳过未分配的占位符

                if (characterElements.ContainsKey(character.Guid))
                {
                    // myLogger?.LogTrace($"MapView: Guid={character.Guid} 已存在，跳过创建。");
                    continue; // 如果已存在，跳过创建，确保 UpdateCharacterVisual 会处理它
                }


                var characterVisual = CreateCharacterVisual(character, teamColor);
                characterCanvas.Children.Add(characterVisual);
                characterElements[character.Guid] = characterVisual;

                // *** 增加日志记录元素添加 ***
                myLogger?.LogDebug($"MapView: 添加角色元素到 Canvas: Guid={character.Guid}, Name='{character.Name}', TeamColor={teamColor}");

                // UpdateCharacterVisual 会在 PropertyChanged 时调用，或者在 RefreshCharacters 最后统一调用
                // UpdateCharacterVisual(character); // 不在此处单独调用，由 RefreshCharacters 统一处理或 PropertyChanged 触发
            }
        }

        // 创建单个角色的视觉元素
        private Control CreateCharacterVisual(CharacterViewModel character, Color teamColor)
        {
            var grid = new Grid
            {
                Width = 12, // 调整大小
                Height = 12,
                Tag = character.Guid // 存储 guID 以便查找
            };

            var ellipse = new Ellipse
            {
                Width = 16,
                Height = 16,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(teamColor),
                StrokeThickness = 1,
            };
            grid.Children.Add(ellipse);

            // 可以添加一个小的 TextBlock 显示编号或类型首字母
            var textBlock = new TextBlock
            {
                //Text = character.CharacterId.ToString(), 
                Text = GetCharacterInitial(character.Name),
                FontSize = 10,
                Foreground = new SolidColorBrush(teamColor),
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                IsHitTestVisible = false
            };
           
            grid.Children.Add(textBlock);


           

            return grid;
        }

        //获取角色名称首字母或标识符
        private string GetCharacterInitial(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Trim().EndsWith("?")) return "?";
            var trimmedName = name.Trim();
            if (trimmedName == "唐僧") return "T";
            if (trimmedName == "孙悟空") return "S";
            if (trimmedName == "猪八戒") return "Z";
            if (trimmedName == "沙悟净") return "W";
            if (trimmedName == "白龙马") return "B";
            if (trimmedName == "猴子猴孙") return "h";
            if (trimmedName == "九灵元圣") return "J";
            if (trimmedName == "红孩儿") return "H";
            if (trimmedName == "牛魔王") return "N";
            if (trimmedName == "铁扇公主") return "F";
            if (trimmedName == "蜘蛛精") return "P";
            if (trimmedName == "无名小妖") return "y";
            return trimmedName.Substring(0, 1);
        }



        // 角色属性变化时的处理
        private void Character_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is CharacterViewModel character)
            {
                // myLogger?.LogTrace($"MapView: PropertyChanged received for Guid={character.Guid}, Property={e.PropertyName}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (viewModel != null && this.IsAttachedToVisualTree())
                    {
                        // 不论哪个属性变化，都调用 UpdateCharacterVisual，让它决定如何处理
                        UpdateCharacterVisual(character);
                    }
                });
            }
        }

        // 更新单个角色的视觉状态（位置、可见性）
        // 更新视觉状态时添加详细日志
        private void UpdateCharacterVisual(CharacterViewModel character)
        {
            // *** 1. 检查基本条件 ***
            if (characterCanvas == null)
            {
                myLogger?.LogWarning($"MapView: UpdateCharacterVisual - Canvas is null, cannot update Guid={character.Guid}");
                return;
            }
            if (character.Guid <= 0)
            {
                // myLogger?.LogTrace($"MapView: UpdateCharacterVisual - Skipping Guid={character.Guid} (<=0)");
                return; // 不处理无效 Guid
            }

            // *** 2. 获取或创建视觉元素 ***
            if (!characterElements.TryGetValue(character.Guid, out var element))
            {
                // 如果元素不存在，可能 AddCharacterVisual 还没执行或失败，这里尝试补救添加
                myLogger?.LogWarning($"MapView: UpdateCharacterVisual - Element for Guid={character.Guid} not found in dictionary, attempting to add.");
                AddCharacterVisual(character); // 尝试添加
                if (!characterElements.TryGetValue(character.Guid, out element)) // 再次检查
                {
                    myLogger?.LogError($"MapView: UpdateCharacterVisual - Failed to get or add element for Guid={character.Guid}. Aborting update.");
                    return; // 如果添加后仍然找不到，则放弃
                }
            }


            // *** 强制可见 (用于调试) ***
            bool shouldBeVisible = character.Guid > 0 &&
                                   !character.StatusEffects.Contains("已死亡") && // 暂时忽略死亡状态?
                                   character.PosX >= 0 && character.PosX <= 50000 &&
                                   character.PosY >= 0 && character.PosY <= 50000;
            element.IsVisible = shouldBeVisible;
            // myLogger?.LogTrace($"MapView: Update Guid={character.Guid}, Name='{character.Name}', HP={character.Hp}, Pos=({character.PosX},{character.PosY}), ShouldBeVisible={shouldBeVisible}");


            // *** 4. 如果可见，计算并设置位置 ***
            if (shouldBeVisible)
            {
                double gameMaxX = 50000.0;
                double gameMaxY = 50000.0;
                double canvasWidth = characterCanvas.Bounds.Width;
                double canvasHeight = characterCanvas.Bounds.Height;

                // *** 检查 Canvas 尺寸 ***
                if (canvasWidth > 1 && canvasHeight > 1)
                {
                    // 获取元素尺寸 (如果首次布局可能为 0，使用默认值)
                    double elementWidth = element.Bounds.Width > 0 ? element.Bounds.Width : 12.0;
                    double elementHeight = element.Bounds.Height > 0 ? element.Bounds.Height : 12.0;

                    // 计算缩放
                    double scaleXToCanvasHeight = canvasHeight / gameMaxX; // X -> Top -> Height
                    double scaleYToCanvasWidth = canvasWidth / gameMaxY;   // Y -> Left -> Width

                    // 计算目标中心点
                    double targetTop = character.PosX * scaleXToCanvasHeight;
                    double targetLeft = character.PosY * scaleYToCanvasWidth;

                    // 计算左上角坐标以居中
                    double finalTop = targetTop - (elementHeight / 2.0);
                    double finalLeft = targetLeft - (elementWidth / 2.0);

                    // *** 添加详细坐标日志 ***
                    myLogger?.LogDebug($"MapView: SetPos Guid={character.Guid}, " +
                                       $"GamePos=({character.PosX},{character.PosY}), " +
                                       $"CanvasSize=({canvasWidth:F1},{canvasHeight:F1}), " +
                                       $"ScaleXY=({scaleYToCanvasWidth:F5},{scaleXToCanvasHeight:F5}), " + // Log Y scale then X scale
                                       $"TargetTL=({targetLeft:F1},{targetTop:F1}), " +
                                       $"ElementWH=({elementWidth:F1},{elementHeight:F1}), " +
                                       $"FinalTL=({finalLeft:F1},{finalTop:F1})");

                    // *** 确保最终坐标不小于 0 ***
                    finalTop = Math.Max(0, finalTop);
                    finalLeft = Math.Max(0, finalLeft);

                    //myLogger?.LogDebug($"MapView: SetPos Guid={character.Guid}, FinalTL_Clamped=({finalLeft:F1},{finalTop:F1})"); 

                    // *** 设置位置 ***
                    Canvas.SetLeft(element, finalLeft);
                    Canvas.SetTop(element, finalTop);

                    // 更新 Tooltip
                    //ToolTip.SetTip(element, $"{character.Name} (Guid: {character.Guid})\nHP: {character.Hp}/{character.MaxHp}\nPos: ({character.PosX},{character.PosY})\n状态: {character.DisplayStates}");
                }
                else
                {
                    myLogger?.LogWarning($"MapView: SetPos Guid={character.Guid} - Canvas size invalid (W:{canvasWidth:F1}, H:{canvasHeight:F1}), cannot calculate position.");
                }
            }
            // else // 如果不可见，不需要设置位置
            // {
            //    myLogger?.LogTrace($"MapView: Guid={character.Guid} is not visible, skipping position set.");
            // }
        }



    }
}