namespace CLDV6212POE1.Api.Models;

public class AuthResponse
{
    public string Message { get; set; } = "";

    public string Token { get; set; } = "";

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAtUtc { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "";
}