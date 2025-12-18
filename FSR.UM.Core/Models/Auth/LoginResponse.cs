public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }

    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public IEnumerable<string> Roles { get; set; } = [];
    public IEnumerable<string> Permissions { get; set; } = [];
}
