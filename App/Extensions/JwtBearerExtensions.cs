using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Models;
using Microsoft.IdentityModel.Tokens;

namespace App.Extensions;


public static class JwtBearerExtensions
{
    public static List<Claim> CreateClaims(this User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            // new(ClaimTypes.Role, string.Join(" ", roles.Select(x => x.Name)))
        };
        return claims;
    }
    
    public static SigningCredentials CreateSigningCredentials(this IConfiguration configuration)
    {
        return new(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)
            ),
            SecurityAlgorithms.HmacSha256
        );
    }

    public static JwtSecurityToken CreateJwtToken(this IEnumerable<Claim> claims, IConfiguration configuration)
    {
        var expire = configuration.GetSection("JwtSettings:Expire").Get<int>();
        
        return new(
            configuration["JwtSettings:Issuer"],
            configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(expire),
            signingCredentials: configuration.CreateSigningCredentials()
        );
    }
    
    public static JwtSecurityToken CreateToken(this IConfiguration configuration, List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));
        var tokenValidityInMinutes = configuration.GetSection("JwtSettings:TokenValidityInMinutes").Get<int>();
        
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    public static string GenerateRefreshToken(this IConfiguration configuration)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public static ClaimsPrincipal? GetPrincipalFromExpiredToken(this IConfiguration configuration, string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)),
            ValidateLifetime = false,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}