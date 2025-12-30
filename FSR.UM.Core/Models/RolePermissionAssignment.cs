namespace FSR.UM.Core.Models
{
    /// <summary>
    /// Junction table for many-to-many relationship between Roles and Permissions
    /// </summary>
    public class RolePermissionAssignment
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
