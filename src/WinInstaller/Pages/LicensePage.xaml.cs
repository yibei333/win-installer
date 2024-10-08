using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace WinInstaller.Pages
{
    public partial class LicensePage : UserControl
    {
        public LicenseViewModel ViewModel { get; set; } = new LicenseViewModel();

        public LicensePage()
        {
            InitializeComponent();
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Agree) return;
            MainWindow.Instance.SetPage<PreRequestPage>();
        }
    }

    public partial class LicenseViewModel : ObservableObject
    {
        [ObservableProperty]
        string _license = @"1.该软件为免费开源软件,可以用于任何场景.
2.请使用者注意备份数据,由该软件造成的数据丢失和任何问题,开发中不承担任何责任.
";
        [ObservableProperty]
        bool _agree;

        [RelayCommand]
        public void AgreeSelect(string agree)
        {
            Agree = bool.TryParse(agree, out var result) && result;
        }
    }
}
