using System.ComponentModel.DataAnnotations;

namespace CLDV6212POE1.Api.Models;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}