using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class WorkoutSyncData
    {
        public List<Workout> Workouts { get; set; }
        public List<WorkoutExercise> WorkoutExercises { get; set; }
        public List<WorkoutSet> WorkoutSets { get; set; }
    }
}
