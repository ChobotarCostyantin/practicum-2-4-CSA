using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FastEndpoints.Security;

namespace Nimble.Modulith.Users.Endpoints;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Token { get; set; }
}

public class Login(UserManager<IdentityUser> userManager) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/login");
        AllowAnonymous();
        Summary(s => {
            s.Summary = "Login with email and password";
            s.Description = "Authenticates a user and returns a token";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);

        if (user != null && await userManager.CheckPasswordAsync(user, req.Password))
        {
            var roles = await userManager.GetRolesAsync(user);

            var jwtToken = JwtBearer.CreateToken(o =>
            {
                o.SigningKey = Config["Auth:JwtSecret"]!;
                o.ExpireAt = DateTime.UtcNow.AddDays(7);
                o.User.Roles.AddRange(roles);
                o.User.Claims.Add((ClaimTypes.Email, user.Email!));
                o.User.Claims.Add((ClaimTypes.Name, user.Email!));
            });

            await Send.OkAsync(new LoginResponse { Success = true, Message = "Login successful", Token = jwtToken },
                ct);
        }
        else
        {
            await Send.UnauthorizedAsync(ct);
        }
    }
}