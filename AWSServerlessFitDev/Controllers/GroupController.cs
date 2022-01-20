using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GroupController : Controller
    {
        IDatabaseService DbService;
        S3Service S3Client { get; set; }
        ILogger<GroupController> Logger { get; set; }
        public GroupController(Services.IDatabaseService dbService, IAmazonS3 s3Client, IConfiguration configuration, ILogger<GroupController> logger)
        {
            DbService = dbService;
            S3Client = new S3Service(configuration, s3Client);
            Logger = logger;
        }

        [Route("Gym")]
        [HttpGet]
        public async Task <IActionResult> GetGyms([FromQuery] string CityName, [FromQuery] string GymName, [FromQuery] int? MaxCount)
        {
            try
            {
                int limit;
                if (String.IsNullOrEmpty(CityName))
                    CityName = null;
                if (String.IsNullOrEmpty(GymName))
                    GymName = null;
                if (MaxCount == null)
                    limit = 20;
                else
                    limit = MaxCount.Value;

                List<Gym> result = DbService.GetGyms(CityName, GymName, limit)?.ToList();

                //return Ok(new { Value = result });
                return Ok(ApiPayloadClass<List<Gym>>.CreateSmallApiResponse(result));
            }
            catch(Exception ex)
            {
                Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex, Request);
                return BadRequest();
            }
        }

        [Route("GymSearch")]
        [HttpGet]
        public async Task<IActionResult> SearchGyms([FromQuery] int lastGroupId, [FromQuery] string searchText, [FromQuery] double? leastRelevance, [FromQuery] int limit)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            try
            {
                if (String.IsNullOrWhiteSpace(searchText))
                    searchText = null;
                if (limit > 20)
                    limit = 20;

                List<Gym> gyms = new List<Gym>();

                gyms = DbService.SearchGyms(lastGroupId, searchText, leastRelevance, limit)?.ToList();

                return Ok(ApiPayloadClass<List<Gym>>.CreateSmallApiResponse(gyms));
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }
            
        }


        [Route("{groupId:int}/Users")]
        [HttpGet]
        public async Task<IActionResult> GetGroupMembers([FromRoute] int groupId, [FromQuery] string searchText, [FromQuery] string offsetOldestUserName, [FromQuery] int limit)
        {
            string authenticatedUserName = String.Empty;
            try
            {
                authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

                if (limit < 0)
                    return BadRequest();

                if (String.IsNullOrWhiteSpace(searchText))
                    searchText = null;
                if (String.IsNullOrWhiteSpace(offsetOldestUserName))
                    offsetOldestUserName = null;

                /*
                 * TODO : If Group is private or secret
                 */

                List<User> users = DbService.GetGroupMembers(groupId, searchText, offsetOldestUserName, limit)?.ToList();
                List<BlockedUser> usersThatBlockedCaller = DbService.GetBlockingUsersFor(authenticatedUserName).ToList();
                List<User> resultUserList = new List<User>();
                if (users != null)
                {
                    foreach (User user in users)
                    {
                        if (usersThatBlockedCaller.Any(u => u.UserName.ToLower() == user.UserName.ToLower()))
                            continue;
                        user.ProfilePictureUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureUrl, HttpVerb.GET, 60 * 24);
                        user.ProfilePictureHighResUrl = S3Client.GeneratePreSignedURL(user.ProfilePictureHighResUrl, HttpVerb.GET, 60 * 24);
                        resultUserList.Add(user);
                    }
                }
                //return Ok(new { Value = users });
                return Ok(ApiPayloadClass<List<User>>.CreateSmallApiResponse(resultUserList));
            }
            catch(Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }
        }

        [Route("{groupId:int}/MemberCount")]
        public async Task<IActionResult> GetMemberCount([FromRoute] int groupId)
        {
            try
            {
                if (groupId < 0)
                    return BadRequest();

                int memberCount = DbService.GetGroupMemberCount(groupId);
                return Ok(ApiPayloadClass<int>.CreateSmallApiResponse(memberCount));
            }
            catch(Exception ex)
            {
                Logger.LogException(Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString(), ex, Request);
                return BadRequest();
            }
        }


    }
}
