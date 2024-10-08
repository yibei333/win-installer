using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WinInstaller.Attaches;

public static class ButtonForegroundAttach
{
    public static DependencyProperty HoverForegroundProperty = DependencyProperty.RegisterAttached("HoverForeground", typeof(Brush), typeof(ButtonForegroundAttach), new FrameworkPropertyMetadata(ButtonStatePropertyChangedCallback));

    public static Brush GetHoverForeground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(HoverForegroundProperty);

    public static void SetHoverForeground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(HoverForegroundProperty, value);

    public static DependencyProperty PressForegroundProperty = DependencyProperty.RegisterAttached("PressForeground", typeof(Brush), typeof(ButtonForegroundAttach), new FrameworkPropertyMetadata(ButtonStatePropertyChangedCallback));

    public static Brush GetPressForeground(DependencyObject dependencyObject) => (Brush)dependencyObject.GetValue(PressForegroundProperty);

    public static void SetPressForeground(DependencyObject dependencyObject, object value) => dependencyObject.SetValue(PressForegroundProperty, value);

    static readonly Dictionary<Button, Brush> _hoverCache = new();
    static void ButtonStatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Button button && _hoverCache.All(x => x.Key != button))
        {
            var pressDescriptor = DependencyPropertyDescriptor.FromProperty(ButtonBase.IsPressedProperty, typeof(Button));
            pressDescriptor.AddValueChanged(button, SetBackground);

            var hoverDescriptor = DependencyPropertyDescriptor.FromProperty(UIElement.IsMouseOverProperty, typeof(Button));
            hoverDescriptor.AddValueChanged(button, SetBackground);

            _hoverCache.Add(button, button.Foreground);
        }
    }

    static void SetBackground(object sender, EventArgs args)
    {
        if (sender is Button button)
        {
            if (button.IsPressed) button.Foreground = GetPressForeground(button);
            else if (button.IsMouseOver) button.Foreground = GetHoverForeground(button);
            else button.Foreground = _hoverCache[button];
        }
    }
}
