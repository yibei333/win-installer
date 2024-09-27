using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
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
    }

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        bool _running;

        [ObservableProperty]
        string _name;

        [RelayCommand]
        public async Task Change()
        {
            Running = true;
            await Task.Yield();
            await Task.Delay(5000);
            Name += "Changed";
            Running = false;
        }
    }
}
