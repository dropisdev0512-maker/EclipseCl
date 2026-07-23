namespace EclipseClient.Models;

public class AppSettings
{
    public bool StreamProof { get; set; }
    public int UiRefreshHz { get; set; } = 144;
    public int BlurRefreshHz { get; set; } = 30;
    public bool LightTheme { get; set; }
    public string AccentColor { get; set; } = "Blue";
    public Dictionary<string, bool> ModuleStates { get; set; } = new();
    public Dictionary<string, Dictionary<string, object>> ModuleSettings { get; set; } = new();
}

public class SessionData
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool RememberMe { get; set; }
}
