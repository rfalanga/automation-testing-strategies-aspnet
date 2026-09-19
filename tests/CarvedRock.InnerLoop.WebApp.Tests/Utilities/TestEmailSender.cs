using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarvedRock.InnerLoop.WebApp.Tests.Utilities;

public class TestEmailSender : IEmailSender
{
    private readonly SharedFixture _fixture;
    public TestEmailSender(SharedFixture fixture) => _fixture = fixture;

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var id = Guid.NewGuid().ToString();
        _fixture.SentEmails.Add(new EmailModel(email, subject, htmlMessage, "e-commerce@carvedrock.com", id));
        return Task.CompletedTask;
    }
}
