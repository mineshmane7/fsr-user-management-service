namespace FSR.UM.Core.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties for many-to-many relationships
        public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
        public ICollection<RolePermissionAssignment> RolePermissionAssignments { get; set; } = new List<RolePermissionAssignment>();
        
        // Helper property to get permissions (not mapped to database)
        public ICollection<Permission> Permissions => RolePermissionAssignments?.Select(rpa => rpa.Permission).ToList() ?? new List<Permission>();
    }
}
