using System.IdentityModel.Tokens.Jwt;
using System.Text;
using App.Extensions;
using App.Models;
using Microsoft.IdentityModel.Tokens;

namespace App.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var token = user
            .CreateClaims()
            .CreateJwtToken(_configuration);
        JwtSecurityTokenHandler tokenHandler = new();

        return tokenHandler.WriteToken(token);
    }

    public bool ValidateJwtToken(string token, out string userName)
    {
        userName = default!;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]!);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Можете настроить в соответствии с вашими требованиями
            ValidateAudience = false, // Можете настроить в соответствии с вашими требованиями
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            userName = principal.Identity.Name;
            return true; // Токен действителен
        }
        catch
        {
            return false; // Токен недействителен
        }
    }
}