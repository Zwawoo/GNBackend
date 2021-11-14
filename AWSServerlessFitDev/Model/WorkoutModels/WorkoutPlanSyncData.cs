using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class WorkoutPlanSyncData
    {
        public WorkoutPlanSyncData()
        {
            WorkoutPlans = new List<WorkoutPlan>();
            Exercises = new List<Exercise>();
            WorkoutPlanExercises = new List<WorkoutPlanExercise>();
            Muscles = new List<Muscle>();
            Equipment = new List<Equipment>();
        }
        public List<WorkoutPlan> WorkoutPlans { get; set; }
        public List<Exercise> Exercises { get; set; }
        public List<WorkoutPlanExercise> WorkoutPlanExercises { get; set; }
        public List<Muscle> Muscles { get; set; }
        public List<Equipment> Equipment { get; set; }
    }
}
