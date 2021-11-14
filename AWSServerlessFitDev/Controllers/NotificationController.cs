using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        IDatabaseService DbService { get; set; }
        SNSService SnsService { get; set; }
        S3Service S3Client { get; set; }

        public NotificationController(Services.IDatabaseService dbService, IConfiguration configuration, IAmazonS3 s3Client)
        {
            DbService = dbService;
            SnsService = new SNSService(configuration);
            S3Client = new S3Service(configuration, s3Client);
        }

        [Route("Register/android")]
        [HttpPut]
        public async Task<IActionResult> RegisterAndroidDeviceForNotification()
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                string deviceToken = await ApiPayloadClass<string>.GetRequestValueAsync(S3Client, Request.Body);

                if (String.IsNullOrEmpty(deviceToken))
                    return BadRequest();


                //string endpointArn = await SnsService.RegisterFCMEndpoint(deviceToken);
                //if (!String.IsNullOrEmpty(endpointArn))
                //DbService.InsertUserDeviceEndpoint(authenticatedUserName, endpointArn, "android", deviceToken);
                DbService.InsertUserDeviceEndpoint(authenticatedUserName, "", "android", deviceToken);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }

        }
        [Route("Register/iOS")]
        [HttpPut]
        public async Task<IActionResult> RegisteriOSDeviceForNotification()
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                string deviceToken = await ApiPayloadClass<string>.GetRequestValueAsync(S3Client, Request.Body);

                if (String.IsNullOrEmpty(deviceToken))
                    return BadRequest();


                //string endpointArn = await SnsService.RegisterFCMEndpoint(deviceToken);
                //if (!String.IsNullOrEmpty(endpointArn))
                //    DbService.InsertUserDeviceEndpoint(authenticatedUserName, endpointArn, "iOS", deviceToken);
                DbService.InsertUserDeviceEndpoint(authenticatedUserName, "", "iOS", deviceToken);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }


        [Route("UnRegister")]
        [HttpPut]
        public async Task<IActionResult> UnRegisterDeviceForNotification()
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                string deviceToken = await ApiPayloadClass<string>.GetRequestValueAsync(S3Client, Request.Body);

                if (String.IsNullOrEmpty(deviceToken))
                    return BadRequest();

                DbService.DeleteUserDeviceEndpoint(authenticatedUserName, deviceToken);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
        }

        


        [Route("UnRegisterAll")]
        [HttpPut]
        public async Task<IActionResult> UnRegisterAllDevices()
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                string deviceTokenToKeep = await ApiPayloadClass<string>.GetRequestValueAsync(S3Client, Request.Body);

                var userDevices = DbService.GetUserDevices(authenticatedUserName);
                foreach(var device in userDevices)
                {
                    if( String.IsNullOrEmpty(deviceTokenToKeep) || !deviceTokenToKeep.Equals(device.DeviceToken, StringComparison.OrdinalIgnoreCase))
                    {
                        DbService.DeleteUserDeviceEndpoint(authenticatedUserName, device.DeviceToken);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] string lastSyncString)
        {
            try
            {
                string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                DateTime lastSyncTime = DateTime.ParseExact(lastSyncString, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

                List<Notification> notifications = DbService.GetNotifications(authenticatedUserName, lastSyncTime).ToList();

                return Ok(await ApiPayloadClass<List<Notification>>.CreateApiResponseAsync(S3Client, notifications));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }

        }

    }
        
}
