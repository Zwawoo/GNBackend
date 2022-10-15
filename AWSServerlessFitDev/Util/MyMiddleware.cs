using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util
{
    public class MyMiddleware
    {
        private readonly RequestDelegate next;

        public MyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, Services.IDatabaseService dbService, ILogger<MyMiddleware> logger)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                var shouldContinue = await this.BeginInvoke(context, dbService, logger);
                if (shouldContinue)
                    await this.next.Invoke(context);
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                this.EndInvoke(context);


                sw.Stop();
                var elapsedTime = sw.Elapsed.TotalMilliseconds;//sw.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
                logger.LogInformation("ElapsedTime={elapsedms} Path={path}", elapsedTime, context?.Request?.Path);
            }
            catch (Exception ex)
            {
                if (context.RequestAborted.IsCancellationRequested)
                {
                    logger.LogWarning(ex, "RequestAborted. " + ex.Message);
                    return;
                }
                throw;
            }
            finally
            {
                
            }
        }

        private async Task<bool> BeginInvoke(HttpContext context, Services.IDatabaseService dbService, ILogger<MyMiddleware> logger)
        {
            // Do custom work before controller execution
            string callerUserName = String.Empty;
            try
            {
                callerUserName = context?.User?.FindFirst(Constants.UserNameClaim)?.Value;

                if (!String.IsNullOrWhiteSpace(context?.Request?.Headers["X-Forwarded-For"].ToString()))
                {
                    logger?.LogInformation("Forwarded for UserName={username} with Ip={ip} Path={path} Method={method}", callerUserName, context?.Request?.Headers["X-Forwarded-For"].ToString(), context?.Request?.Path, context?.Request?.Method);
                }
                //Guid? subId = Guid.Parse( context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                //if (String.IsNullOrWhiteSpace(callerUserName))
                //{
                //    return false;
                //}

                //Wenn ein User disabled wird, bleiben seine Access Tokens max 1 std gültig. Hier wird bei schreibenden Methoden geprüft, ob der User enabled ist.
                if (HttpMethods.IsPost(context?.Request?.Method) || HttpMethods.IsPut(context?.Request?.Method) || HttpMethods.IsDelete(context?.Request?.Method))
                {
                    if (!String.IsNullOrEmpty(callerUserName))
                    {
                        User u = await dbService.AdminGetUserOnly(callerUserName);
                        if(u != null)
                        {
                            if (u.IsDeactivated || u.IsDeleted)
                                return false;
                        }
                    }
                }


                context.Items.Add(Constants.AuthenticatedUserNameItem, callerUserName);
            }
            catch (Exception ex)
            {
                logger.LogException(callerUserName, ex, context?.Request);
                return false;
            }
            return true;
        }

        private void EndInvoke(HttpContext context)
        {
            // Do custom work after controller execution
        }
    }
}
