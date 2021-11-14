using AWSServerlessFitDev.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        //[Route("PostTestNotification")]
        //[HttpPost]
        //public async Task<IActionResult> PostTestNotification()
        //{
        //    var ns = new NotificationService(null, null);
        //    string token = "eWZCoc3SB0DZu02OMMZIaj:APA91bGOVfX1uUi88Ic6UGnKiZ2VtOzYWw5IBPMMJ1OAwp_o3R0JlKNASrZtnJU8PgPH-C45BfBanj6Pe64pFNaFLt6zJrjCW1yF1-ST9xBIMFR2CP-VPNdfQiFUeuo-rsjJ1dtwxJot";
        //    await ns.FCMPublishMessage( token, -1, Model.NotificationType.Follow, "miron", "miron4", null, -1);
        //    return Ok();
        //}
    }
}
