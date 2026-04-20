using Mediator;
using Microsoft.Extensions.Logging;
using Nimble.Modulith.Email.Contracts;

namespace Nimble.Modulith.Users.Events;

public class UserAddedToRoleEventHandler : INotificationHandler<UserAddedToRoleEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserAddedToRoleEventHandler> _logger;

    public UserAddedToRoleEventHandler(
        IMediator mediator,
        ILogger<UserAddedToRoleEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async ValueTask Handle(UserAddedToRoleEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "User {UserId} was added to role {RoleName}, sending email notification",
            notification.UserId,
            notification.RoleName);

        var emailCommand = new SendEmailCommand(
            To: notification.UserEmail,
            Subject: $"You've been added to the {notification.RoleName} role",
            Body: $"Hello,\n\nYou have been added to the {notification.RoleName} role in the Nimble Modulith application.\n\nBest regards,\nThe Nimble Team"
        );

        await _mediator.Send(emailCommand, cancellationToken);

        _logger.LogInformation("Email notification sent to {Email}", notification.UserEmail);
    }
}