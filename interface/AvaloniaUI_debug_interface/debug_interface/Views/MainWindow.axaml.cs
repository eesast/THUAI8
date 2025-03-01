using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace debug_interface.Views
{    
    
    //public partial class MainWindow : Window
    //{
    //    public MainWindow()
    //    {
    //        InitializeComComponent();
    //    }
    //}

    /// <summary>
    /// �����ڵĺ��ô��룬�����ʼ�������
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools(); // �ڵ���ģʽ�¸��ӿ����߹���
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

}