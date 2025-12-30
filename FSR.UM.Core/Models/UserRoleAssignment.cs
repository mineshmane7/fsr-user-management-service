namespace FSR.UM.Core.Models
{
    /// <summary>
    /// Junction table for many-to-many relationship between Users and Roles
    /// </summary>
    public class UserRoleAssignment
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public Guid? AssignedBy { get; set; } // User who assigned this role
    }
}
