using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Mapping;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Services;

public class CustomerService(ICustomerRepository customerRepository, IShippingSlipRepository shippingSlipRepository)
{
    public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        return customer.ToDto();
    }

    public async Task<IReadOnlyList<ShippingSlipDto>> GetShippingSlipsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        _ = await customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);

        return [.. (await shippingSlipRepository.GetByCustomerIdAsync(customerId, cancellationToken)).Select(s => s.ToDto())];
    }
}
