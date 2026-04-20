using Ardalis.Result;
using FastEndpoints;
using Mediator;
using Nimble.Modulith.Customers.UseCases.Customers;
using Nimble.Modulith.Customers.UseCases.Customers.Commands;

namespace Nimble.Modulith.Customers.Endpoints.Customers;

public class Create(IMediator mediator) : Endpoint<CreateCustomerRequest, CustomerResponse>
{
    public override void Configure()
    {
        Post("/customers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new customer";
            s.Description = "Creates a new customer with the provided information";
        });
        Tags("customers");
    }

    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        var command = new CreateCustomerCommand(
            req.FirstName,
            req.LastName,
            req.Email,
            req.PhoneNumber,
            req.Address.Street,
            req.Address.City,
            req.Address.State,
            req.Address.PostalCode,
            req.Address.Country
        );

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Map UseCases DTO to Endpoint Response DTO
        Response = new CustomerResponse(
            result.Value.Id,
            result.Value.FirstName,
            result.Value.LastName,
            result.Value.Email,
            result.Value.PhoneNumber,
            new AddressResponse(
                result.Value.Address.Street,
                result.Value.Address.City,
                result.Value.Address.State,
                result.Value.Address.PostalCode,
                result.Value.Address.Country
            )
        );

        await Send.CreatedAtAsync<GetById>(new { id = result.Value.Id }, generateAbsoluteUrl: false, cancellation: ct);
    }
}