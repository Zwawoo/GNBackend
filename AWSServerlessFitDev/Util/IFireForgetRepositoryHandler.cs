using AWSServerlessFitDev.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util
{
    public interface IFireForgetRepositoryHandler
    {
        void Execute(Func<IDatabaseService, INotificationService, Task> work);
    }
}
