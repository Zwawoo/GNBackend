using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model.WorkoutModels
{
    public class Muscle
    {
        public int MuscleId { get; set; }
        public string MuscleName { get; set; }
        public int? Position { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
