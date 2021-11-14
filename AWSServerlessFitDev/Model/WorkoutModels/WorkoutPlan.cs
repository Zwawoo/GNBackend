using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class WorkoutPlan
    {
        public Guid WorkoutPlanId { get; set; }
        public string WorkoutName { get; set; }
        public string UserName { get; set; }      
        public bool IsPublic { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
