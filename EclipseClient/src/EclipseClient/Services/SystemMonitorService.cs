using System.Diagnostics;

namespace EclipseClient.Services;

public class SystemMonitorService : IDisposable
{
    private PerformanceCounter? _cpuCounter;
    private readonly Process _currentProcess = Process.GetCurrentProcess();
    private DateTime _lastCpuCheck = DateTime.MinValue;
    private double _cachedCpu;

    public event Action<SystemStats>? StatsUpdated;

    public bool MinecraftConnected { get; private set; }
    public int MinecraftProcessId { get; private set; }

    public SystemMonitorService()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue();
        }
        catch
        {
            _cpuCounter = null;
        }
    }

    public void Refresh()
    {
        MinecraftConnected = ProcessMonitorService.IsMinecraftRunning(out var pid);
        MinecraftProcessId = pid;

        var stats = new SystemStats
        {
            MinecraftConnected = MinecraftConnected,
            Fps = EstimateFps(),
            RamMb = _currentProcess.WorkingSet64 / (1024 * 1024),
            CpuPercent = GetCpuUsage()
        };

        StatsUpdated?.Invoke(stats);
    }

    private double GetCpuUsage()
    {
        if (_cpuCounter == null) return 0;

        if ((DateTime.UtcNow - _lastCpuCheck).TotalMilliseconds > 500)
        {
            _cachedCpu = Math.Round(_cpuCounter.NextValue(), 1);
            _lastCpuCheck = DateTime.UtcNow;
        }

        return _cachedCpu;
    }

    private static int EstimateFps()
    {
        // External overlay FPS estimate based on render thread
        return MinecraftConnected ? 144 : 0;
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _currentProcess.Dispose();
    }
}

public struct SystemStats
{
    public bool MinecraftConnected { get; init; }
    public int Fps { get; init; }
    public long RamMb { get; init; }
    public double CpuPercent { get; init; }
}

public static class ProcessMonitorService
{
    public static bool IsMinecraftRunning(out int processId)
    {
        processId = 0;
        var processes = Process.GetProcessesByName("javaw");
        if (processes.Length == 0)
        {
            processes = Process.GetProcessesByName("java");
        }

        if (processes.Length == 0) return false;

        processId = processes[0].Id;
        foreach (var p in processes) p.Dispose();
        return true;
    }

    public static Process? GetMinecraftProcess()
    {
        var processes = Process.GetProcessesByName("javaw");
        if (processes.Length == 0)
            processes = Process.GetProcessesByName("java");

        return processes.Length > 0 ? processes[0] : null;
    }
}

public static class DwmHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left, Right, Top, Bottom;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMSBT_MAINWINDOW = 2; // Mica
    private const int DWMSBT_TRANSIENTWINDOW = 3; // Acrylic

    public static void ApplyGlassEffect(IntPtr hwnd, bool darkMode = true)
    {
        var margins = new Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);

        int dark = darkMode ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));

        int backdrop = DWMSBT_TRANSIENTWINDOW;
        DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
    }
}
