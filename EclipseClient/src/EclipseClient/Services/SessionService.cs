using System.IO;
using System.Text.Json;
using EclipseClient.Models;

namespace EclipseClient.Services;

public static class SessionService
{
    private static readonly string SessionFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EclipseClient", "session.json");

    public static SessionData? LoadSession()
    {
        if (!File.Exists(SessionFile)) return null;

        try
        {
            var json = File.ReadAllText(SessionFile);
            var session = JsonSerializer.Deserialize<SessionData>(json);
            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                ClearSession();
                return null;
            }
            return session;
        }
        catch
        {
            return null;
        }
    }

    public static void SaveSession(string email, bool rememberMe)
    {
        if (!rememberMe)
        {
            ClearSession();
            return;
        }

        var session = new SessionData
        {
            Email = email,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            RememberMe = true
        };

        Directory.CreateDirectory(Path.GetDirectoryName(SessionFile)!);
        File.WriteAllText(SessionFile, JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void ClearSession()
    {
        if (File.Exists(SessionFile)) File.Delete(SessionFile);
    }
}
