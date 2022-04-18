using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Util.Helper
{
    public static class LoggerHelper
    {
        public static void LogException(this ILogger logger, string authenticatedUserName, Exception ex, HttpRequest request)
        {
            if(request != null)
            {
                logger.LogError("[{timestamp}] Exception for UserName={userName} \n" +
                " Exception={exception} \n \n" +
                "HTTP request information:\n" +
                "\tMethod: {httpContext.Request.Method}\n" +
                "\tPath: {httpContext.Request.Path}\n" +
                "\tQueryString: {httpContext.Request.QueryString}\n" +
                "\tHeaders: {httpContext.Request.Headers}\n" +
                "\tSchema: {httpContext.Request.Scheme}\n" +
                "\tHost: {httpContext.Request.Host}\n" +
                "\tBody: {body}",
                DateTime.UtcNow.ToString("O", CultureInfo.GetCultureInfo("de-DE")),
                authenticatedUserName, ex.ToString(),
                request.Method, request.Path, request.QueryString, FormatHeaders(request.Headers), request.Scheme, request.Host, ReadBodyFromRequest(request));
            }
            else
            {
                logger.LogError("Exception for UserName={userName} \n" +
                " Exception={exception}",
                authenticatedUserName, ex.ToString());
            }
            
        }

        //public static void LogRequest(this ILogger logger, HttpRequest request)
        //{
        //    logger.LogError("HTTP request information:\n" +
        //        "\tMethod: {httpContext.Request.Method}\n" +
        //        "\tPath: {httpContext.Request.Path}\n" +
        //        "\tQueryString: {httpContext.Request.QueryString}\n" +
        //        "\tHeaders: {httpContext.Request.Headers}\n" +
        //        "\tSchema: {httpContext.Request.Scheme}\n" +
        //        "\tHost: {httpContext.Request.Host}\n" +
        //        "\tBody: {body}", 
        //        request.Method, request.Path, request.QueryString, FormatHeaders(request.Headers), request.Scheme, request.Host, ReadBodyFromRequest(request));
        //}

        private static string FormatHeaders(IHeaderDictionary headers) => string.Join(", ", headers.Select(kvp => $"{{{kvp.Key}: {string.Join(", ", kvp.Value)}}}"));

        private static string ReadBodyFromRequest(HttpRequest request)
        {
            // Ensure the request's body can be read multiple times (for the next middlewares in the pipeline).
            request.EnableBuffering();

            using var streamReader = new StreamReader(request.Body, leaveOpen: true);
            var requestBody = streamReader.ReadToEnd();

            // Reset the request's body stream position for next middleware in the pipeline.
            request.Body.Position = 0;
            return requestBody;
        }
    }
}
