using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransportRoute.Core.Data;
using TransportRoute.Core.Models;
using TransportRoute.Security.Interfaces;
using TransportRoute.Security.Services;
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
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext context, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IAntiforgery antiforgery, IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _antiforgery = antiforgery;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists.");

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        string verificationToken = Convert.ToHexString(tokenBytes);

        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role, // Set the user role from the DTO
            EmailVerificationToken = verificationToken,
            IsEmailVerified = false
        };

        // Save the new user to the database
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Send the verification email
        string verificationLink = $"http://localhost:3000/verify-email?token={verificationToken}";
        string emailBody = $"<h3>Welcome to TransportRoute!</h3><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>.</p>";
        
        await _emailService.SendEmailAsync(newUser.Email, "Verify your account", emailBody);

        return Ok(new { message = "Registration successful. Please check your email to verify your account before logging in." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        if (user.IsBanned)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "This account has been suspended by an administrator." });
        }

        if (!user.IsEmailVerified)
        {
            return Unauthorized(new { message = "You must verify your email address before logging in." });
        }

        var roles = new string[] { user.Role };

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

        return Ok(new { message = "Logged in successfully", roles = roles }); // No token in the body!
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

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "Verification token is required." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token);
        
        if (user == null) return BadRequest(new { message = "Invalid verification token." });

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null; // Wipe it so it can't be used again
        await _context.SaveChangesAsync();

        return Ok(new { message = "Email verified successfully! You may now log in." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username is required." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        // SECURITY RULE: Always return OK even if the user isn't found. 
        // This prevents hackers from using this endpoint to guess valid usernames.
        if (user == null)
            return Ok(new { message = "If that account exists, a reset link has been generated." });

        // 1. Generate a secure, 64-character random hex string
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        string resetToken = Convert.ToHexString(randomBytes);

        // 2. Assign token and set expiration (e.g., 15 minutes from now)
        user.PasswordResetToken = resetToken;
        user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        string resetLink = $"http://localhost:3000/reset-password?token={resetToken}";
        string emailBody = $"<p>Click <a href='{resetLink}'>here</a> to reset your password. This link expires in 15 minutes.</p>";

        await _emailService.SendEmailAsync(user.Email, "Password Reset Request", emailBody);

        return Ok(new { message = "If that account exists, a reset link has been generated." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Reset token and new password are required." });

        // 1. Find the user holding this exact token
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

        // 2. Validate token existence and expiration
        if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
            return BadRequest(new { message = "Invalid or expired password reset token." });

        // 3. Hash the new password using your existing hasher
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // 4. Wipe the token so it can never be used again
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password has been successfully reset." });
    }
}