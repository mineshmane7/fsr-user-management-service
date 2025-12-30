namespace FSR.UM.Core.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<RolePermissionAssignment> RolePermissionAssignments { get; set; } = new List<RolePermissionAssignment>();
    }
}
