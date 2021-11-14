using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public class SNSService
    {
        string FCMPlatformApplicationArn { get; set; }
        string IOSPlatformApplicationArn { get; set; }
        AmazonSimpleNotificationServiceClient SnsClient { get; set; }

        public SNSService(IConfiguration configuration)
        {
            FCMPlatformApplicationArn = configuration["FCMPlatformApplicationArn"];
            IOSPlatformApplicationArn = configuration["IOSPlatformApplicationArn"];
            SnsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.EUCentral1);

        }

        public async Task<string> RegisterFCMEndpoint(string deviceToken)
        {
            var response = await SnsClient.CreatePlatformEndpointAsync(new CreatePlatformEndpointRequest
            {
                Token = deviceToken,
                PlatformApplicationArn = FCMPlatformApplicationArn
            });
            string endpointArn = response?.EndpointArn;
            return endpointArn;
        }

        public async Task PublishMessage(string deviceArn, string jsonmessage)
        {
            var pr = new PublishRequest()
            {
                TargetArn = deviceArn,
                MessageStructure = "json",
                Message = jsonmessage
            };
            var x = await SnsClient.PublishAsync(pr);
        }

        public async Task DeleteAndroidEndpoint(string endpointArn)
        {
            var delReq = new DeleteEndpointRequest()
            {
                EndpointArn = endpointArn
            };
            await SnsClient.DeleteEndpointAsync(delReq);
        }

    }
}
