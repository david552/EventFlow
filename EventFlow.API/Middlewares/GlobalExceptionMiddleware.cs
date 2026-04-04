using EventFlow.API.Models;
using Newtonsoft.Json;

namespace EventFlow.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "exception occurred during request {Path}. TraceId: {TraceId}",
                 context.Request.Path, context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
          }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var error = new APIError(context, ex);
            var result = JsonConvert.SerializeObject(error);

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = error.Status.Value;

            await context.Response.WriteAsync(result);


        }
    }
}
