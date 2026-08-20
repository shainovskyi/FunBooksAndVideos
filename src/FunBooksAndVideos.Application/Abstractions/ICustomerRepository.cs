using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateMembershipsAsync(Customer customer, CancellationToken cancellationToken = default);
}
