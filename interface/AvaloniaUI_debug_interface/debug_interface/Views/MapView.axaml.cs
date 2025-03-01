//MapVie.axaml.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using debug_interface.ViewModels;

namespace debug_interface.Views
{
    public partial class MapView : UserControl
    {
        public MapView()
        {
            InitializeComponent();
            DataContext = new MapViewModel();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}