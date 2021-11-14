using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class SyncRequest<T>
    {
        public T ObjectChangedOnClient { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public SyncDirection SyncDirection { get; set; }
    }
}
