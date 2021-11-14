using AWSServerlessFitDev.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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

        public async Task Invoke(HttpContext context, Services.IDatabaseService dbService)
        {
            var shouldContinue = await this.BeginInvoke(context, dbService);
            if(shouldContinue)
                await this.next.Invoke(context);
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            this.EndInvoke(context);
        }

        private async Task<bool> BeginInvoke(HttpContext context, Services.IDatabaseService dbService)
        {
            // Do custom work before controller execution
            try
            {
                System.Console.WriteLine("Forwarded for Ip: " + context?.Request?.Headers["X-Forwarded-For"].ToString());

                string callerUserName = context?.User?.FindFirst(Constants.UserNameClaim)?.Value;
                //Guid? subId = Guid.Parse( context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (String.IsNullOrWhiteSpace(callerUserName))
                {
                    return false;
                }
                
                //Wenn ein User disabled wird, bleiben seine Access Tokens max 1 std gültig. Hier wird bei schreibenden Methoden geprüft, ob der User enabled ist.
                if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method))
                {
                    User u = dbService.AdminGetUserOnly(callerUserName);
                    if (u.IsDeactivated || u.IsDeleted)
                        return false;
                }




                context.Items.Add(Constants.AuthenticatedUserNameItem, callerUserName);
            }
            catch (Exception ex)
            {
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
