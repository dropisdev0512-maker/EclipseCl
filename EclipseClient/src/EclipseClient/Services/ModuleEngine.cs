using EclipseClient.Models;

namespace EclipseClient.Services;

/// <summary>
/// Runs active module logic on the client side (external coordination).
/// </summary>
public static class ModuleEngine
{
    private static readonly Dictionary<string, IModuleHandler> Handlers = new()
    {
        ["auto_inventory_totem"] = new AutoInventoryTotemHandler()
    };

    private static Timer? _tickTimer;

    public static void Start()
    {
        _tickTimer?.Dispose();
        _tickTimer = new Timer(Tick, null, 0, 50);
    }

    public static void Stop()
    {
        _tickTimer?.Dispose();
        _tickTimer = null;
    }

    public static void OnModuleToggled(string moduleId, bool enabled)
    {
        if (Handlers.TryGetValue(moduleId, out var handler))
        {
            if (enabled) handler.OnEnable();
            else handler.OnDisable();
        }

        ModuleBridge.WriteState();
    }

    private static void Tick(object? state)
    {
        if (!ProcessMonitorService.IsMinecraftRunning(out _)) return;

        foreach (var module in ModuleRegistry.All.Where(m => m.Enabled))
        {
            if (Handlers.TryGetValue(module.Id, out var handler))
                handler.OnTick(module);
        }
    }
}

public interface IModuleHandler
{
    void OnEnable();
    void OnDisable();
    void OnTick(ModuleDefinition module);
}

/// <summary>
/// Auto Inventory Totem - manages totems in offhand and configured hotbar slot.
/// </summary>
public class AutoInventoryTotemHandler : IModuleHandler
{
    private bool _inventoryOpen;
    private int _lastTotemCount;
    private bool _totemJustPopped;

    public void OnEnable() { }

    public void OnDisable() { }

    public void OnTick(ModuleDefinition module)
    {
        if (!SettingsService.GetModuleEnabled("auto_inventory_totem")) return;

        var minTotems = (int)SettingsService.GetModuleSetting("auto_inventory_totem", "min_totems", 2.0);
        var hotbarSlot = SettingsService.GetModuleSetting("auto_inventory_totem", "slot", "1");
        var autoRefill = SettingsService.GetModuleSetting("auto_inventory_totem", "auto_refill", true);

        // Simulated inventory state (injected DLL provides real data via IPC)
        var totemCount = SimulateTotemCount();
        _inventoryOpen = SimulateInventoryOpen();

        if (totemCount < minTotems)
        {
            _lastTotemCount = totemCount;
            return;
        }

        if (_inventoryOpen)
        {
            // When inventory opens: equip offhand + hotbar backup
            ExecuteTotemMove("offhand");
            ExecuteTotemMove($"hotbar_{hotbarSlot}");
        }

        if (autoRefill && _totemJustPopped)
        {
            // Totem pop detected: immediately refill offhand and backup
            ExecuteTotemMove("offhand");
            ExecuteTotemMove($"hotbar_{hotbarSlot}");
            _totemJustPopped = false;
        }

        if (_lastTotemCount > totemCount && totemCount >= minTotems - 1)
            _totemJustPopped = true;

        _lastTotemCount = totemCount;
    }

    private static void ExecuteTotemMove(string target)
    {
        // Sent to injected DLL via shared memory bridge
        ModuleBridge.WriteState();
        DebugLog($"[AutoInventoryTotem] Moving totem to {target}");
    }

    private static int SimulateTotemCount() => 3;
    private static bool SimulateInventoryOpen() => false;

    private static void DebugLog(string msg) =>
        System.Diagnostics.Debug.WriteLine(msg);
}
