using Amazon.EC2;
using Amazon.ElasticBeanstalk;
using Amazon.ElasticBeanstalk.Model;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Jobs
{
    [DisallowConcurrentExecution]
    public class ClearDeletedUsersJob : IJob
    {
        private readonly ILogger<ClearDeletedUsersJob> Logger;
        IDatabaseService DbService { get; set; }
        S3Service S3Client { get; set; }
        public ClearDeletedUsersJob(ILogger<ClearDeletedUsersJob> logger, IDatabaseService dbService, IConfiguration configuration, IAmazonS3 s3Client)
        {
            Logger = logger;
            DbService = dbService;
            S3Client = new S3Service(configuration, s3Client);
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                if (await CheckIfCurrentEC2InstanceIsLeader())
                {
                    Logger?.LogInformation("Clearing started.");

                    //get all users with deleted = true and DeletedAt <= currentTime - 2 Wochen
                    List<User> usersToBeCleared = DbService.GetUsersForClearing().ToList();

                    foreach(User u in usersToBeCleared)
                    {
                        try
                        {
                            Logger?.LogInformation("Clearing started for user: " + u.UserName + "SubId: " + u.SubId);


                            DbService.ClearUser(u.SubId);

                            Logger?.LogInformation("Clear User Stored Procedure successfull: " + u.UserName + "SubId: " + u.SubId);

                            //Delete Profile Images from storage
                            if (!String.IsNullOrEmpty(u.ProfilePictureHighResUrl))
                                await S3Client.Delete(null, u.ProfilePictureHighResUrl);
                            if (!String.IsNullOrEmpty(u.ProfilePictureUrl))
                                await S3Client.Delete(null, u.ProfilePictureUrl);

                            //Delete Posts from storage
                            var postsToDelete = DbService.GetAllPostsFromUser(u.SubId).ToList();
                            foreach (Post p in postsToDelete)
                            {
                                await S3Client.Delete(null, p.PostResourceUrl);
                                await S3Client.Delete(null, p.PostResourceThumbnailUrl);
                            }

                            //
                            await CognitoService.AdminDeleteUser(u.UserName);

                            Logger?.LogInformation("Deleted Cognito User for user: " + u.UserName + "SubId: " + u.SubId);

                        }
                        catch(Exception ex0)
                        {
                            Logger?.LogError(ex0.ToString());
                        }
                    }

                }

                
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex.ToString());
            }      
        }

        public async Task<bool> CheckIfCurrentEC2InstanceIsLeader()
        {
            //var credentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile();
            //CredentialProfile profile;
            //credentialsFile.TryGetProfile("GymnectDevVisualStudio", out profile);
            //var credentials = profile.GetAWSCredentials(credentialsFile);

            string ec2InstanceId = Amazon.Util.EC2InstanceMetadata.InstanceId;
            var ebClient = new AmazonElasticBeanstalkClient();
            var ec2Client = new AmazonEC2Client();
            var tagResponse = await ec2Client.DescribeTagsAsync();
            var environmentIdTag = tagResponse.Tags.Where(tag => tag.Key.ToLower() == "elasticbeanstalk:environment-id").FirstOrDefault();
            if (environmentIdTag != null)
            {
                var envDescrResponse = await ebClient.DescribeEnvironmentResourcesAsync(new DescribeEnvironmentResourcesRequest()
                {
                    EnvironmentId = environmentIdTag.Value
                });
                var instanceList = envDescrResponse.EnvironmentResources.Instances;
                instanceList = instanceList?.OrderBy(x => x.Id).ToList();

                if (ec2InstanceId == instanceList?.FirstOrDefault()?.Id)
                {
                    Logger?.LogInformation("Instance Id is leader: " + ec2InstanceId);
                    //Execute this only in one instance (Instance with first Id)

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("elasticbeanstalk:environment-id Tag not found or null");
            }
        }


    }
}
