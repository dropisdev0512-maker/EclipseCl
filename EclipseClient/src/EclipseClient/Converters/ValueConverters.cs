using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace EclipseClient.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility.Visible;
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b ? !b : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is bool b ? !b : value;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value == null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class AccentBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, Color> Colors = new()
    {
        ["Blue"] = Color.FromRgb(64, 156, 255),
        ["Red"] = Color.FromRgb(255, 82, 82),
        ["Yellow"] = Color.FromRgb(255, 214, 64),
        ["Green"] = Color.FromRgb(64, 255, 128),
        ["Purple"] = Color.FromRgb(168, 85, 247),
        ["Pink"] = Color.FromRgb(255, 105, 180),
        ["Cyan"] = Color.FromRgb(0, 229, 255),
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = value?.ToString() ?? "Blue";
        if (!Colors.TryGetValue(name, out var color))
            color = Colors["Blue"];
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class ConnectionStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? "MC Connected" : "MC Disconnected";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class ConnectionColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        new SolidColorBrush(value is true
            ? Color.FromRgb(64, 255, 128)
            : Color.FromRgb(255, 82, 82));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
