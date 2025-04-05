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
        private bool isMapInitialized = false; // ���һ����־λ

        //public MapView()
        //{
        //    InitializeComponent();
        //    this.AttachedToVisualTree += MapView_AttachedToVisualTree;
        //    this.DataContextChanged += MapView_DataContextChanged;
        //    // ���� Canvas �ߴ�仯�Ը�����������
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
                .Select(args => args.NewValue.GetValueOrDefault()) // ���� Rect
                .Subscribe(newBounds => // newBounds �� Rect ����
                {
                    // ֱ��ʹ�� newBounds��������Ҫ��� HasValue ����� Value
                    // Rect currentBounds = newBounds; // �����Ƕ����

                    // ʹ��֮ǰ�� IsEmpty �������
                    bool isEffectivelyEmpty = newBounds.Width <= 0 || newBounds.Height <= 0;

                    if (isEffectivelyEmpty)
                    {
                        // Console.WriteLine("Bounds are effectively empty, skipping scaling.");
                    }
                    else
                    {
                        // ֱ�Ӵ��� newBounds
                        Dispatcher.UIThread.InvokeAsync(() => UpdateCharacterScaling(newBounds));
                    }
                });
        }

        // �� Canvas �ߴ�仯ʱ�����¼������н�ɫ��λ��
        private void UpdateCharacterScaling(Rect bounds)
        {
            // if (viewModel == null || characterCanvas == null || bounds.IsEmpty == true) return; // ��ʱ�Ƴ����
            if (viewModel == null || characterCanvas == null) return; // �������������

            // ����Ĵ������ڱ�֤�� UI �߳�ִ��
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                UpdateCharacterVisual(character); // ���ø����߼������¶�λ
            }
        }

        private void MapView_DataContextChanged(object? sender, EventArgs e)
        {
            // �� DataContext �仯ʱ���������� ViewModel ����ʼ����ͼ
            if (this.DataContext is MainWindowViewModel vm)
            {
                viewModel = vm; // ��ȡ ViewModel
                TryInitializeMap(); // ���Գ�ʼ��
            }
            else
            {
                // DataContext ����գ�������Դ
                CleanupViewModel();
                viewModel = null;
                isMapInitialized = false; // ���ñ�־λ
                mapGrid?.Children.Clear(); // ��յ�ͼGrid����
                characterCanvas?.Children.Clear(); // ��ս�ɫCanvas����
            }
        }

        private void MapView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // �����ӵ����ӻ���ʱ�����ҿؼ������Գ�ʼ����ͼ
            characterCanvas = this.FindControl<Canvas>("CharacterCanvas");
            mapGrid = this.FindControl<Grid>("MapGrid");
            TryInitializeMap(); // ���Գ�ʼ��
        }


        // ���Գ�ʼ����ͼ�ͽ�ɫ�������߼���
        private void TryInitializeMap()
        {
            // ֻ�е� ViewModel �� MapGrid ��׼���ã����ҵ�ͼ��δ��ʼ��ʱ��ִ��
            if (viewModel != null && mapGrid != null && !isMapInitialized)
            {
                // ��� MapVM �Ƿ�Ҳ׼������
                if (viewModel.MapVM != null)
                {
                    Console.WriteLine("MapView: Initializing Map Grid and Characters..."); // �����־
                    CleanupViewModel(); // ����ɵĶ��ģ����֮ǰ�еĻ���

                    // ���� ViewModel ���¼�������
                    SetupViewModelSubscriptions();

                    // ��ʼ����ͼ Grid
                    MapHelper.InitializeMapGrid(mapGrid, viewModel.MapVM);

                    // ��ʼ����ɫ Canvas
                    RefreshCharacters();

                    isMapInitialized = true; // ��ǵ�ͼ�ѳ�ʼ��
                }
                else
                {
                    Console.WriteLine("MapView: ViewModel is ready, but MapVM is null."); // ��־
                }
            }
            // else
            // {
            // ���Լ���־˵��Ϊʲôû��ʼ��
            // Console.WriteLine($"MapView: Initialize skipped. ViewModel: {viewModel != null}, MapGrid: {mapGrid != null}, Initialized: {isMapInitialized}");
            // }
        }
        private void CleanupViewModel()
        {
            if (viewModel != null)
            {
                // ȡ��֮ǰ�ļ��Ϻ����Ա������
                viewModel.BuddhistsTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                viewModel.MonstersTeamCharacters.CollectionChanged -= TeamCharacters_CollectionChanged;
                foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
                {
                    character.PropertyChanged -= Character_PropertyChanged;
                }
                // ע�⣺���ﲻ��� viewModel ������DataContextChanged �ᴦ��
            }
            // characterElements �� characterCanvas �������Ƶ� RefreshCharacters �� DataContext �����ʱ
            // characterElements.Clear();
            // characterCanvas?.Children.Clear();
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

            // ������ɫ���ϱ仯
            viewModel.BuddhistsTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;
            viewModel.MonstersTeamCharacters.CollectionChanged += TeamCharacters_CollectionChanged;

            // �����������н�ɫ�����Ա仯
            foreach (var character in viewModel.BuddhistsTeamCharacters.Concat(viewModel.MonstersTeamCharacters))
            {
                // ȷ��ֻ���һ�μ�����
                character.PropertyChanged -= Character_PropertyChanged; // ���Ƴ�����ֹ�ظ����
                character.PropertyChanged += Character_PropertyChanged;
            }
        }

        private void TeamCharacters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // ���Ϸ����仯ʱ�������ϲ�ӦƵ����������Ϊ��ռλ������ˢ��������ɫUI
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(RefreshCharacters);
        }


        private void RefreshCharacters()
        {
            if (characterCanvas == null || viewModel == null) return;

            // ����ɵ� UI Ԫ�غ��¼�������
            foreach (var kvp in characterElements)
            {
                characterCanvas.Children.Remove(kvp.Value);
                // ������Ӧ���� CleanupViewModel ��ͳһ����� ViewModel ���¼�ע��
            }
            characterElements.Clear();

            // Ϊ ViewModel �е����н�ɫ������ռλ������������� UI Ԫ��
            InitializeTeamCharacters(viewModel.BuddhistsTeamCharacters, Colors.Red);
            InitializeTeamCharacters(viewModel.MonstersTeamCharacters, Colors.Blue);

            // ����������ȷ����ʼλ����ȷ
            UpdateCharacterScaling(this.Bounds);
        }

        // �����������Է�ӳ�䴦��������
        private void InitializeTeamCharacters(System.Collections.ObjectModel.ObservableCollection<CharacterViewModel> characters, Color teamColor)
        {
            if (characterCanvas == null || viewModel == null) return;

            foreach (var character in characters)
            {
                // ����ý�ɫ�� UI Ԫ���Ѵ��ڣ�����������ֻ��ȷ���¼��������Ѹ���
                if (characterElements.ContainsKey(character.CharacterId)) continue;

                // ������ɫ�Ӿ�Ԫ�� (����һ�� Grid ���� Ellipse �� TextBlock)
                var characterVisual = CreateCharacterVisual(character, teamColor);

                // ��ӵ� Canvas
                characterCanvas.Children.Add(characterVisual);
                characterElements[character.CharacterId] = characterVisual; // �����ֵ�

                // ������ɫ���Ա仯
                character.PropertyChanged += Character_PropertyChanged;

                // ���ó�ʼλ�úͿɼ���
                UpdateCharacterVisual(character);
            }
        }

        // ����������ɫ���Ӿ�Ԫ��
        private Control CreateCharacterVisual(CharacterViewModel character, Color teamColor)
        {
            var grid = new Grid
            {
                Width = 12, // ������С
                Height = 12,
                Tag = character.CharacterId // �洢 ID �Ա����
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

            // �������һ��С�� TextBlock ��ʾ��Ż���������ĸ
            var textBlock = new TextBlock
            {
                //Text = character.CharacterId.ToString(), // ��������������ĸ
                Text = GetCharacterInitial(character.Name),
                FontSize = 7,
                Foreground = new SolidColorBrush(teamColor),
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            grid.Children.Add(textBlock);


            ToolTip.SetTip(grid, $"{character.Name} (ID: {character.CharacterId})\nHP: {character.Hp}\n״̬: {character.ActiveState}"); // ��ʼ Tooltip

            return grid;
        }

        // ��ȡ��ɫ��������ĸ���ʶ��
        private string GetCharacterInitial(string name)
        {
            if (string.IsNullOrEmpty(name) || name.EndsWith("?")) return "?";
            if (name == "��ɮ") return "T";
            if (name == "�����") return "S";
            if (name == "��˽�") return "Z";
            if (name == "ɳ��") return "W";
            if (name == "������") return "B";
            if (name == "���Ӻ���") return "h";
            if (name == "��ͷԪʥ") return "J";
            if (name == "�캢��") return "H";
            if (name == "ţħ��") return "N";
            if (name == "���ȹ���") return "F";
            if (name == "֩�뾫") return "P";
            if (name == "����С��") return "y";
            return name.Length > 0 ? name.Substring(0, 1) : "?";
        }


        // ��ɫ���Ա仯ʱ�Ĵ���
        private void Character_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is CharacterViewModel character)
            {
                // �� UI �߳��ϸ����Ӿ�Ԫ��
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => UpdateCharacterVisual(character));
            }
        }

        // ���µ�����ɫ���Ӿ�״̬��λ�á��ɼ��ԡ�Tooltip��
        private void UpdateCharacterVisual(CharacterViewModel character)
        {
            if (characterElements.TryGetValue(character.CharacterId, out var element))
            {
                // ���� Tooltip
                ToolTip.SetTip(element, $"{character.Name} (ID: {character.CharacterId})\nHP: {character.Hp}\n״̬: {character.ActiveState}\nλ��: ({character.PosX},{character.PosY})");

                // ����ɫ�Ƿ���Ч��Ӧ��ʾ�ڵ�ͼ��
                bool shouldBeVisible = character.Hp > 0 && // ����
                                      !character.Name.EndsWith("?") && // ���Ǵ�ռλ��
                                      !character.PassiveStates.Contains("������") && // û������״̬
                                      character.PosX >= 0 && character.PosX < 50 && // �ڵ�ͼ������
                                      character.PosY >= 0 && character.PosY < 50;

                element.IsVisible = shouldBeVisible;

                if (shouldBeVisible)
                {
                    // �������ź������λ��
                    double cellWidth = characterCanvas?.Bounds.Width / 50.0 ?? 0;
                    double cellHeight = characterCanvas?.Bounds.Height / 50.0 ?? 0;
                    double left = character.PosY * cellWidth + (cellWidth / 2.0) - (element.Bounds.Width / 2.0);
                    double top = character.PosX * cellHeight + (cellHeight / 2.0) - (element.Bounds.Height / 2.0);

                    Canvas.SetLeft(element, left);
                    Canvas.SetTop(element, top);
                }
            }
        }



        // --- �ɵĻ�����Ҫ�ķ��� ---
        // private void InitializeRandomPositions() { ... } // ����Ҫ��
        // private void InitializeCharacters<T>(...) { ... } // �ѱ� InitializeTeamCharacters ���
        // private void UpdateCharacterPosition(Control element, int x, int y) { ... } // �Ѻϲ��� UpdateCharacterVisual
        // public void UpdateCharacterPosition(long characterId, int x, int y, bool isRedTeam, string name) { ... } // ������Ҫ���� ViewModel ����
        // private Ellipse FindCharacterMarker(long characterId) { ... } // ������Ҫ
        // private TextBlock FindCharacterLabel(long characterId) { ... } // ������Ҫ

    }
}