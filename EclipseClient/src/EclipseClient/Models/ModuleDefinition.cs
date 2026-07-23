namespace EclipseClient.Models;

public enum ModuleCategory
{
    Spvp,
    Mace,
    Misc,
    Movement,
    Prevent
}

public enum SettingType
{
    Toggle,
    Slider,
    Number,
    Text,
    Keybind,
    Dropdown
}

public class ModuleSetting
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public SettingType Type { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Step { get; set; } = 1;
    public string[]? Options { get; set; }
    public object Value { get; set; } = false;
}

public class ModuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModuleCategory Category { get; set; }
    public string IconPath { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public List<ModuleSetting> Settings { get; set; } = new();
}
