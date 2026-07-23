namespace EclipseClient.Models;

public class UserAccount
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired =>
        !IsPermanent && ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
}
