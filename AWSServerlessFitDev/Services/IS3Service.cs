using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Services
{
    public interface IS3Service
    {
        string BucketName { get; set; }
        string GymnectS3DataFolder { get; set; }
        Task<string> GetObject(string folder, string key);
        Task<string> GetObject(string key);
        Task<byte[]> GetObjectAsByteArray(string folder, string key);
        Task<string> PutObjectAsync(string folder, string key, MemoryStream stream);
        Task<string> PutObjectAsync(string folder, string key, string content);
        Task Delete(string folder, string key);
        Task Delete(string key);
        string GeneratePreSignedURL(string objectKey, HttpVerb httpVerb, int expiresInMinutes);
    }
}
