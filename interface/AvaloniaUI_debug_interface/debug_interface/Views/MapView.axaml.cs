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
        private Dictionary<long, Control> characterElements = new Dictionary<long, Control>(); // Key �� Guid
        private MainWindowViewModel? viewModel;
        private bool isMapInitialized = false;
        private IDisposable? _boundsSubscription;
        private Logger? myLogger; // Logger ����


        public MapView()
        {
            InitializeComponent();
            // *** ȷ�� Canvas ����ɫ���ڹ۲� ***
            var canvas = this.FindControl<Canvas>("CharacterCanvas");
            if (canvas != null)
            {
                 //canvas.Background = Brushes.LightPink; // ȡ�����������Թ۲� Canvas ����
                // *** ��� ZIndex ***
                canvas.SetValue(Panel.ZIndexProperty, 1);
            }

            this.DataContextChanged += MapView_DataContextChanged;
            this.AttachedToVisualTree += MapView_AttachedToVisualTree;
            this.DataContextChanged += (s, e) => { /* ... Logger ��ȡ�߼� ... */ };
        }


        // �� Canvas �ߴ�仯ʱ�����¼������н�ɫ��λ��
        private void UpdateCharacterScaling(Rect bounds)
        {
            // if (viewModel == null || characterCanvas == null || bounds.IsEmpty == true) return; // ��ʱ�Ƴ����
            if (viewModel == null || characterCanvas == null) return; // �������������

            myLogger?.LogInfo($"MapView: UpdateCharacterScaling - Canvas Bounds: {bounds}");

            // �������� *�� Guid* �Ľ�ɫ�������Ӿ�������λ�úͿ��ܵ����ŵ�����
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                if (character.Guid > 0) // ֻ�����ѷ��� Guid �Ľ�ɫ
                {
                    // myLogger?.LogTrace($"  -> UpdateCharacterScaling: ���� UpdateCharacterVisual for Guid={character.Guid}");
                    UpdateCharacterVisual(character); // ���ø����߼������¶�λ/����
                }
            }
        }

        private void MapView_DataContextChanged(object? sender, EventArgs e)
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                // myLogger?.LogDebug("MapView: DataContext ��Ϊ MainWindowViewModel�����Գ�ʼ��...");
                viewModel = vm;
                TryInitializeMap(); // ���Գ�ʼ�������³�ʼ��
            }
            else
            {
                myLogger?.LogDebug("MapView: DataContext ��ջ����Ͳ�ƥ�䣬����...");
                CleanupViewModel(); // ����ɶ���
                viewModel = null;
                isMapInitialized = false;
                mapGrid?.Children.Clear();
                characterCanvas?.Children.Clear();
                characterElements.Clear();
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
             myLogger?.LogDebug("MapView: �Ѹ��ӵ����ӻ�����");
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapGrid = this.FindControl<Grid>("MapGrid");
            TryInitializeMap();
        }


        // ���Գ�ʼ����ͼ�ͽ�ɫ�������߼���
        private void TryInitializeMap()
        {
            if (viewModel != null && mapGrid != null && characterCanvas != null && !isMapInitialized)
            {
                if (viewModel.MapVM != null)
                {
                    myLogger?.LogDebug("MapView: ��ʼ��ʼ����ͼ����ͽ�ɫ...");
                    CleanupViewModel(); // ������ܴ��ڵľɶ���

                    // ���� ViewModel ���� (���� CollectionChanged)
                    SetupViewModelSubscriptions();
                    myLogger?.LogDebug("  -> ViewModel ���������á�");

                    // ��ʼ����ͼ���� (ֻ��һ��)
                    MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);
                    myLogger?.LogDebug("  -> ��ͼ�����ѳ�ʼ����");

                    // *** ������Ҫ RefreshCharacters() ��������ã��� CollectionChanged �����ʼ��� ***
                    // RefreshCharacters(); // �Ƴ�

                    // ���� Bounds �仯 (����)
                    SubscribeToBoundsChanges();
                    myLogger?.LogDebug("  -> Canvas �ߴ�仯���������á�");

                    isMapInitialized = true;
                    myLogger?.LogDebug("MapView: ��ʼ����ɡ�");
                }
                else
                {
                    myLogger?.LogWarning("MapView: ViewModel �Ѿ���, �� MapVM Ϊ null���ȴ� MapVM...");
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
                             // myLogger?.LogDebug($"MapView: Canvas Bounds �仯: {newBounds}");
                             UpdateCharacterScaling(newBounds);
                         }
                     });
                 }, ex => myLogger?.LogError($"MapView: Bounds ���ĳ���: {ex}"));
        }
        private bool IsAttachedToVisualTree() => this.Parent != null || (this.VisualRoot != null);






        private void CleanupViewModel()
        {
            _boundsSubscription?.Dispose();
            _boundsSubscription = null;

            if (viewModel != null)
            {
                // ȡ�����ϱ仯����
                viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;

                // ȡ���������н�ɫ�����Ա仯���� (��Ҫ��)
                foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
                {
                    character.PropertyChanged -= Character_PropertyChanged;
                }
                myLogger?.LogDebug("MapView: ���� ViewModel ������ɡ�");
            }

            // ���� Canvas ���ֵ�
            characterCanvas?.Children.Clear();
            characterElements.Clear();
            isMapInitialized = false; // �������³�ʼ��
        }

        private void SetupViewModel(MainWindowViewModel vm)
        {
            // ��ֹ�ظ����û���� ViewModel
            if (vm == viewModel) return;

            CleanupViewModel(); // ����ɵ� ViewModel ����

            viewModel = vm;

            // ��ʼ����ͼ���� (�����δ��ʼ���� ViewModel �ı�)
            if (viewModel.MapVM != null)
            {
                if (mapGrid != null) MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);
            }

            // ������ɫ���ϱ仯 (���/ɾ�� - ��Ȼ����Ԥʵ�����ˣ����Է���һ)
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // ��ʼ����ɫ UI Ԫ�ز��������Ա仯
            RefreshCharacters();
            // InitializeRandomPositions(); // ������Ҫ���λ�ã����� ViewModel �� PosX/PosY

            // ��ͼ��Ԫ��� PropertyChanged ���� MapHelper �д���
        }

        // ������ ViewModel ��صĶ���
        private void SetupViewModelSubscriptions()
        {
            if (viewModel == null) return;

            // --- ���ļ��ϱ仯 ---
            viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged; // ���Ƴ�ȷ�����ظ�
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged; // ���Ƴ�ȷ�����ظ�
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // --- ����ǰ�Ѵ��ڵĽ�ɫ (��� ViewModel �ǿ�����) ---
            // (����߼������� CollectionChanged �״δ��� Add ������)
            // Ҳ�����������ֶ�����һ�δ����߼���ȷ������ʱ���н�ɫ
            ProcessNewCollection(viewModel.BuddhistsTeamCharacters);
            ProcessNewCollection(viewModel.MonstersTeamCharacters);
        }

        private void TeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() => // ȷ���� UI �̲߳��� Canvas
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
                else if (e.Action == NotifyCollectionChangedAction.Reset) // �����б���յ����
                {
                    myLogger?.LogWarning("MapView: ��ɫ���ϱ����� (Reset)���������н�ɫ�Ӿ�Ԫ�ء�");
                    characterCanvas?.Children.Clear();
                    characterElements.Clear();
                    // ������Ҫ���¶������н�ɫ�� PropertyChanged���������³�ʼ��
                }
                // ���Ը�����Ҫ���� Replace, Move ������ Action
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

            Color teamColor = character.TeamId == 0 ? Colors.Red : Colors.Blue; // Team 0=ȡ��(��), Team 1=����(��)
            var characterVisual = CreateCharacterVisual(character, teamColor);

            characterCanvas.Children.Add(characterVisual);
            characterElements[character.Guid] = characterVisual;

            // �������Ա仯
            character.PropertyChanged -= Character_PropertyChanged; // ���ظ�
            character.PropertyChanged += Character_PropertyChanged;

            myLogger?.LogDebug($"MapView: Added visual element and subscribed PropertyChanged for Guid={character.Guid}, Name='{character.Name}'");

            // ��ʼ���� UpdateCharacterVisual ������λ��/�ɼ���
            UpdateCharacterVisual(character);
        }

        private void RemoveCharacterVisual(CharacterViewModel character)
        {
            if (characterCanvas == null || character.Guid <= 0 || !characterElements.TryGetValue(character.Guid, out var element))
            {
                // myLogger?.LogTrace($"MapView: �����Ƴ��Ӿ�Ԫ�� Guid={character.Guid} (δ�ҵ�����Ч)");
                return; // Ԫ�ز�����
            }

            // ȡ�����Ա仯���� (�ǳ���Ҫ����ֹ�ڴ�й©)
            character.PropertyChanged -= Character_PropertyChanged;

            characterCanvas.Children.Remove(element);
            characterElements.Remove(character.Guid);

            myLogger?.LogDebug($"MapView: �Ƴ��Ӿ�Ԫ�� Guid={character.Guid}, Name='{character.Name}'");
        }


        // ˢ�½�ɫʱ��ȷ��������ɵ�
        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;
            myLogger?.LogDebug("MapView: RefreshCharacters - ��ʼִ��...");

            // 1. ���� Canvas ���ֵ� (�Է���һ�в���)
            characterCanvas.Children.Clear();
            characterElements.Clear();
            myLogger?.LogDebug("  -> Canvas ���ֵ�������");


            // 2. ���� ViewModel �е����н�ɫ������ UpdateCharacterVisual
            // �⽫Ϊ Guid > 0 �Ľ�ɫ����������Ӿ�Ԫ��
            int createdCount = 0;
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                UpdateCharacterVisual(character); // ���������������/����/����
                if (character.Guid > 0 && characterElements.ContainsKey(character.Guid)) createdCount++;
            }
            myLogger?.LogDebug($"  -> UpdateCharacterVisual ��Ϊ���� ViewModel ��ɫ���á���ǰ��Ч��ɫԪ����: {createdCount}");


            // 3. Ӧ������ (��� Canvas ���гߴ�)
            if (characterCanvas.Bounds.Width > 0 && characterCanvas.Bounds.Height > 0)
            {
                UpdateCharacterScaling(characterCanvas.Bounds);
                myLogger?.LogDebug($"  -> ��ʼ����Ӧ����� (����Canvas Bounds: {characterCanvas.Bounds})��");

            }
            else
            {
                myLogger?.LogDebug("  -> Canvas ��ʼ�ߴ���Ч���Ժ� Bounds �仯ʱ��Ӧ�����š�");
            }
            myLogger?.LogDebug("MapView: RefreshCharacters - ִ����ϡ�");

        }

        // �����������Է�ӳ�䴦��������
        private void InitializeTeamCharacters(System.Collections.ObjectModel.ObservableCollection<CharacterViewModel> characters, Color teamColor)
        {
            if (characterCanvas == null || viewModel == null) return;

            foreach (var character in characters)
            {
                if (character.Guid <= 0) continue; // ����δ�����ռλ��

                if (characterElements.ContainsKey(character.Guid))
                {
                    // myLogger?.LogTrace($"MapView: Guid={character.Guid} �Ѵ��ڣ�����������");
                    continue; // ����Ѵ��ڣ�����������ȷ�� UpdateCharacterVisual �ᴦ����
                }


                var characterVisual = CreateCharacterVisual(character, teamColor);
                characterCanvas.Children.Add(characterVisual);
                characterElements[character.Guid] = characterVisual;

                // *** ������־��¼Ԫ����� ***
                myLogger?.LogDebug($"MapView: ��ӽ�ɫԪ�ص� Canvas: Guid={character.Guid}, Name='{character.Name}', TeamColor={teamColor}");

                // UpdateCharacterVisual ���� PropertyChanged ʱ���ã������� RefreshCharacters ���ͳһ����
                // UpdateCharacterVisual(character); // ���ڴ˴��������ã��� RefreshCharacters ͳһ����� PropertyChanged ����
            }
        }

        // ����������ɫ���Ӿ�Ԫ��
        private Control CreateCharacterVisual(CharacterViewModel character, Color teamColor)
        {
            var grid = new Grid
            {
                Width = 12, // ������С
                Height = 12,
                Tag = character.Guid // �洢 guID �Ա����
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

            // �������һ��С�� TextBlock ��ʾ��Ż���������ĸ
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

        //��ȡ��ɫ��������ĸ���ʶ��
        private string GetCharacterInitial(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Trim().EndsWith("?")) return "?";
            var trimmedName = name.Trim();
            if (trimmedName == "��ɮ") return "T";
            if (trimmedName == "�����") return "S";
            if (trimmedName == "��˽�") return "Z";
            if (trimmedName == "ɳ��") return "W";
            if (trimmedName == "������") return "B";
            if (trimmedName == "���Ӻ���") return "h";
            if (trimmedName == "����Ԫʥ") return "J";
            if (trimmedName == "�캢��") return "H";
            if (trimmedName == "ţħ��") return "N";
            if (trimmedName == "���ȹ���") return "F";
            if (trimmedName == "֩�뾫") return "P";
            if (trimmedName == "����С��") return "y";
            return trimmedName.Substring(0, 1);
        }



        // ��ɫ���Ա仯ʱ�Ĵ���
        private void Character_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is CharacterViewModel character)
            {
                // myLogger?.LogTrace($"MapView: PropertyChanged received for Guid={character.Guid}, Property={e.PropertyName}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (viewModel != null && this.IsAttachedToVisualTree())
                    {
                        // �����ĸ����Ա仯�������� UpdateCharacterVisual������������δ���
                        UpdateCharacterVisual(character);
                    }
                });
            }
        }

        // ���µ�����ɫ���Ӿ�״̬��λ�á��ɼ��ԣ�
        // �����Ӿ�״̬ʱ�����ϸ��־
        private void UpdateCharacterVisual(CharacterViewModel character)
        {
            // *** 1. ���������� ***
            if (characterCanvas == null)
            {
                myLogger?.LogWarning($"MapView: UpdateCharacterVisual - Canvas is null, cannot update Guid={character.Guid}");
                return;
            }
            if (character.Guid <= 0)
            {
                // myLogger?.LogTrace($"MapView: UpdateCharacterVisual - Skipping Guid={character.Guid} (<=0)");
                return; // ��������Ч Guid
            }

            // *** 2. ��ȡ�򴴽��Ӿ�Ԫ�� ***
            if (!characterElements.TryGetValue(character.Guid, out var element))
            {
                // ���Ԫ�ز����ڣ����� AddCharacterVisual ��ûִ�л�ʧ�ܣ����ﳢ�Բ������
                myLogger?.LogWarning($"MapView: UpdateCharacterVisual - Element for Guid={character.Guid} not found in dictionary, attempting to add.");
                AddCharacterVisual(character); // �������
                if (!characterElements.TryGetValue(character.Guid, out element)) // �ٴμ��
                {
                    myLogger?.LogError($"MapView: UpdateCharacterVisual - Failed to get or add element for Guid={character.Guid}. Aborting update.");
                    return; // �����Ӻ���Ȼ�Ҳ����������
                }
            }


            // *** ǿ�ƿɼ� (���ڵ���) ***
            bool shouldBeVisible = character.Guid > 0 &&
                                   !character.StatusEffects.Contains("������") && // ��ʱ��������״̬?
                                   character.PosX >= 0 && character.PosX <= 50000 &&
                                   character.PosY >= 0 && character.PosY <= 50000;
            element.IsVisible = shouldBeVisible;
            // myLogger?.LogTrace($"MapView: Update Guid={character.Guid}, Name='{character.Name}', HP={character.Hp}, Pos=({character.PosX},{character.PosY}), ShouldBeVisible={shouldBeVisible}");


            // *** 4. ����ɼ������㲢����λ�� ***
            if (shouldBeVisible)
            {
                double gameMaxX = 50000.0;
                double gameMaxY = 50000.0;
                double canvasWidth = characterCanvas.Bounds.Width;
                double canvasHeight = characterCanvas.Bounds.Height;

                // *** ��� Canvas �ߴ� ***
                if (canvasWidth > 1 && canvasHeight > 1)
                {
                    // ��ȡԪ�سߴ� (����״β��ֿ���Ϊ 0��ʹ��Ĭ��ֵ)
                    double elementWidth = element.Bounds.Width > 0 ? element.Bounds.Width : 12.0;
                    double elementHeight = element.Bounds.Height > 0 ? element.Bounds.Height : 12.0;

                    // ��������
                    double scaleXToCanvasHeight = canvasHeight / gameMaxX; // X -> Top -> Height
                    double scaleYToCanvasWidth = canvasWidth / gameMaxY;   // Y -> Left -> Width

                    // ����Ŀ�����ĵ�
                    double targetTop = character.PosX * scaleXToCanvasHeight;
                    double targetLeft = character.PosY * scaleYToCanvasWidth;

                    // �������Ͻ������Ծ���
                    double finalTop = targetTop - (elementHeight / 2.0);
                    double finalLeft = targetLeft - (elementWidth / 2.0);

                    // *** �����ϸ������־ ***
                    myLogger?.LogDebug($"MapView: SetPos Guid={character.Guid}, " +
                                       $"GamePos=({character.PosX},{character.PosY}), " +
                                       $"CanvasSize=({canvasWidth:F1},{canvasHeight:F1}), " +
                                       $"ScaleXY=({scaleYToCanvasWidth:F5},{scaleXToCanvasHeight:F5}), " + // Log Y scale then X scale
                                       $"TargetTL=({targetLeft:F1},{targetTop:F1}), " +
                                       $"ElementWH=({elementWidth:F1},{elementHeight:F1}), " +
                                       $"FinalTL=({finalLeft:F1},{finalTop:F1})");

                    // *** ȷ���������겻С�� 0 ***
                    finalTop = Math.Max(0, finalTop);
                    finalLeft = Math.Max(0, finalLeft);

                    //myLogger?.LogDebug($"MapView: SetPos Guid={character.Guid}, FinalTL_Clamped=({finalLeft:F1},{finalTop:F1})"); 

                    // *** ����λ�� ***
                    Canvas.SetLeft(element, finalLeft);
                    Canvas.SetTop(element, finalTop);

                    // ���� Tooltip
                    //ToolTip.SetTip(element, $"{character.Name} (Guid: {character.Guid})\nHP: {character.Hp}/{character.MaxHp}\nPos: ({character.PosX},{character.PosY})\n״̬: {character.DisplayStates}");
                }
                else
                {
                    myLogger?.LogWarning($"MapView: SetPos Guid={character.Guid} - Canvas size invalid (W:{canvasWidth:F1}, H:{canvasHeight:F1}), cannot calculate position.");
                }
            }
            // else // ������ɼ�������Ҫ����λ��
            // {
            //    myLogger?.LogTrace($"MapView: Guid={character.Guid} is not visible, skipping position set.");
            // }
        }



    }
}