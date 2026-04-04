using EventFlow.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EventFlow.API.Models
{
    public class APIError : ProblemDetails
    {
        public const string UnhandledError = "UnhandledError";
        private readonly HttpContext _context;
        private readonly Exception _exception;

        public string Code { get; set; }
        public LogLevel LogLevel { get; set; }

        public string TraceId
        {
            get
            {
                if (Extensions.TryGetValue("TraceId", out var traceid))
                {
                    return (string)traceid;
                }

                return null;
            }
            set => Extensions["TraceId"] = value;
        }


        public APIError(HttpContext context, Exception exception)
        {
            _context = context;
            _exception = exception;

            TraceId = context.TraceIdentifier;
            Instance = context.Request.Path;

            HandleException((dynamic)exception);

        }
        private void HandleException(NotFoundException exception)
        {
            Code = exception.Code;
            Status = (int)HttpStatusCode.NotFound;
            Title = exception.Message;
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/404";
            LogLevel = LogLevel.Warning;
        }
        private void HandleException(BadRequestException exception)
        {
            Code = exception.Code;
            Status = (int)HttpStatusCode.BadRequest;
            Title = exception.Message;
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400";
            LogLevel = LogLevel.Warning;
        }
        private void HandleException(ForbiddenException exception)
        {
            Code = exception.Code;
            Status = (int)HttpStatusCode.Forbidden;
            Title = exception.Message;
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/403";
            LogLevel = LogLevel.Warning;
        }
        private void HandleException(Exception exception)
        {
            Code = UnhandledError;
            Status = (int)HttpStatusCode.InternalServerError;
            Title = "An unexpected error occurred."; 
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500";
            LogLevel = LogLevel.Error;
        }
    }
}
