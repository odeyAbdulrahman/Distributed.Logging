using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Distributed.Logging.Extensions;

namespace Distributed.Logging.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly ILogger<ExceptionHandlingMiddleware> Logger;
        
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            Next = next;
            Logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await Next(httpContext);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Critical Exception");
                await HandleExtension.HandleExceptionAsync(httpContext, ex);
            }
        }
    }
}
