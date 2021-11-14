using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class WorkoutSet
    {
        public Guid SetId { get; set; }
        public Guid WorkoutId { get; set; }
        public Guid ExerciseId { get; set; }
        public int SetNumber { get; set; }
        public decimal? Weight { get; set; }
        public int? Reps { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }

        
    }
}
