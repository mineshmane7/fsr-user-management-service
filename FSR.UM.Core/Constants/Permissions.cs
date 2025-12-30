namespace FSR.UM.Core.Constants
{
    /// <summary>
    /// Standard permissions used across the application
    /// </summary>
    public static class Permissions
    {
        public const string Create = "Create";
        public const string View = "View";
        public const string Edit = "Edit";
        public const string Archive = "Archive";
        public const string Delete = "Delete";
        public const string BulkEdit = "BulkEdit";
        public const string BulkExport = "BulkExport";
        public const string BulkImport = "BulkImport";

        /// <summary>
        /// Get all standard permissions
        /// </summary>
        public static IEnumerable<string> GetAllPermissions()
        {
            return new[]
            {
                Create,
                View,
                Edit,
                Archive,
                Delete,
                BulkEdit,
                BulkExport,
                BulkImport
            };
        }
    }
}
