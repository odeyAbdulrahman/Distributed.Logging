using Distributed.Logging.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Distributed.Logging.Extensions
{
    public static class HandleExtension
    {
        public static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            StringBuilder ValidationBuilder = new();
            string traceIdentifier = context.TraceIdentifier; // IS REQUEST ID ALSO
            context.Response.ContentType = "application/json";
            var response = context.Response;
            var errorResponse = new ErrorResponseMode { Status = context.Response.StatusCode };
            errorResponse.TraceId = traceIdentifier;
            switch (exception)
            {
                case DbEntityValidationException ex:
                    ex.EntityValidationErrors.Where(x => !x.IsValid).Select((x) =>
                    {
                        return ValidationBuilder.Append($"Entity of type { x.Entry.GetType().Name} is state {x.Entry.State} has the following Validation errors: { x.ValidationErrors.Select((v) => { return v.PropertyName + ":U+0020" + v.ErrorMessage; }).ToList().Aggregate((xx, yy) => $"[{xx} - {yy} ]")}");
                    }).ToList();
                    response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Status = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Message = ValidationBuilder.ToString();
                    break;
                case DbUpdateException ex:
                    if (ex.InnerException is not null)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                        errorResponse.Status = (int)HttpStatusCode.NotAcceptable;
                        errorResponse.Type = response.ContentType;
                        if (ex.InnerException.Message.Contains("unique index"))
                        {
                            errorResponse.Title = "Unique Constraint";
                            errorResponse.Message = $"Cannot insert duplicate key row";
                        }
                        else if (ex.InnerException.Message.Contains("Check Constraint"))
                        {
                            errorResponse.Title = "Check Constraint";
                            errorResponse.Message = $"Constraint chech violation.";
                        }
                        else
                        {
                            errorResponse.Title = "Database Issues";
                            errorResponse.Message = ex.Message;
                        }
                        break;
                    }
                    response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Status = (int)HttpStatusCode.NotAcceptable;
                    errorResponse.Message = ex.Message;
                    break;
                case UnauthorizedAccessException ex:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.Unauthorized);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = ex.Message;
                    break;
                case ApplicationException ex:
                    if (ex.Message.Contains("Invalid token"))
                    {
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                        errorResponse.Type = response.ContentType;
                        errorResponse.Title = nameof(HttpStatusCode.Forbidden);
                        errorResponse.Status = response.StatusCode;
                        errorResponse.Message = ex.Message;
                        break;
                    }
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = ex.Message;
                    break;
                case KeyNotFoundException ex:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Type = response.ContentType;
                    errorResponse.Title = nameof(HttpStatusCode.NotFound);
                    errorResponse.Status = response.StatusCode;
                    errorResponse.Message = ex.Message;
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
