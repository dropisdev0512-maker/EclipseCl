using EclipseClient.Models;

namespace EclipseClient.Services;

public static class ModuleRegistry
{
    private static List<ModuleDefinition>? _modules;

    public static IReadOnlyList<ModuleDefinition> All => _modules ??= BuildModules();

    public static IEnumerable<ModuleDefinition> GetByCategory(ModuleCategory category) =>
        All.Where(m => m.Category == category);

    private static List<ModuleDefinition> BuildModules()
    {
        var modules = new List<ModuleDefinition>
        {
            // SPVP / Combat
            Mod("auto_crystal", "Auto Crystal", "Automatically places & breaks end crystals.",
                ModuleCategory.Spvp, "crystal.png",
                Toggle("range", "Range", 1, 6, 4.5),
                Toggle("speed", "Speed", 1, 10, 5),
                ToggleBool("rotate", "Rotate", true),
                ToggleBool("silent", "Silent", false)),

            Mod("hit_crystal", "Hit Crystal", "Triggers packet actions around crystal coordinates.",
                ModuleCategory.Spvp, "crystal.png",
                Slider("range", "Range", 1, 6, 4),
                Slider("speed", "Speed", 1, 10, 6)),

            Mod("crystal_optimizer", "Crystal Optimizer", "Reduces entity tick response updates for crystal performance.",
                ModuleCategory.Spvp, "crystal.png",
                Slider("speed", "Speed", 1, 10, 7)),

            Mod("fast_place", "Fast Place", "Maintains optimal latency variables for item placement.",
                ModuleCategory.Spvp, "lightning.png"),

            Mod("auto_inventory_totem", "Auto Inventory Totem",
                "Automatically manages Totems of Undying in your inventory, ensuring you always have one ready in your offhand and a backup in your hotbar.",
                ModuleCategory.Spvp, "totem.png",
                Dropdown("slot", "Hotbar Slot", new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "Offhand" }, "1"),
                Slider("min_totems", "Min Totems", 1, 9, 2),
                ToggleBool("auto_refill", "Auto Refill", true)),

            Mod("reach", "Reach", "Extends attack reach distance.",
                ModuleCategory.Spvp, "sword.png",
                Slider("range", "Range", 3, 6, 3.5)),

            Mod("aim_assist", "Aim Assist", "Assists aim toward nearby targets.",
                ModuleCategory.Spvp, "crosshair.png",
                Slider("range", "Range", 1, 6, 4),
                Slider("speed", "Speed", 1, 10, 5),
                Slider("fov", "FOV", 10, 180, 90)),

            Mod("trigger_bot", "Trigger Bot", "Automatically attacks when crosshair is on target.",
                ModuleCategory.Spvp, "crosshair.png",
                Slider("delay", "Delay (ms)", 0, 500, 50),
                Slider("range", "Range", 1, 6, 3),
                Slider("hit_chance", "Hit Chance %", 1, 100, 85)),

            Mod("auto_pot", "Auto Pot", "Automatically throws health potions.",
                ModuleCategory.Spvp, "potion.png",
                Slider("health", "Health %", 1, 100, 35),
                Dropdown("type", "Potion Type", new[] { "Instant Health", "Instant Health II" }, "Instant Health")),

            Mod("no_miss_delay", "No Miss Delay", "Removes attack miss cooldown.",
                ModuleCategory.Spvp, "sword.png"),

            Mod("disable_shields", "Disable Shields", "Automatically disables opponent shields.",
                ModuleCategory.Spvp, "shield.png"),

            Mod("totem_offhand", "Totem Offhand", "Keeps totem in offhand slot.",
                ModuleCategory.Spvp, "totem.png"),

            Mod("hitboxes", "Hitboxes", "Expands entity hitboxes.",
                ModuleCategory.Spvp, "hitbox.png",
                Slider("size", "Size", 1, 3, 1.2)),

            // MACE
            Mod("shield_disabler", "Shield Disabler", "Disables opponent shields during mace combat.",
                ModuleCategory.Mace, "shield.png"),

            Mod("auto_jump_reset", "Auto Jump Reset", "Automatically resets jumps for optimal knockback.",
                ModuleCategory.Mace, "jump.png",
                Slider("delay", "Delay (ms)", 0, 500, 100)),

            Mod("auto_wtap", "Auto WTap", "Automatically W-taps for combo damage.",
                ModuleCategory.Mace, "sword.png",
                Slider("delay", "Delay (ms)", 0, 500, 80)),

            Mod("no_jump_delay", "No Jump Delay", "Removes jump input delay.",
                ModuleCategory.Mace, "jump.png"),

            Mod("auto_clicker", "Auto Clicker", "Automated left-click at configurable CPS.",
                ModuleCategory.Mace, "click.png",
                Slider("cps", "CPS", 1, 20, 12)),

            Mod("key_pearl", "Key Pearl", "Throws ender pearl on keybind.",
                ModuleCategory.Mace, "pearl.png",
                Keybind("key", "Keybind", "R")),

            // MISC
            Mod("anchor_macro", "Anchor Macro", "Rapidly activates respawn anchors.",
                ModuleCategory.Misc, "anchor.png",
                Slider("delay", "Delay (ms)", 10, 500, 100),
                ToggleBool("random_delay", "Random Delay", false)),

            Mod("safe_anchor_macro", "Safe Anchor Macro", "Executes anchor macros based on health threshold.",
                ModuleCategory.Misc, "anchor.png",
                Slider("delay", "Delay (ms)", 10, 500, 150),
                Slider("health", "Health Threshold", 5, 40, 10)),

            Mod("stream_proof", "Stream Proof", "Hides GUI from OBS and recording software.",
                ModuleCategory.Misc, "stealth.png"),

            Mod("csrss_bypass", "CSRSS Bypass", "Bypasses CSRSS detection mechanisms.",
                ModuleCategory.Misc, "stealth.png"),

            Mod("steam_proof", "Steam Proof", "Steam overlay bypass for undetected gameplay.",
                ModuleCategory.Misc, "stealth.png"),

            Mod("hover_totem", "Hover Totem", "Totem hovers in offhand without inventory open.",
                ModuleCategory.Misc, "totem.png"),

            Mod("free_cam", "Free Cam", "Free camera movement independent of player body.",
                ModuleCategory.Misc, "eye.png",
                Slider("speed", "Speed", 1, 10, 5)),

            Mod("auto_firework", "Auto Firework", "Auto-uses fireworks for elytra flight.",
                ModuleCategory.Misc, "firework.png",
                Slider("delay", "Delay (ms)", 100, 2000, 500)),

            Mod("fake_lag", "Fake Lag", "Simulates network lag to opponents.",
                ModuleCategory.Misc, "lag.png",
                Slider("delay", "Delay (ms)", 50, 500, 150)),

            Mod("pack_spoof", "Pack Spoof", "Spoofs texture pack to server.",
                ModuleCategory.Misc, "pack.png",
                Text("pack_name", "Pack Name", "Default")),

            Mod("full_bright", "Full Bright", "Maximum brightness without night vision potion.",
                ModuleCategory.Misc, "bright.png",
                Slider("gamma", "Gamma", 1, 10, 10)),

            Mod("auto_double_hand", "Auto Double Hand", "Automatically switches to totem when low health.",
                ModuleCategory.Misc, "totem.png"),

            Mod("auto_pot_refill", "Auto Pot Refill", "Refills hotbar with potions from inventory.",
                ModuleCategory.Misc, "potion.png",
                Dropdown("type", "Potion Type", new[] { "Instant Health", "Instant Health II" }, "Instant Health"),
                Slider("min_count", "Min Count", 1, 64, 8)),

            Mod("double_anchor", "Double Anchor", "Places and charges two anchors rapidly.",
                ModuleCategory.Misc, "anchor.png",
                Slider("delay", "Delay (ms)", 10, 500, 80)),

            // PREVENT
            Mod("auto_xp", "Auto XP", "Auto-collects XP orbs within range.",
                ModuleCategory.Prevent, "xp.png",
                Slider("range", "Range", 1, 6, 3)),

            Mod("ping_spoof", "Ping Spoof", "Spoofs ping to appear laggy or stable.",
                ModuleCategory.Prevent, "lag.png",
                Slider("ping", "Ping (ms)", 50, 500, 100)),
        };

