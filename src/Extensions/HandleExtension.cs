using Distributed.Logging.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Distributed.Logging.Extensions
{
    public static class HandleExtension
    {
        public static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            var errorResponse = new ErrorResponseModel
            {
                Status = context.Response.StatusCode,
                TraceId = context.TraceIdentifier // IS REQUEST ID ALSO
            };
            switch (exception.GetType().ToString())
            {
                case "System.IndexOutOfRangeException":
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.NotFound);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                case "Microsoft.EntityFrameworkCore.DbUpdateException":
                    response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Status = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Title = "Database Issues";
                    var dbUpEx = (Microsoft.EntityFrameworkCore.DbUpdateException)exception;
                    if (dbUpEx.InnerException is not null)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                        errorResponse.Status = (int)HttpStatusCode.NotAcceptable;
                        errorResponse.Type = response.ContentType;
                        if (dbUpEx.InnerException.Message.ToLower().Contains("unique index"))
                        {
                            errorResponse.Title = "Unique Constraint";
                            errorResponse.Message = "Cannot insert duplicate key row";
                        }
                        else if (dbUpEx.InnerException.Message.ToLower().Contains("check Constraint"))
                        {
                            errorResponse.Title = "Check Constraint";
                            errorResponse.Message = "Constraint chech violation.";
                        }
                        else if (dbUpEx.InnerException.Message.ToLower().Contains("foreign key constraint"))
                        {
                            errorResponse.Title = "FOREIGN KEY constraint";
                            errorResponse.Message = "The INSERT statement conflicted with the FOREIGN KEY constraint";
                        }
                        else
                        {
                            errorResponse.Message = dbUpEx.Message;
                        }
                    }
                    break;
                case "System.UnauthorizedAccessException":
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.Unauthorized);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                case "System.ApplicationException":
                    exception.GetType();
                    if (exception.Message.Contains("Invalid token"))
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        errorResponse.Type = response.ContentType;
                        errorResponse.Title = nameof(HttpStatusCode.Forbidden);
                        errorResponse.Status = response.StatusCode;
                        errorResponse.Message = exception.Message;
                        break;
                    }
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                case "System.Collections.Generic.KeyNotFoundException":
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.NotFound);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                case "System.IO.FileNotFoundException":
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.NotFound);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                case "System.DllNotFoundException":
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.NotFound);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = exception.Message;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.InternalServerError);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = "Internal Server errors. Check Logs!";
                    break;
            }
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
