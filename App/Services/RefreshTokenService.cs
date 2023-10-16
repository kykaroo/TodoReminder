using System.IdentityModel.Tokens.Jwt;
using App.Extensions;

namespace App.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly DataContext _db;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(DataContext db,
        IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<TokenModel> RefreshToken(TokenModel tokenModel)
    {
        if (tokenModel is null)
            throw new ArgumentNullException(nameof(tokenModel));

        var accessToken = tokenModel.AccessToken;
        var refreshToken = tokenModel.RefreshToken;
        var principal = _configuration.GetPrincipalFromExpiredToken(accessToken);

        if (principal == null)
            throw new Exception("Invalid access token or refresh token");

        var username = principal.Identity!.Name;
        var user = await _db.Users.FindAsync(username);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new Exception("Invalid access token or refresh token");

        var newAccessToken = _configuration.CreateToken(principal.Claims.ToList());
        var newRefreshToken = _configuration.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return new TokenModel
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken = newRefreshToken,
        };
    }

    public async Task Revoke(string username)
    {
        var user = await _db.Users.FindAsync(username);
        if (user == null)
            throw new Exception($"User {username} not found");

        user.RefreshToken = null;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAll()
    {
        var users = _db.Users.ToList();
        foreach (var user in users)
        {
            user.RefreshToken = null;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}