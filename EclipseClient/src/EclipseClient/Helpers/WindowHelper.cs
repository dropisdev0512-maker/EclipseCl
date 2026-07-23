using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace EclipseClient.Helpers;

public static class WindowHelper
{
    public static void EnableGlass(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            Services.DwmHelper.ApplyGlassEffect(hwnd, !Services.SettingsService.Current.LightTheme);
        };
    }

    public static void EnableDrag(Window window, UIElement titleBar)
    {
        titleBar.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ClickCount == 2 && window.ResizeMode != ResizeMode.NoResize)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                return;
            }
            window.DragMove();
        };
    }
}

public static class RelayCommand
{
    public static ICommand Create(Action execute, Func<bool>? canExecute = null) =>
        new RelayCommandImpl(execute, canExecute);
}

internal class RelayCommandImpl : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommandImpl(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
}
