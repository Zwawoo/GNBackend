using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReportController : Controller
    {
        IDatabaseService DbService { get; set; }
        INotificationService NotifyService { get; set; }
        public ReportController(Services.IDatabaseService dbService, INotificationService iNotifyService)
        {
            DbService = dbService;
            NotifyService = iNotifyService;
        }

        [HttpPost]
        public async Task<IActionResult> Report([FromQuery] string reportedUser, [FromQuery] long? reportedPost, [FromQuery] long? reportedPostComment, [FromQuery] string reason)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            if(reportedPostComment != null)
            {
                PostComment pc = await DbService.GetPostComment(reportedPostComment.Value);
                reportedPost = pc.PostId;
                reportedUser = pc.UserName;
            }
            else if(reportedPost != null)
            {
                Post p = await DbService.GetPost(reportedPost.Value);
                reportedUser = p.UserName;
            }
            else if (String.IsNullOrWhiteSpace(reportedUser))
            {
                return BadRequest();
            }

            await DbService.InsertReport(authenticatedUserName, reportedUser, reportedPost, reportedPostComment, reason);

            return Ok();
        }

    }
}