        foreach (var m in modules)
        {
            m.Enabled = SettingsService.GetModuleEnabled(m.Id);
            foreach (var s in m.Settings)
            {
                s.Value = SettingsService.GetModuleSetting(m.Id, s.Key, s.Value);
            }
        }

        return modules;
    }

    private static string _modId = "";

    private static ModuleDefinition Mod(string id, string name, string desc, ModuleCategory cat, string icon,
        params ModuleSetting[] settings)
    {
        _modId = id;
        return new ModuleDefinition
        {
            Id = id,
            Name = name,
            Description = desc,
            Category = cat,
            IconPath = $"pack://application:,,,/Assets/Icons/{icon}",
            Settings = settings.ToList()
        };
    }

    private static ModuleSetting Slider(string key, string label, double min, double max, double def) => new()
    {
        Key = key, Label = label, Type = SettingType.Slider,
        Min = min, Max = max, Step = key.Contains("range") || key == "size" ? 0.1 : 1,
        Value = SettingsService.GetModuleSetting(_modId, key, def)
    };

    private static ModuleSetting ToggleBool(string key, string label, bool def) => new()
    {
        Key = key, Label = label, Type = SettingType.Toggle,
        Value = SettingsService.GetModuleSetting(_modId, key, def)
    };

    private static ModuleSetting Toggle(string key, string label, double min, double max, double def) =>
        Slider(key, label, min, max, def);

    private static ModuleSetting Dropdown(string key, string label, string[] options, string def) => new()
    {
        Key = key, Label = label, Type = SettingType.Dropdown,
        Options = options,
        Value = SettingsService.GetModuleSetting(_modId, key, def)
    };

    private static ModuleSetting Text(string key, string label, string def) => new()
    {
        Key = key, Label = label, Type = SettingType.Text,
        Value = SettingsService.GetModuleSetting(_modId, key, def)
    };

    private static ModuleSetting Keybind(string key, string label, string def) => new()
    {
        Key = key, Label = label, Type = SettingType.Keybind,
        Value = SettingsService.GetModuleSetting(_modId, key, def)
    };
}
