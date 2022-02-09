using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    //[AllowAnonymous]
    //[Route("api/v1/[controller]")]
    //[ApiController]
    //public class TestController : Controller
    //{
        //IDatabaseService DbService { get; set; }
        //IS3Service S3Client { get; set; }
        //ILogger<TestController> Logger { get; set; }
        //INotificationService NotifyService { get; set; }

        //public TestController(Services.IDatabaseService dbService, IConfiguration configuration, IS3Service s3Client, INotificationService iNotifyService,
        //    ILogger<TestController> logger)
        //{
        //    DbService = dbService;
        //    NotifyService = iNotifyService;
        //    S3Client = s3Client;
        //    Logger = logger;
        //}
        //[AllowAnonymous]
        //[Route("Test")]
        //[HttpGet]
        //public async Task<IActionResult> PostTestNotification()
        //{
        //    new EmailService().SendEmail("support@gymnect.de", "testsubject", "textbody");
        //    return Ok();
        //}
        //[AllowAnonymous]
        //[Route("Test2")]
        //[HttpGet]
        //public async Task<IActionResult> TestPOstCOmment()
        //{
        //    var comment = new PostComment()
        //    {
        //        IsDeleted = false,
        //        PostId = 510,
        //        LastModified = DateTime.UtcNow,
        //        Text = "test",
        //        TimePosted = DateTime.UtcNow,
        //        UserName = "miron"
        //    };
        //    long? postCommentId = DbService.InsertPostComment(comment);
        //    return Ok();
        //}

    //}
}
