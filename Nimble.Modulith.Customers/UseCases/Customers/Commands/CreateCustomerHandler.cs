using Ardalis.Result;
using Mediator;
using Nimble.Modulith.Customers.Domain.CustomerAggregate;
using Nimble.Modulith.Customers.Domain.Interfaces;

namespace Nimble.Modulith.Customers.UseCases.Customers.Commands;

public class CreateCustomerHandler(IRepository<Customer> repository) 
    : ICommandHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async ValueTask<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var customer = new Customer
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            Address = new Address
            {
                Street = command.Street,
                City = command.City,
                State = command.State,
                PostalCode = command.PostalCode,
                Country = command.Country
            }
        };

        await repository.AddAsync(customer, ct);
        await repository.SaveChangesAsync(ct);

        var dto = new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            new AddressDto(
                customer.Address.Street,
                customer.Address.City,
                customer.Address.State,
                customer.Address.PostalCode,
                customer.Address.Country
            ),
            customer.CreatedAt,
            customer.UpdatedAt
        );

        return Result<CustomerDto>.Success(dto);
    }
}