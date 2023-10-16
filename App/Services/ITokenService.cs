using App.Models;

namespace App.Services; 

public interface ITokenService
{ 
    string CreateToken(User appUser);
    bool ValidateJwtToken(string token, out string userName);
}