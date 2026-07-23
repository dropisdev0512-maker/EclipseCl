using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EclipseClient.Models;

namespace EclipseClient.Services;

public static class AuthService
{
    private static readonly string DataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EclipseClient");
    private static readonly string UsersFile = Path.Combine(DataDir, "users.json");

    private static List<UserAccount> _users = new();
    private static UserAccount? _currentUser;

    public static UserAccount? CurrentUser => _currentUser;

    static AuthService()
    {
        Directory.CreateDirectory(DataDir);
        LoadUsers();
        EnsureDefaultAdmin();
    }

    public static bool Login(string email, string password, out string error)
    {
        error = string.Empty;
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            error = "Invalid email or password.";
            return false;
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            error = "Invalid email or password.";
            return false;
        }

        if (user.IsExpired)
        {
            error = "Your account has expired. Contact an administrator.";
            return false;
        }

        _currentUser = user;
        return true;
    }

    public static void Logout() => _currentUser = null;

    public static bool IsAdmin => _currentUser?.IsAdmin == true;

    public static IReadOnlyList<UserAccount> GetAllUsers() =>
        _users.OrderBy(u => u.Email).ToList();

    public static bool AddUser(string email, string password, bool isPermanent, DateTime? expiry, out string error)
    {
        error = string.Empty;
        email = email.Trim();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            error = "Enter a valid email address.";
            return false;
        }

        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            error = "User already exists.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
        {
            error = "Password must be at least 4 characters.";
            return false;
        }

        _users.Add(new UserAccount
        {
            Email = email,
            PasswordHash = HashPassword(password),
            IsPermanent = isPermanent,
            ExpiryDate = isPermanent ? null : expiry?.ToUniversalTime(),
            IsAdmin = false
        });

        SaveUsers();
        return true;
    }

    public static bool RemoveUser(string email)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null || user.IsAdmin) return false;

        _users.Remove(user);
        SaveUsers();
        return true;
    }

    public static bool UpdateUser(string email, bool isPermanent, DateTime? expiry)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null || user.IsAdmin) return false;

        user.IsPermanent = isPermanent;
        user.ExpiryDate = isPermanent ? null : expiry?.ToUniversalTime();
        SaveUsers();
        return true;
    }

    public static bool ChangePassword(string email, string newPassword, out string error)
    {
        error = string.Empty;
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            error = "User not found.";
            return false;
        }

        user.PasswordHash = HashPassword(newPassword);
        SaveUsers();
        return true;
    }

    private static void EnsureDefaultAdmin()
    {
        const string adminEmail = "dropisnotdev0512@gmail.com";
        const string adminPass = "anas@drop007";

        var admin = _users.FirstOrDefault(u =>
            u.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase));

        if (admin == null)
        {
            _users.Add(new UserAccount
            {
                Email = adminEmail,
                PasswordHash = HashPassword(adminPass),
                IsAdmin = true,
                IsPermanent = true
            });
            SaveUsers();
        }
    }

    private static void LoadUsers()
    {
        if (!File.Exists(UsersFile)) return;

        try
        {
            var json = File.ReadAllText(UsersFile);
            _users = JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new();
        }
        catch
        {
            _users = new();
        }
    }

    private static void SaveUsers()
    {
        var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UsersFile, json);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "EclipseSalt_v1"));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyPassword(string password, string hash) =>
        HashPassword(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
}
