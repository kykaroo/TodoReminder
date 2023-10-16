namespace App.Services;

public interface IRefreshTokenService
{
    Task<TokenModel> RefreshToken(TokenModel tokenModel);
    Task Revoke(string username);
    Task RevokeAll();
}