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

namespace AWSServerlessFitDev.Services
{
    public class S3Service
    {
        IAmazonS3 S3Client { get; set; }
        ILogger Logger { get; set; }
        string BucketName { get; set; }
        public string GymnectS3DataFolder { get; set; }

        public S3Service(IConfiguration configuration, IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
            this.BucketName = Constants.GymnectS3BucketName;
            this.GymnectS3DataFolder = Constants.GymnectFolder;
            if (string.IsNullOrEmpty(this.BucketName))
            {
                throw new Exception("Missing configuration for S3 bucket. The AppS3Bucket configuration must be set to a S3 bucket.");
            }

        }

        public async Task<string> GetObject(string folder, string key)
        {
            string responseBody = "";
            try
            {
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return null;
        }

        public async Task<string> GetObject(string key)
        {
            string responseBody = "";
            try
            {
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

                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                    return responseBody;
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return null;
        }

        public async Task<byte[]> GetObjectAsByteArray(string folder, string key)
        {
            try
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return null;
        }


        //Returns the created FileName with path
        //returns null if exception occured
        public async Task<string> PutObjectAsync(string folder, string key, MemoryStream stream)
        {
            try
            {
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
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
                return null;
            }
        }
        //Returns the created FileName with ne path
        //returns null if exception occured
        public async Task<string> PutObjectAsync(string folder, string key, string content)
        {
            try
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
                return null;
            }
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }


        public string GeneratePreSignedURL(string objectKey, HttpVerb httpVerb, int expiresInMinutes)
        {
            if (String.IsNullOrEmpty(objectKey))
            {
                return null;
            }
            string urlString = "";
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
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return urlString;
        }

    }  
}
