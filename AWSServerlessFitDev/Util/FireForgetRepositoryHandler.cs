using AWSServerlessFitDev.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util
{
    public class FireForgetRepositoryHandler : IFireForgetRepositoryHandler
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public FireForgetRepositoryHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Execute(Func<IDatabaseService, INotificationService, Task> work)
        {
            // Fire off the task, but don't await the result
            Task.Run(async () =>
            {
                // Exceptions must be caught
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await work(repository, notificationService);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}
