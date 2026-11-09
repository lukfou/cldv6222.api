using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CLDV6212POE1.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace CLDV6212POE1.Api.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(
        UserProfile user)
    {
        string issuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is missing.");

        string audience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is missing.");

        string key =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is missing.");

        int expiryMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpiryMinutes") ?? 60;

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        DateTime expiresAtUtc =
            DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim("name", user.FullName),
            new Claim("email", user.RowKey),
            new Claim("role", user.PartitionKey)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        string encodedToken =
            new JwtSecurityTokenHandler().WriteToken(token);

        return (encodedToken, expiresAtUtc);
    }
}