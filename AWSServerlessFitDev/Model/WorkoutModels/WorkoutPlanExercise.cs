using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class WorkoutPlanExercise
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid ExerciseId { get; set; }
        public int Position { get; set; }
        public int SetCount { get; set; }
        //public int SetTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
