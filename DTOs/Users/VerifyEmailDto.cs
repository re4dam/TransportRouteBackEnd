using System.ComponentModel.DataAnnotations;

namespace TransportRouteApi.DTOs;

public class VerifyEmailDto
{
    [Required(ErrorMessage = "Verification token is required.")]
    public string Token { get; set; } = string.Empty;
}