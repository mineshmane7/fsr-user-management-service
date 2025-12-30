namespace FSR.UM.Core.Models
{
    /// <summary>
    /// Represents users who are registered with Ping Identity
    /// Only users in this table can be created in the system
    /// </summary>
    public class RegisteredPingUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
