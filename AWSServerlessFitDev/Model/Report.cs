using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Report
    {
        public long ReportId { get; set; }
        public string ReportedBy { get; set; }
        public string ReportedUser { get; set; }
        public long? ReportedPost { get; set; }
        public long? ReportedPostComment { get; set; }
        public string Reason { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsHandled { get; set; }
        public string HandledBy { get; set; }
        public DateTime? HandledAt { get; set; }
        public string ActionTaken { get; set; }
        public string GroupName { get; set; }
    }
}
