using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;

namespace EclipseClient.Services;

/// <summary>
/// IPC bridge between WPF client and injected EclipseCore.dll via shared memory.
/// </summary>
public static class ModuleBridge
{
    private const string MapName = "EclipseClient_IPC_v1";
    private static MemoryMappedFile? _map;
    private static MemoryMappedViewAccessor? _view;

    public static bool IsConnected { get; private set; }

    public static void Initialize()
    {
        try
        {
            _map = MemoryMappedFile.CreateOrOpen(MapName, 65536);
            _view = _map.CreateViewAccessor();
            IsConnected = true;
            WriteState();
        }
        catch
        {
            IsConnected = false;
        }
    }

    public static void WriteState()
    {
        if (_view == null) return;

        var payload = new BridgePayload
        {
            Modules = SettingsService.Current.ModuleStates,
            Settings = SettingsService.Current.ModuleSettings,
            StreamProof = SettingsService.Current.StreamProof,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        if (bytes.Length >= 65530) return;

        _view.Write(0, bytes.Length);
        _view.WriteArray(4, bytes, 0, bytes.Length);
    }

    public static void NotifyModuleChanged(string moduleId, bool enabled)
    {
        SettingsService.SetModuleEnabled(moduleId, enabled);
        WriteState();
        ModuleEngine.OnModuleToggled(moduleId, enabled);
    }

    public static void Shutdown()
    {
        _view?.Dispose();
        _map?.Dispose();
        IsConnected = false;
    }
}

internal class BridgePayload
{
    public Dictionary<string, bool> Modules { get; set; } = new();
    public Dictionary<string, Dictionary<string, object>> Settings { get; set; } = new();
    public bool StreamProof { get; set; }
    public long Timestamp { get; set; }
}
