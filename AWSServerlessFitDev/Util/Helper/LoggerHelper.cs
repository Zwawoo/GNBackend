using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util.Helper
{
    public static class LoggerHelper
    {
        public static void LogException(this ILogger logger, string authenticatedUserName, Exception ex)
        {
            logger.LogError("Exception for UserName={userName} Exception={exception}", authenticatedUserName, ex.ToString());
        }
    }
}
