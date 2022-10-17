using AWSServerlessFitDev.Model;
using AWSServerlessFitDev.Services;
using Microsoft.AspNetCore.Http;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AWSServerlessFitDev
{
    public class Utils
    {
        public static async Task<int> CallMySQLSTPReturnAffectedRows(string connectionString, string stpName, List<MySqlParameter> _params)
        {
            if (_params == null)
                _params = new List<MySqlParameter>();

            var resultParam = new MySqlParameter("AffectedRows_", MySqlDbType.Int32);
            resultParam.Direction = ParameterDirection.Output;
            _params.Add(resultParam);

            await CallMySQLSTP(connectionString, stpName, _params);

            if (resultParam.Value != null)
            {
                try
                {
                    return Convert.ToInt32(resultParam.Value);
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
            else
                return 0;
        }

        public static async Task CallMySQLSTP(string connectionString, string stpName, List<MySqlParameter> _params)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(stpName, conn) { CommandType = CommandType.StoredProcedure })
                {
                    await conn.OpenAsync();
                    if (_params != null)
                    {
                        command.Parameters.AddRange(_params.ToArray());
                    }
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task<DataTable> callMySQLSTPReturnDt(string connectionString, string stpName, List<MySqlParameter> _params)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(stpName, conn) { CommandType = CommandType.StoredProcedure })
                {

                    await conn.OpenAsync();

                    if (_params != null)
                    {
                        command.Parameters.AddRange(_params.ToArray());
                    }
                    DataTable dt = new DataTable();
                    dt.Load(await command.ExecuteReaderAsync());
                    return dt;
                }

            }
        }

        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                    SixLabors.ImageSharp.Image.Load(ms);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidVideo(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                { //mp4
                  // Video v
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"


            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static bool CheckInternet()
        {
            try
            {
                int timeoutMs = 10000;
                string url = "http://google.com";

                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
                return false;
            }
        }



        //public static async Task<AuthenticationResult> CheckAuthentication(HttpRequest httpReq)
        //{
        //    AuthenticationResult result = new AuthenticationResult();
        //    Microsoft.Extensions.Primitives.StringValues headerValues;
        //    string accessToken;
        //    string userName = "";

        //    if (httpReq.Headers.TryGetValue("Authorization", out headerValues))//Authentication
        //    {
        //        accessToken = headerValues.FirstOrDefault();
        //        if(accessToken.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            accessToken = accessToken.Split(" ")[1];
        //        }
        //        userName = await CognitoService.GetUserNameFromAccessToken(accessToken);
        //        if (String.IsNullOrWhiteSpace(userName))
        //        {
        //            result.AuthenticatedUser = null;
        //            result.IsAuthenticated = false;
        //        }
        //        else
        //        {
        //            result.AuthenticatedUser = userName;
        //            result.IsAuthenticated = true;
        //        }
        //    }
        //    else
        //    {
        //        result.AuthenticatedUser = null;
        //        result.IsAuthenticated = false;
        //    }
        //    return result;

        //}


    }

    public static class CustomExtensions
    {
        public static DateTime TrimMilliseconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }

        public static DateTime? TrimMilliseconds(this DateTime? dt)
        {
            if (dt.HasValue)
            {
                return dt.Value.TrimMilliseconds();
            }
            return null;
        }


        public static string GetStringOrNull(this IDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static string GetStringOrNull(this IDataReader reader, string columnName)
        {
            return reader.GetStringOrNull(reader.GetOrdinal(columnName));
        }

        public static DateTime? GetDateTimeOrNull(this IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        public static int? GetInt32OrNull(this IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : (int?)reader.GetInt32(reader.GetOrdinal(columnName));
        }

        public static long? GetInt64OrNull(this IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : (long?)reader.GetInt64(reader.GetOrdinal(columnName));
        }
    }

}
