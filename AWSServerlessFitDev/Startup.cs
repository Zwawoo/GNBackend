using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using AWSServerlessFitDev.Jobs;
using AWSServerlessFitDev.Services;
using AWSServerlessFitDev.Util;
using AWSServerlessFitDev.Util.Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Quartz;

namespace AWSServerlessFitDev
{
    public class Startup
    {


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddControllers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                        {
                            RequestCachePolicy policy = new RequestCachePolicy(RequestCacheLevel.Default);
                            var webClient = new System.Net.WebClient();
                            webClient.CachePolicy = policy;
                            // get JsonWebKeySet from AWS
                            var json = webClient.DownloadString(parameters.ValidIssuer + "/.well-known/jwks.json");
                            // serialize the result
                            //var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
                            var keys = new JsonWebKeySet(json).Keys;
                            // cast the result to be the type expected by IssuerSigningKeyResolver
                            return (IEnumerable<SecurityKey>)keys;
                        },

                        ValidIssuer = $"https://cognito-idp.{Constants.Region}.amazonaws.com/{Constants.UserPoolId}",
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        //ValidAudience = Constants.AppClientId,
                        //ValidateAudience = true
                        ValidateAudience = false //Need to set this to false because the access token that we use in authorize header does not include audience claim
                    };
                });

            services.AddAuthorization(options =>
            {
                // Configure the default policy
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(Constants.UserNameClaim)
                    .Build();

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(Constants.UserNameClaim)
                    .Build();

                options.AddPolicy("AdminPolicy", policy => policy.RequireAuthenticatedUser()
                    .RequireClaim(Constants.UserNameClaim).RequireClaim(Constants.CognitoGroupsClaim, Constants.AdminGroup));
            });


            // Add S3 to the ASP.NET Core dependency injection framework.

            //services.AddAWSService<Amazon.S3.IAmazonS3>();
            //Credentials From BucketUser
            //services.AddSingleton<Amazon.S3.IAmazonS3>(s => new AmazonS3Client(Constants.BucketUserAccessKey, Constants.BucketUserSecretKey, new AmazonS3Config() { SignatureVersion = "V4", RegionEndpoint = RegionEndpoint.EUCentral1 }));
            
            services.AddSingleton<Amazon.S3.IAmazonS3>(s => new AmazonS3Client(new AmazonS3Config() { SignatureVersion = "V4", RegionEndpoint = RegionEndpoint.EUCentral1 }));
            services.AddSingleton<IS3Service, S3Service>();

            //string connString = Constants.ConnectionString;
            //IDatabaseService dbService = new MySQLService(connString);
            services.AddSingleton<IDatabaseService>(s => 
                        new MySQLService(s.GetRequiredService<ILogger<MySQLService>>(), Constants.ConnectionString));

            //var sp = services.BuildServiceProvider();
            //services.AddSingleton<INotificationService>(s => new NotificationService(Configuration, dbService));
            services.AddSingleton<INotificationService, NotificationService>();

            services.AddSingleton<IEmailService, EmailService>();

            services.AddTransient<AWSServerlessFitDev.Util.IFireForgetRepositoryHandler, AWSServerlessFitDev.Util.FireForgetRepositoryHandler>();
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddLogging(config =>
            {
                config.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
                config.SetMinimumLevel(LogLevel.Debug);
                //config.AddConsole(c => { c.TimestampFormat = "[HH:mm:ss]"; });
            });

            services.AddHealthChecks();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Create a "key" for the job
                var jobKey = new JobKey("ClearDeletedUsersJob");

                // Register the job with the DI container
                q.AddJob<ClearDeletedUsersJob>(opts => opts.WithIdentity(jobKey));

                // Create a trigger for the job
                q.AddTrigger(opts => opts
                    .ForJob(jobKey) // link to the HelloWorldJob
                    .WithIdentity("ClearDeletedUsersJob-trigger") // give the trigger a unique name
                    .WithCronSchedule("0 0 1 * * ?")); //"0 0 2 * * ?" = jeden Tag um 2 uhr
            });

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            string creds;
#if Prod
            creds = Utils.ReadResource("AWSServerlessFitDev.gymnectprod-firebase-adminsdk-xqgsq-68b24b80f3.json");
#else
            creds = Utils.ReadResource("AWSServerlessFitDev.fitappdev-254410-firebase-adminsdk-j2kzc-cdc4e8f053.json");
#endif

            FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions() { Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(creds) });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {

                app.UseHsts();
                app.UseExceptionHandler(a => a.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature.Error;

                    if (context.RequestAborted.IsCancellationRequested)
                    {
                        logger.LogWarning("RequestAborted. " + exception.Message);
                        return;
                    }

                    string authenticatedUserName = context.Request?.HttpContext?.Items[Constants.AuthenticatedUserNameItem]?.ToString();
                    logger.LogException(authenticatedUserName, exception, context.Request);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Fehler");
                }));
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); //new
            app.UseAuthorization();

            app.UseMiddleware<MyMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapHealthChecks("/").WithMetadata(new AllowAnonymousAttribute());
                endpoints.MapGet("/auth", async context =>
                {
                    await context.Response.WriteAsync("Authorized");
                });

                //Test DB and Internet Access Access 
                endpoints.MapGet("/serviceInfo", async context =>
                {
                    string response;
                    var db = app.ApplicationServices.GetService<IDatabaseService>();
                    var muscles = db.GetMusclesSinceDate(DateTime.MinValue.AddDays(1));
                    response = muscles.Count().ToString();
                    response += "\n CheckInternet: " + Utils.CheckInternet();
                    await context.Response.WriteAsync(response);
                }).WithMetadata(new AllowAnonymousAttribute());
            });
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
