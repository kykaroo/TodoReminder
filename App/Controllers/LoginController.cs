using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly DataContext _db;
    private readonly ITokenService _tokenService;
    private readonly ICryptographyService _cryptographyService;

    public LoginController(DataContext db, ITokenService tokenService, ICryptographyService cryptographyService)
    {
        _db = db;
        _tokenService = tokenService;
        _cryptographyService = cryptographyService;
    }
    
    [HttpPost]
    public string TryLogin(string name,[FromBody] string password)
    {
        var user = _db.Users.FirstOrDefault(user => user.UserName == name);
        
        if (user == null) return "Неверное имя пользователя или пароль";

        if (user.PasswordHash != _cryptographyService.Encrypt(password)) return "Неверное имя пользователя или пароль";

        return _tokenService.CreateToken(user);
    }
}