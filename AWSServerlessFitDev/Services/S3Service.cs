using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Amazon.S3.Model;
using System.IO;
using AWSServerlessFitDev.Util;
using Amazon.CloudFront;

namespace AWSServerlessFitDev.Services
{
    public class S3Service : IS3Service
    {
        IAmazonS3 S3Client { get; set; }
        ILogger<S3Service> Logger { get; set; }
        public string BucketName { get; set; }
        public string GymnectS3DataFolder { get; set; }

        public S3Service(IConfiguration configuration, IAmazonS3 s3Client, ILogger<S3Service> logger)
        {
            this.S3Client = s3Client;
            this.BucketName = Constants.GymnectS3BucketName;
            this.GymnectS3DataFolder = Constants.GymnectFolder;
            Logger = logger;
            if (string.IsNullOrEmpty(this.BucketName))
            {
                throw new Exception("Missing configuration for S3 bucket. The AppS3Bucket configuration must be set to a S3 bucket.");
            }

        }

        public async Task<string> GetObject(string folder, string key)
        {
            string responseBody = "";
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = String.IsNullOrEmpty(folder) ? key : (folder + "/" + key)
            };
            using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string contentType = response.Headers["Content-Type"];

                responseBody = reader.ReadToEnd(); // Now you process the response body.
                return responseBody;
            }
        }

        public async Task<string> GetObject(string key)
        {
            string responseBody = "";
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = key
            };
            using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                string contentType = response.Headers["Content-Type"];

                responseBody = await reader.ReadToEndAsync(); // Now you process the response body.
                return responseBody;
            }
        }

        public async Task<byte[]> GetObjectAsByteArray(string folder, string key)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = String.IsNullOrEmpty(folder) ? key : (folder + "/" + key)
            };
            using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
            using (MemoryStream ms = new MemoryStream())
            {
                response.ResponseStream.CopyTo(ms);

                string contentType = response.Headers["Content-Type"];

                return ms.ToArray();
            }
        }


        //Returns the created FileName with path
        public async Task<string> PutObjectAsync(string folder, string key, MemoryStream stream)
        {
            //try
            //{
            string createdFileNameWithPath = String.IsNullOrEmpty(folder) ? key : (folder + "/" + key);
            stream.Position = 0;
            var putRequest1 = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = createdFileNameWithPath,
                InputStream = stream
            };

            PutObjectResponse response1 = await S3Client.PutObjectAsync(putRequest1);
            return createdFileNameWithPath;
            //}
            //catch (AmazonS3Exception e)
            //{
            //    Console.WriteLine(
            //            "Error encountered ***. Message:'{0}' when writing an object"
            //            , e.Message);
            //    return null;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(
            //        "Unknown encountered on server. Message:'{0}' when writing an object"
            //        , e.Message);
            //    return null;
            //}
        }
        //Returns the created FileName with ne path
        public async Task<string> PutObjectAsync(string folder, string key, string content)
        {
            string createdFileNameWithPath = String.IsNullOrEmpty(folder) ? key : (folder + "/" + key);
            var putRequest1 = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = createdFileNameWithPath,
                ContentBody = content
            };

            PutObjectResponse response1 = await S3Client.PutObjectAsync(putRequest1);
            return createdFileNameWithPath;

        }


        public async Task Delete(string folder, string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = String.IsNullOrEmpty(folder) ? key : (folder + "/" + key)
            };

            try
            {
                var response = await this.S3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception e)
            {
                Logger.LogError("Error deleting object={objectpath} . Exception={exception}", deleteRequest.Key, e.ToString());
            }
        }
        public async Task Delete(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = key
            };

            try
            {
                var response = await this.S3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (Exception e)
            {
                Logger.LogError("Error deleting object={objectpath} . Exception={exception}", key, e.ToString());
            }
        }

        
        //Generate presigned url through S3
        public string GeneratePreSignedURL(string objectKey, HttpVerb httpVerb, int expiresInMinutes)
        {
            if (String.IsNullOrEmpty(objectKey))
            {
                return null;
            }
            string urlString = "";

            if(httpVerb == HttpVerb.GET)
            {
                urlString = GenerateCloudFrontSignedURL(objectKey, expiresInMinutes);
            }
            else
            {
                try
                {
                    GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                    {
                        BucketName = BucketName,
                        Key = objectKey,
                        Verb = httpVerb,
                        Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes)

                    };
                    urlString = S3Client.GetPreSignedURL(request);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error generating presigned URL. Key={key} \n" +
                                    "Exception={exception}", e.ToString());
                }
            }
            
            return urlString;
        }

        public string GenerateCloudFrontSignedURL(string objectKey, int expiresInMinutes)
        {
            if (String.IsNullOrEmpty(objectKey))
            {
                return null;
            }
            string urlString = "";
            try
            {
                using(TextReader reader = new StringReader(Constants.SIGNED_URL_PRIVATE_KEY))
                {
                    urlString = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
                                             AmazonCloudFrontUrlSigner.Protocol.https,
                                             Constants.CLOUDFRONT_DOMAIN,
                                             reader,
                                             objectKey,
                                             Constants.SIGNED_URL_PUBLIC_KEY_ID,
                                             DateTime.UtcNow.AddMinutes(expiresInMinutes));
                }  
            }
            catch (Exception e)
            {
                Logger.LogError("Error generating CloudFront signed URL. Key={key} \n" +
                                "Exception={exception}", e.ToString());
            }
            return urlString;
        }



    }
}
