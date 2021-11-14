using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public enum SyncDirection
    {
        None = 0,
        ClientToServer = 1,
        ServerToClient = 2,
        BiDirectional = 3
    }
}
