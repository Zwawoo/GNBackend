using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class Exercise
    {
        public Guid ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public string UserName { get; set; }
        public bool IsCustom { get; set; }
        public string Description { get; set; }
        public int EquipmentId { get; set; }
        public int MuscleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
