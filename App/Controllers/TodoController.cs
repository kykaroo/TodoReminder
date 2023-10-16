using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly DataContext _db;
    private readonly ITokenService _tokenService;

    public TodoController(DataContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> SetTodo(string token, [FromBody] TodoItem item)
    {
        if (!_tokenService.ValidateJwtToken(token, out var userName)) return BadRequest("Время авторизации истекло");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = _db.Users.First(u => u.UserName == userName);
        item.User = user;
        user.TodoList.Add(item);
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        
        return Ok("Напоминание успешно создано");
    }

    [HttpGet]
    public IActionResult GetTodo(string token)
    {
        if (!_tokenService.ValidateJwtToken(token, out var userName)) return BadRequest("Время авторизации истекло");

        var user = _db.Users.First(u => u.UserName == userName); 
        var items = new List<TodoItem>();
        items.AddRange(_db.Items.Where(item => item.User == user));

        return Ok(items); //TODO Какой формат отправлять в ответ?
    }
}