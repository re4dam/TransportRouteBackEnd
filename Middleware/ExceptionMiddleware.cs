using System.Net;
using System.Text.Json;

namespace TransportRoute.Middleware // Adjust namespace if yours is different
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        // 1. We inject three tools: 
        // - 'next' (the pointer to the next step in the API)
        // - 'logger' (the built-in ASP.NET Core terminal logger)
        // - 'env' (tells us if we are in Development or Production)
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // 2. This method fires on EVERY single HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // We let the request pass through to the Controllers...
                await _next(context);
            }
            catch (Exception ex)
            {
                // 3. IF A CONTROLLER CRASHES, WE CATCH IT HERE!
                
                // Print the giant red error and stack trace to your VS Code terminal
                _logger.LogError(ex, "🚨 SHIELD CAUGHT AN EXCEPTION: {Message}", ex.Message);

                // 4. Format a polite JSON response for Next.js
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // If we are developing locally, send the stack trace to the browser so we can debug.
                // If we are in production, hide the stack trace so hackers can't read our code!
                var response = _env.IsDevelopment()
                    ? new { statusCode = context.Response.StatusCode, message = ex.Message, details = ex.StackTrace?.ToString() }
                    : new { statusCode = context.Response.StatusCode, message = "An unexpected error occurred.", details = (string?)null };

                // Ensure the JSON matches Next.js camelCase expectations
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}