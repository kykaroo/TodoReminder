using Microsoft.AspNetCore.Identity.UI.Services;

namespace App.Services;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // TODO Допилить отправку подтверждения по почте
        await Task.CompletedTask;
    }
}