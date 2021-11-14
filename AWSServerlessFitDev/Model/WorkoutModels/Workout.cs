using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class Workout
    {
        public Guid WorkoutId { get; set; }
        public Guid WorkoutPlanId { get; set; }

        
        public string SerializedWorkoutExercises { get; set; }
        public string SerializedWorkoutSets { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime NewestChangedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
