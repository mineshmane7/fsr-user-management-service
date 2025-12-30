namespace FSR.UM.Core.Models.DTOs
{
    /// <summary>
    /// Simple request to create a new user with basic details and a role
    /// </summary>
    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Role name to assign (e.g., "Admin", "Manager", "User")
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
    }
}
