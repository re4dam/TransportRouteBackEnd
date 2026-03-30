using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRoute.Security.Interfaces;
using TransportRouteApi.DTOs; // <-- Add this to import the newly separated DTO

namespace TransportRouteApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IAntiforgery _antiforgery;

    public AuthController(AppDbContext context, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IAntiforgery antiforgery)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _antiforgery = antiforgery;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role // Set the user role from the DTO
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        string token = _jwtProvider.Generate(user.Id.ToString(), user.Username, user.Role);

        // Define the secure cookie rules
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript cannot read this
            Secure = false,  // Set to TRUE in production when using HTTPS!
            SameSite = SameSiteMode.Lax, // Allows it to work across local ports
            Expires = DateTime.UtcNow.AddDays(1)
        };

        // Attach the cookie to the HTTP Response
        Response.Cookies.Append("jwt", token, cookieOptions);

        return Ok(new { message = "Logged in successfully" }); // No token in the body!
    }

    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken()
    {
        // This generates the token pair (one cookie, one request token)
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        
        // We return the request token to the frontend so it can attach it to headers
        return Ok(new { token = tokens.RequestToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // The name "jwt" MUST match exactly what you named it during Login
        Response.Cookies.Delete("jwt", new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to false if you aren't using HTTPS locally
            SameSite = SameSiteMode.None // Must match how you set it during login!
        });
        
        return Ok(new { message = "Successfully logged out." });
    }
}