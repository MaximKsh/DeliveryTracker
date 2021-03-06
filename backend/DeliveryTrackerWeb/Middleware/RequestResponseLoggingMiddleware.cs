﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DeliveryTrackerWeb.Middleware
{
    public sealed class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory
                .CreateLogger<RequestResponseLoggingMiddleware>();
        }
    
        public async Task Invoke(HttpContext context)
        {  
            #if RELEASE
            await this.next(context);
            #else
            var uri = context.Request.GetDisplayUrl();

            var nlogger = LogManager.GetLogger("tt");
            nlogger.Log(NLog.LogLevel.Info, $"Request {uri}:{Environment.NewLine} {await FormatRequest(context.Request)}");

            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await this.next(context);

                nlogger.Log(NLog.LogLevel.Info, $"Response {uri}:{Environment.NewLine} {await FormatResponse(context.Response)}");
                await responseBody.CopyToAsync(originalBodyStream);
            }
            #endif
        }
    
        private static async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableRewind();
            var body = request.Body;

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            if (!string.IsNullOrWhiteSpace(bodyAsText))
            {
                bodyAsText = JObject.Parse(bodyAsText).ToString(Formatting.Indented);
            }
            body.Seek(0, SeekOrigin.Begin);
            request.Body = body;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private static async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync(); 
            response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrWhiteSpace(text))
            {
                return JObject.Parse(text).ToString(Formatting.Indented);
            }

            return text;

        }
    }
}