using System.Globalization;

namespace EventFlow.API.Middlewares
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _supportedCultures = { "en-US", "ka-GE" };
        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var cultureName = "en-US";

            var queryCulture = context.Request.Headers["Accept-Language"].ToString();

            if (!string.IsNullOrWhiteSpace(queryCulture))
            {
                var extractedCulture = queryCulture.Split(',')[0];
                if (_supportedCultures.Contains(extractedCulture))
                {
                    cultureName = extractedCulture;
                }
            }
            var culture = new CultureInfo(cultureName);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await _next(context);
        }
    }
}
