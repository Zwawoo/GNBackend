using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Model.WorkoutModels;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WorkoutController : Controller
    {
        IDatabaseService DbService;
        S3Service S3Client { get; set; }
        ILogger<WorkoutController> Logger { get; set; }
        public WorkoutController(Services.IDatabaseService dbService, IAmazonS3 s3Client, IConfiguration configuration,
            ILogger<WorkoutController> logger)
        {
            DbService = dbService;
            S3Client = new S3Service(configuration, s3Client);
            Logger = logger;
        }

        [Route("WorkoutPlan/BiSync")]
        [Route("WorkoutPlans/BiSync")]
        [HttpPost]
        public async Task<IActionResult> WorkoutPlansBiSync()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            WorkoutPlanSyncData clientWorkoutPlanData = null;
            DateTime? lastSync = null;
            SyncRequest<WorkoutPlanSyncData> workoutPlanSyncReq = null;
            try
            {
                workoutPlanSyncReq = await ApiPayloadClass<SyncRequest<WorkoutPlanSyncData>>.GetRequestValueAsync(S3Client, Request.Body);
                clientWorkoutPlanData = workoutPlanSyncReq?.ObjectChangedOnClient;
                lastSync = workoutPlanSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            if (clientWorkoutPlanData != null)
            {
                foreach (WorkoutPlan wp in clientWorkoutPlanData.WorkoutPlans)
                {
                    try
                    {
                        if (wp.UserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        DbService.InsertOrUpdateWorkoutPlanIfNewer(wp);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }
                }
                foreach (Exercise ex in clientWorkoutPlanData.Exercises)
                {
                    try
                    {
                        if (ex.UserName.ToLower() != authenticatedUserName.ToLower())
                            continue;
                        if (ex.IsCustom == false)
                            continue;
                        DbService.InsertOrUpdateExerciseIfNewer(ex);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }
                }
                foreach (WorkoutPlanExercise wpEx in clientWorkoutPlanData.WorkoutPlanExercises)
                {
                    try
                    {

                        DbService.InsertOrUpdateWorkoutPlanExerciseIfNewer(authenticatedUserName, wpEx);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }
                }
            }


            WorkoutPlanSyncData serverWorkoutPlans = new WorkoutPlanSyncData();
            DateTime sinceDate = DateTime.MinValue;
            if (lastSync != null)
            {
                sinceDate = (DateTime)lastSync;
            }
            serverWorkoutPlans.Equipment = DbService.GetEquipmentSinceDate(sinceDate).ToList();
            serverWorkoutPlans.Muscles = DbService.GetMusclesSinceDate(sinceDate).ToList();
            serverWorkoutPlans.WorkoutPlans = DbService.GetAllWorkoutPlansSinceDate(authenticatedUserName, sinceDate).ToList();
            serverWorkoutPlans.Exercises = DbService.GetAllExercisesSinceDate(authenticatedUserName, sinceDate).ToList();
            serverWorkoutPlans.WorkoutPlanExercises = DbService.GetAllWorkoutPlanExercisesSinceDate(authenticatedUserName, sinceDate).ToList();


            return Ok(await ApiPayloadClass<WorkoutPlanSyncData>.CreateApiResponseAsync(S3Client, serverWorkoutPlans));
        }




        [HttpPost]
        public async Task<IActionResult> UploadWorkouts()
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            WorkoutSyncData clientWorkoutData = null;
            DateTime? lastSync = null;
            SyncRequest<WorkoutSyncData> workoutSyncReq = null;
            try
            {
                workoutSyncReq = await ApiPayloadClass<SyncRequest<WorkoutSyncData>>.GetRequestValueAsync(S3Client, Request.Body);
                clientWorkoutData = workoutSyncReq?.ObjectChangedOnClient;
                lastSync = workoutSyncReq?.LastSyncTime;
            }
            catch (Exception ex)
            {
                Logger.LogException(authenticatedUserName, ex, Request);
                return BadRequest();
            }

            if (clientWorkoutData != null)
            {
                foreach (Workout w in clientWorkoutData.Workouts)
                {
                    try
                    {
                        var workoutExercises = clientWorkoutData.WorkoutExercises.Where(x => x.WorkoutId == w.WorkoutId).ToList();
                        var workoutSets = clientWorkoutData.WorkoutSets.Where(x => x.WorkoutId == w.WorkoutId).ToList();

                        w.SerializedWorkoutExercises = Newtonsoft.Json.JsonConvert.SerializeObject(workoutExercises);
                        w.SerializedWorkoutSets = Newtonsoft.Json.JsonConvert.SerializeObject(workoutSets);

                        //Get and Save the newest Date, on which a part of the workout Changed
                        DateTime newestChangeDate = workoutExercises.Select(x => x.LastModified).Concat(workoutSets.Select(x => x.LastModified)).Append(w.LastModified).Max();
                        w.NewestChangedDate = newestChangeDate;

                        DbService.InsertOrUpdateWorkoutIfNewer(authenticatedUserName, w);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogException(authenticatedUserName, ex2, Request);
                    }
                }

            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> DownloadWorkouts([FromQuery] string lastSyncString)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            DateTime lastSyncTime = DateTime.ParseExact(lastSyncString, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

            WorkoutSyncData serverWorkoutData = new WorkoutSyncData();
            serverWorkoutData.Workouts = new List<Workout>();
            serverWorkoutData.WorkoutExercises = new List<WorkoutExercise>();
            serverWorkoutData.WorkoutSets = new List<WorkoutSet>();

            List<Workout> workouts = DbService.GetAllWorkoutsSinceDate(authenticatedUserName, lastSyncTime).ToList();
            foreach (Workout w in workouts)
            {
                try
                {
                    List<WorkoutExercise> workoutExercises = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorkoutExercise>>(w.SerializedWorkoutExercises);
                    List<WorkoutSet> workoutSets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorkoutSet>>(w.SerializedWorkoutSets);
                    w.SerializedWorkoutExercises = null;
                    w.SerializedWorkoutSets = null;
                    serverWorkoutData.Workouts.Add(w);
                    serverWorkoutData.WorkoutExercises.AddRange(workoutExercises);
                    serverWorkoutData.WorkoutSets.AddRange(workoutSets);

                }
                catch (Exception ex)
                {
                    Logger.LogException(authenticatedUserName, ex, Request);
                }
            }

            return Ok(await ApiPayloadClass<WorkoutSyncData>.CreateApiResponseAsync(S3Client, serverWorkoutData));
        }


        [Route("WorkoutPlans")]
        [HttpGet]
        public async Task<IActionResult> GetPublicWorkoutPlans([FromQuery] string userName)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            User user = DbService.GetUser(userName, false);
            if (user == null)
                return BadRequest();
            if (user.IsPrivate)
            {
                if (!DbService.IsUser1FollowingUser2(authenticatedUserName, userName))
                {
                    return Unauthorized();
                }
            }

            WorkoutPlanSyncData userWorkoutPlanData = new WorkoutPlanSyncData();
            if (!String.IsNullOrWhiteSpace(userName))
                userWorkoutPlanData = DbService.GetPublicWorkoutPlans(userName);

            return Ok(await ApiPayloadClass<WorkoutPlanSyncData>.CreateApiResponseAsync(S3Client, userWorkoutPlanData));
        }

        [Route("WorkoutPlans/Copy")]
        [HttpPost]
        public async Task<IActionResult> CopyWorkoutPlan([FromQuery] string workoutPlanIdString)
        {
            string authenticatedUserName = Request.HttpContext.Items[Constants.AuthenticatedUserNameItem].ToString();

            Guid workoutPlanId = Guid.Parse(workoutPlanIdString);

            DbService.CopyWorkoutPlan(workoutPlanId, authenticatedUserName);

            return Ok();
        }

    }
}
