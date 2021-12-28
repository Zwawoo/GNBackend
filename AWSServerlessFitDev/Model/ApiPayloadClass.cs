using Amazon.S3;
using AWSServerlessFitDev.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class ApiPayloadClass<T>
    {
        public T Value { get; set; }
        public bool IsPayloadSizeTooLarge { get; set; }
        public string S3UrlIfSizeTooLarge { get; set; }

        /*
         * Creates an Api Response. If Payload is bigger than 6MB (AWS Lambda limit), it puts the response into a S3 File and returns the url.
         * Otherwise it puts the response into the http payload
         */
        public static async Task<ApiPayloadClass<T>> CreateApiResponseAsync(S3Service s3Client, T responseValue)
        {
            ApiPayloadClass<T> response = new ApiPayloadClass<T>();
            string serializedValue = Newtonsoft.Json.JsonConvert.SerializeObject(responseValue);
            long byteCount = System.Text.Encoding.UTF8.GetByteCount(serializedValue);
            if(byteCount > 6000000)
            {
                response.Value = default(T);
                response.IsPayloadSizeTooLarge = true;

                string uniqueFileName = string.Format(@"{0}_{1}.json", "DownloadReq", Guid.NewGuid());
                string filePath = await s3Client.PutObjectAsync(s3Client.GymnectS3DataFolder + "/Temp", uniqueFileName, serializedValue);
                response.S3UrlIfSizeTooLarge = s3Client.GeneratePreSignedURL(filePath, HttpVerb.GET, 60 * 24 * 6);
            }
            else
            {
                response.IsPayloadSizeTooLarge = false;
                response.S3UrlIfSizeTooLarge = null;
                response.Value = responseValue;
            }

            return response;
        }

        /*
         * Creates a fast api Response for a payload, which you know will be smaller than 6 MB
         * 
         */
        public static ApiPayloadClass<T> CreateSmallApiResponse(T responseValue)
        {
            ApiPayloadClass<T> response = new ApiPayloadClass<T>();
            response.IsPayloadSizeTooLarge = false;
            response.S3UrlIfSizeTooLarge = null;
            response.Value = responseValue;
            return response;
        }


        public static async Task<T> GetRequestValueAsync(S3Service s3Client, Stream bodyStream)
        {
            string body;
            StreamReader sr = new StreamReader(bodyStream);
            body = sr.ReadToEnd();

            ApiPayloadClass<T> payload = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiPayloadClass<T>>(body);
            T value;
            if (payload.IsPayloadSizeTooLarge)
            {
                string file = await s3Client.GetObject(payload.S3UrlIfSizeTooLarge);
                value = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(file);
                await s3Client.Delete(payload.S3UrlIfSizeTooLarge);
            }
            else
            {
                value = payload.Value;
            }
            return value;
        }
    }
}
