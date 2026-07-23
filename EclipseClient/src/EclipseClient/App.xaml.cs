using System.Windows;
using EclipseClient.Services;
using EclipseClient.Themes;

namespace EclipseClient;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.LoadSavedTheme();
        ModuleEngine.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ModuleEngine.Stop();
        ModuleBridge.Shutdown();
        base.OnExit(e);
    }
}
