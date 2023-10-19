using System.Linq.Expressions;
using App.Models;
using App.Services;
using Hangfire;
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
        if (!_tokenService.ValidateJwtToken(token, out var userName)) return Forbid("Время аунтефикации истекло");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = _db.Users.First(u => u.UserName == userName);
        item.User = user;
        user.TodoList.Add(item);
        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        

        var executionTime = item.ExecutionTime == null
            ? DateTime.Now + TimeSpan.FromHours(7) - DateTime.Now
            : DateTime.Now - item.ExecutionTime.Value;

        BackgroundJob.Schedule(SendNotification(item), executionTime);
        
        return Ok("Напоминание успешно создано");
    }

    private Expression<Action> SendNotification(TodoItem item)
    {
        //TODO отправка уведомлений
        throw new NotImplementedException();
    }

    [HttpGet]
    public IActionResult GetTodo(string token)
    {
        if (!_tokenService.ValidateJwtToken(token, out var userName)) return Forbid("Время аунтефикации истекло");

        return Ok(_db.Items.Where(item => item.User == _db.Users.First(u => u.UserName == userName)));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTodo(string token, [FromBody] int todoId)
    {
        if (!_tokenService.ValidateJwtToken(token, out var userName)) return Forbid("Время аунтефикации истекло");

        var item = _db.Items.First(item => item.Id == todoId);

        if (item.User != _db.Users.First(u => u.UserName == userName)) return BadRequest("Вы не являетесь владельцем данного напоминания");
        
        var text = item.Text;
        _db.Remove(item);
        await _db.SaveChangesAsync();

        return Ok($"Напоминание \"{text}\" успешно удалено");
    }
}