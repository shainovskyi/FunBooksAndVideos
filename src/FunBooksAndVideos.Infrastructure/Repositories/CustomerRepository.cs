using Dapper;
using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;

namespace FunBooksAndVideos.Infrastructure.Repositories;

public class CustomerRepository(IUnitOfWork unitOfWork) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await unitOfWork.Connection.QuerySingleOrDefaultAsync<CustomerRow>(
            new CommandDefinition("SELECT Id, Name, Email FROM dbo.Customers WHERE Id = @id",
                new { id }, unitOfWork.Transaction, cancellationToken: cancellationToken));

        if (row is null)
            return null;

        var memberships = await unitOfWork.Connection.QueryAsync<string>(
            new CommandDefinition("SELECT Membership FROM dbo.CustomerMemberships WHERE CustomerId = @id",
                new { id }, unitOfWork.Transaction, cancellationToken: cancellationToken));

        return new Customer(row.Id, row.Name, row.Email, memberships.Select(Enum.Parse<MembershipType>));
    }

    public async Task UpdateMembershipsAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM dbo.CustomerMemberships WHERE CustomerId = @Id",
                new { customer.Id }, unitOfWork.Transaction, cancellationToken: cancellationToken));

        await unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO dbo.CustomerMemberships (CustomerId , Membership )
                VALUES                              (@CustomerId, @Membership)
                """,
                customer.Memberships.Select(m => new { CustomerId = customer.Id, Membership = m.ToString() }),
                unitOfWork.Transaction, cancellationToken: cancellationToken));
    }

    private record CustomerRow(int Id, string Name, string Email);
}
