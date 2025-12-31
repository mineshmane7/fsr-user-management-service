using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSR.UM.Core.Models.DTOs
{
    public class BulkCreateUserResult
    {
        public int TotalRecords { get; set; }
        public int CreatedCount { get; set; }
        public int FailedCount { get; set; }
        public List<BulkUserFailure> Failures { get; set; } = new();
    }
    public class BulkUserFailure
    {
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
