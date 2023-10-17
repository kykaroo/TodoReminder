using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private readonly DataContext _db;
    private readonly ICryptographyService _cryptographyService;

    public RegisterController(DataContext db, ICryptographyService cryptographyService)
    {
        _db = db;
        _cryptographyService = cryptographyService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(string name,[FromBody] string password)
    {
        if (_db.Users.Any(user => user.UserName == name)) return BadRequest($"Пользователь с именем {name} уже существует");

        var user = new User()
        {
            UserName = name,
            PasswordHash = _cryptographyService.Encrypt(password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        
        return Ok("Пользователь успешно создан");
    }
}