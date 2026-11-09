using System.ComponentModel.DataAnnotations;

namespace CLDV6212POE1.Api.Models;

public class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = "";
}