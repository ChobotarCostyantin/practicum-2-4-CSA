using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace Nimble.Modulith.Users.Endpoints;

public class LogoutResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class Logout : EndpointWithoutRequest<LogoutResponse>
{

    public override void Configure()
    {
        Post("/logout");
        AllowAnonymous(); // For now, we'll require auth later when we implement JWT properly
        Summary(s => {
            s.Summary = "Logout the current user";
            s.Description = "Signs out the current user";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Response = new LogoutResponse
        {
            Success = true,
            Message = "Logged out successfully"
        };
        
        await Send.OkAsync(Response, ct);
    }
}