using System;
using System.Net;
using System.Threading.Tasks;
using DeliveryTracker.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryTrackerWeb.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ExceptionHandlingMiddleware(RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.logger = loggerFactory
                .CreateLogger<RequestResponseLoggingMiddleware>();
        }

        public async Task Invoke(
            HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception e)
            {
                this.logger.LogCritical($"Unhandled exception {e.Message}{Environment.NewLine}" +
                                        e.StackTrace);  
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var responseString = JsonConvert
                    .SerializeObject(ErrorFactory.ServerError());
                await context.Response.WriteAsync(responseString).ConfigureAwait(false);
            }
        }
    }
}