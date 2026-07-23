using System.IO;
using System.Text.Json;
using EclipseClient.Models;

namespace EclipseClient.Services;

public static class SettingsService
{
    private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EclipseClient", "settings.json");

    public static AppSettings Current { get; private set; } = new();

    static SettingsService()
    {
        Load();
    }

    public static void Load()
    {
        if (!File.Exists(SettingsFile))
        {
            Current = new AppSettings();
            return;
        }

        try
        {
            var json = File.ReadAllText(SettingsFile);
            Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            Current = new AppSettings();
        }
    }

    public static void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
        var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFile, json);
    }

    public static void SetModuleEnabled(string moduleId, bool enabled)
    {
        Current.ModuleStates[moduleId] = enabled;
        Save();
    }

    public static bool GetModuleEnabled(string moduleId) =>
        Current.ModuleStates.TryGetValue(moduleId, out var v) && v;

    public static void SetModuleSetting(string moduleId, string key, object value)
    {
        if (!Current.ModuleSettings.ContainsKey(moduleId))
            Current.ModuleSettings[moduleId] = new Dictionary<string, object>();

        Current.ModuleSettings[moduleId][key] = value;
        Save();
    }

    public static T GetModuleSetting<T>(string moduleId, string key, T defaultValue)
    {
        if (!Current.ModuleSettings.TryGetValue(moduleId, out var settings))
            return defaultValue;

        if (!settings.TryGetValue(key, out var raw))
            return defaultValue;

        try
        {
            if (raw is JsonElement el)
                return el.Deserialize<T>() ?? defaultValue;
            if (raw is T typed) return typed;
            return (T)Convert.ChangeType(raw, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}
