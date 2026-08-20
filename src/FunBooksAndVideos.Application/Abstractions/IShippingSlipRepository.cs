using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Abstractions;

public interface IShippingSlipRepository
{
    Task<int> AddAsync(ShippingSlip shippingSlip, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShippingSlip>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
}
