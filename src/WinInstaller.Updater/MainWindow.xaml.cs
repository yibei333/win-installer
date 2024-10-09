using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WinInstaller.Updater;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    Config _config;

    public MainWindow()
    {
        InitializeComponent();
        _config = Config.Get();
        CheckUrls.CollectionChanged += (s, e) =>
        {
            var index = 1;
            foreach (var item in CheckUrls)
            {
                item.Id = index;
                index++;
            }
            _config.VersionCheckUrl = string.Join(";", CheckUrls.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).Distinct());
        };
        DownloadUrls.CollectionChanged += (s, e) =>
        {
            var index = 1;
            foreach (var item in DownloadUrls)
            {
                item.Id = index;
                index++;
            }
            _config.PackageDownloadUrl = string.Join(";", DownloadUrls.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).Distinct());
        };
        Init();
        Check();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (Running)
        {
            MessageBox.Show("任务处理中,请稍候...");
            e.Cancel = true;
        }
    }

    private void Window_Close(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Sub_Url(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button is null) return;
        var item = button.DataContext as IdValue;
        if (item.Type == 1)
        {
            if (item is null || !CheckUrls.Contains(item)) return;
            CheckUrls.Remove(item);
        }
        else
        {
            if (item is null || !DownloadUrls.Contains(item)) return;

            DownloadUrls.Remove(item);
        }
    }

    private void Add_CheckUrl(object sender, RoutedEventArgs e)
    {
        CheckUrls.Add(new IdValue { Type = 1 });
    }

    private void Add_DownloadUrl(object sender, RoutedEventArgs e)
    {
        DownloadUrls.Add(new IdValue { Type = 2 });
    }

    #region Properties
    public event PropertyChangedEventHandler PropertyChanged;

    string titleText = "更新程序";
    public string TitleText
    {
        get => titleText;
        set
        {
            if (titleText != value)
            {
                titleText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleText)));
            }
        }
    }

    bool running;
    public bool Running
    {
        get => running;
        set
        {
            if (running != value)
            {
                running = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Running)));
            }
        }
    }

    string currentVersion;
    public string CurrentVersion
    {
        get => currentVersion;
        set
        {
            if (currentVersion != value)
            {
                currentVersion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentVersion)));
            }
        }
    }

    string lastVersion = "?";
    public string LastVersion
    {
        get => lastVersion;
        set
        {
            if (lastVersion != value)
            {
                lastVersion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastVersion)));
            }

            if (!string.IsNullOrWhiteSpace(LastVersion) && LastVersion != CurrentVersion) hasUpdate = true;
            else hasUpdate = false;
        }
    }

    bool hasUpdate;
    public bool HasUpdate
    {
        get => hasUpdate;
        set
        {
            if (hasUpdate != value)
            {
                hasUpdate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUpdate)));
            }
        }
    }

    public ObservableCollection<IdValue> CheckUrls { get; } = new ObservableCollection<IdValue>();
    public ObservableCollection<IdValue> DownloadUrls { get; } = new ObservableCollection<IdValue>();
    #endregion

    void Init()
    {
        TitleText = $"{_config.DisplayName}-更新程序";
        CurrentVersion = _config.DisplayVersion;

        _config.VersionCheckUrl?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList().ForEach(x => CheckUrls.Add(new IdValue { Value = x, Type = 1 }));
        _config.PackageDownloadUrl?.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList().ForEach(x => DownloadUrls.Add(new IdValue { Value = x, Type = 2 }));
    }

    async void Check()
    {
        Running = true;
        await Task.Yield();
        try
        {
            await Task.Run(() =>
            {

            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            Running = false;
        }
    }

    async void Update()
    {
        Running = true;
        await Task.Yield();
        try
        {
            await Task.Run(() =>
            {

            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            Running = false;
        }
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = bool.TryParse(value?.ToString(), out var result) && result;
        if (boolValue) return Visibility.Visible;
        else return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = bool.TryParse(value?.ToString(), out var result) && result;
        if (!boolValue) return Visibility.Visible;
        else return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ItemCountVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var count = int.TryParse(value?.ToString(), out var result) ? result : 0;
        if (count <= 1) return Visibility.Collapsed;
        else return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IdValue : INotifyPropertyChanged
{
    int _id;
    string _value;

    public int Id
    {
        get => _id;
        set
        {
            if (value != _id)
            {
                _id = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
            }
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }

    public int Type { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class ButtonAttach
{
    public static DependencyProperty RadiusProperty = DependencyProperty.RegisterAttached("Radius", typeof(CornerRadius), typeof(ButtonAttach));
    public static CornerRadius GetRadius(DependencyObject dependencyObject) => (CornerRadius)dependencyObject.GetValue(RadiusProperty);
    public static void SetRadius(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(RadiusProperty, value);

    public static DependencyProperty HoverForegroundProperty = DependencyProperty.RegisterAttached("HoverForeground", typeof(Brush), typeof(ButtonAttach));
    public static Brush GetHoverForeground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(HoverForegroundProperty);
    public static void SetHoverForeground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(HoverForegroundProperty, value);

    public static DependencyProperty PressForegroundProperty = DependencyProperty.RegisterAttached("PressForeground", typeof(Brush), typeof(ButtonAttach));
    public static Brush GetPressForeground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(PressForegroundProperty);
    public static void SetPressForeground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(PressForegroundProperty, value);

    public static DependencyProperty HoverBackgroundProperty = DependencyProperty.RegisterAttached("HoverBackground", typeof(Brush), typeof(ButtonAttach));
    public static Brush GetHoverBackground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(HoverBackgroundProperty);
    public static void SetHoverBackground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(HoverBackgroundProperty, value);

    public static DependencyProperty PressBackgroundProperty = DependencyProperty.RegisterAttached("PressBackground", typeof(Brush), typeof(ButtonAttach));
    public static Brush GetPressBackground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(PressBackgroundProperty);
    public static void SetPressBackground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(PressBackgroundProperty, value);
}
