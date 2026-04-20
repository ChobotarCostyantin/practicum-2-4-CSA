using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Nimble.Modulith.Users.Events;

namespace Nimble.Modulith.Users.Endpoints;

public class AddRoleToUserRequest
{
  public string RoleName { get; set; } = string.Empty;
}

public class AddRoleToUserResponse
{
  public string Message { get; set; } = string.Empty;
}

public class AddRoleToUser : Endpoint<AddRoleToUserRequest, AddRoleToUserResponse>
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly IMediator _mediator;

  public AddRoleToUser(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMediator mediator)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _mediator = mediator;
  }

  public override void Configure()
  {
    Post("/users/{id}/roles");
    Roles("Admin");
  }

  public override async Task HandleAsync(AddRoleToUserRequest req, CancellationToken ct)
  {
    var userId = Route<string>("id")!;

    // Find the user
    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      AddError($"User with ID '{userId}' not found");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    // Normalize the role name (handle "admin" -> "Admin")
    var normalizedRoleName = char.ToUpper(req.RoleName[0]) + req.RoleName.Substring(1).ToLower();

    // Check if the role exists (only Admin and Customer are valid)
    if (normalizedRoleName != "Admin" && normalizedRoleName != "Customer")
    {
      AddError($"Role '{normalizedRoleName}' does not exist. Valid roles are: Admin, Customer");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    // Check if user is already in the role
    if (await _userManager.IsInRoleAsync(user, normalizedRoleName))
    {
      AddError($"User is already in the '{normalizedRoleName}' role");
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    // Add the user to the role
    var result = await _userManager.AddToRoleAsync(user, normalizedRoleName);
    if (!result.Succeeded)
    {
      foreach (var error in result.Errors)
      {
        AddError(error.Description);
      }
      await Send.ErrorsAsync(cancellation: ct);
      return;
    }

    // Publish domain event for email notification
    var userAddedEvent = new UserAddedToRoleEvent(
      UserId: user.Id,
      UserEmail: user.Email!,
      RoleName: normalizedRoleName
    );

    await _mediator.Publish(userAddedEvent, ct);

    // Send success response
    Response.Message = $"User '{user.Email}' successfully added to role '{normalizedRoleName}'";
  }
}