using System.Windows;
using System.Windows.Media;

namespace EclipseClient.Themes;

public static class ThemeManager
{
    public static readonly Dictionary<string, Color> AccentColors = new()
    {
        ["Blue"] = Color.FromRgb(64, 156, 255),
        ["Red"] = Color.FromRgb(255, 82, 82),
        ["Yellow"] = Color.FromRgb(255, 214, 64),
        ["Green"] = Color.FromRgb(64, 255, 128),
        ["Purple"] = Color.FromRgb(168, 85, 247),
        ["Pink"] = Color.FromRgb(255, 105, 180),
        ["Cyan"] = Color.FromRgb(0, 229, 255),
    };

    public static void ApplyAccent(string colorName)
    {
        if (!AccentColors.TryGetValue(colorName, out var accent))
            accent = AccentColors["Blue"];

        Application.Current.Resources["AccentBrush"] = new SolidColorBrush(accent);
        Application.Current.Resources["AccentDimBrush"] = new SolidColorBrush(
            Color.FromArgb(80, accent.R, accent.G, accent.B));
        Application.Current.Resources["AccentGlowBrush"] = new SolidColorBrush(
            Color.FromArgb(160, accent.R, accent.G, accent.B));
    }

    public static void ApplyTheme(bool light)
    {
        var bg = light ? Color.FromArgb(220, 240, 240, 245) : Color.FromArgb(210, 8, 8, 14);
        var panel = light ? Color.FromArgb(180, 255, 255, 255) : Color.FromArgb(140, 16, 16, 24);
        var sidebar = light ? Color.FromArgb(200, 230, 230, 240) : Color.FromArgb(160, 10, 10, 18);
        var text = light ? Color.FromRgb(20, 20, 30) : Color.FromRgb(230, 235, 255);
        var textDim = light ? Color.FromRgb(80, 80, 100) : Color.FromRgb(140, 145, 170);
        var border = light ? Color.FromArgb(60, 0, 0, 0) : Color.FromArgb(50, 255, 255, 255);

        Application.Current.Resources["BgBrush"] = new SolidColorBrush(bg);
        Application.Current.Resources["PanelBrush"] = new SolidColorBrush(panel);
        Application.Current.Resources["SidebarBrush"] = new SolidColorBrush(sidebar);
        Application.Current.Resources["TextBrush"] = new SolidColorBrush(text);
        Application.Current.Resources["TextDimBrush"] = new SolidColorBrush(textDim);
        Application.Current.Resources["BorderBrush"] = new SolidColorBrush(border);
    }

    public static void LoadSavedTheme()
    {
        var s = Services.SettingsService.Current;
        ApplyTheme(s.LightTheme);
        ApplyAccent(s.AccentColor);
    }
}
