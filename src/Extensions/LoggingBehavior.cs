using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Logging.Extensions
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly IHttpContextAccessor? ContextAccessor;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IHttpContextAccessor? contextAccessor)
        {
            _logger = logger;
            ContextAccessor = contextAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            #region User Info
            // Log the incoming request
            if (ContextAccessor != null && ContextAccessor.HttpContext != null)
            {
                var userNameClaim = ContextAccessor?.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "name");
                var userIdClaim = ContextAccessor?.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                Log.ForContext("UserNameField", userNameClaim?.Value)
                      .ForContext("UserIdField", userIdClaim?.Value)
                      .ForContext("IsAuthenticatedField", ContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated)
                      .ForContext("AuthenticationTypeField", ContextAccessor?.HttpContext?.User?.Identity?.AuthenticationType)
                      .ForContext("RemoteIpAddressField", ContextAccessor?.HttpContext?.Connection.RemoteIpAddress)
                      .ForContext("LocalIpAddressField", ContextAccessor?.HttpContext?.Connection.LocalIpAddress)
                      .Information("Request User Fields");
            }
            #endregion

            // Serialize the request body to JSON
            var requestBody = JsonConvert.SerializeObject(request);
            _logger.LogInformation($"Request {typeof(TRequest).Name} Body: {requestBody}");

            // Call the next handler in the pipeline
            var response = await next();

            // Serialize the response body to JSON
            var responseBody = JsonConvert.SerializeObject(response);
            _logger.LogInformation($"Response {typeof(TResponse).Name} Body: {responseBody}");

            return response;
        }
    }
}
