using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WinInstaller.Setup.Pages;

namespace WinInstaller.Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            SetPage<LicensePage>();
            Instance = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ViewModel.Running)
            {
                MessageBox.Show("任务处理中,请稍候...");
                e.Cancel = true;
            }
        }

        private void Window_Minimal(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void SetPage<TPage>() where TPage : UserControl, new()
        {
            Dispatcher.Invoke(() => ViewModel.Page = new TPage());
        }
    }

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        bool _running;

        [ObservableProperty]
        UserControl _page;
    }
}
