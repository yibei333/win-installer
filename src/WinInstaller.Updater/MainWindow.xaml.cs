using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WinInstaller.Updater;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    readonly Config _config;
    CancellationTokenSource CancellationTokenSource = new();

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
        if (sender is not Button button) return;
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

    private void CheckUpdate(object sender, RoutedEventArgs e)
    {
        Check();
    }

    private void DownloadUpdate(object sender, RoutedEventArgs e)
    {
        Update();
    }

    private void CancleUpdate(object sender, RoutedEventArgs e)
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource = new CancellationTokenSource();
        Progress = null;
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

    bool updating;
    public bool Updating
    {
        get => updating;
        set
        {
            if (updating != value)
            {
                updating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Updating)));
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

            if (!string.IsNullOrWhiteSpace(LastVersion) && LastVersion != CurrentVersion) HasUpdate = true;
            else HasUpdate = false;
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

    ProgressModel progress;
    public ProgressModel Progress
    {
        get => progress;
        set
        {
            if (progress != value)
            {
                progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
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
        if (CheckUrls.Count(x => !string.IsNullOrWhiteSpace(x.Value)) <= 0)
        {
            MessageBox.Show("请配置检查更新的地址");
            return;
        }

        Running = true;
        await Task.Yield();
        _config.Set();
        try
        {
            var flag = false;
            var message = string.Empty;
            foreach (var url in CheckUrls)
            {
                try
                {
                    LastVersion = await HttpHelper.Get(url.Value);
                    flag = true;
                    break;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
            }
            if (!flag) throw new Exception($"检查更新失败:{message}");
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
        if (DownloadUrls.Count(x => !string.IsNullOrWhiteSpace(x.Value)) <= 0)
        {
            MessageBox.Show("请配置下载更新的地址");
            return;
        }

        Running = true;
        Updating = true;
        await Task.Yield();
        _config.Set();
        try
        {
            var directory = Path.Combine(_config.InstallLocation, "packages");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{_config.DisplayName}.exe");

            var flag = false;
            var message = string.Empty;
            foreach (var url in DownloadUrls)
            {
                try
                {
                    await HttpHelper.Download(url.Value, path, progress =>
                    {
                        Progress = progress;
                    }, CancellationTokenSource.Token);
                    flag = true;
                    break;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                if (!flag) throw new Exception($"更新失败:{message}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(path)
                    {
                        WorkingDirectory = directory
                    }
                };
                process.Start();
                process.WaitForExit();
                Application.Current.Shutdown();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            Running = false;
            Updating = false;
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

public class BoolReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = bool.TryParse(value?.ToString(), out var result) && result;
        return !boolValue;
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

public class ProgressModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    long _total;
    public long Total
    {
        get => _total;
        set
        {
            if (value != _total)
            {
                _total = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Total)));
            }
        }
    }

    long _handled;
    public long Handled
    {
        get => _handled;
        set
        {
            if (value != _handled)
            {
                _handled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Handled)));
            }
        }
    }

    double _progress;
    public double Progress
    {
        get => _progress;
        set
        {
            if (value != _progress)
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));
            }
        }
    }

    string _speed;
    public string Speed
    {
        get => _speed;
        set
        {
            if (value != _speed)
            {
                _speed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Speed)));
            }
        }
    }
}