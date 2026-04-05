using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TransportRouteApi.Middleware;

public class BanCheckMiddleware
{
    private readonly RequestDelegate _next;

    public BanCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (context.Request.Path.StartsWithSegments("/api/Auth/logout", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        
        // Only check if the user is actually authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Grab their username from the JWT Claims
            var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
            
            if (username != null)
            {
                // Check the DB to see if they were banned since their token was issued
                var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
                
                if (user != null && user.IsBanned)
                {
                    // 🚨 If banned, immediately reject the request and stop processing
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "Your session was terminated: Account suspended." });
                    return; 
                }
            }
        }

        // If they aren't banned, let the request continue to the controller
        await _next(context);
    }
}