using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Mapping;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Services;

public class PurchaseOrderService(PurchaseOrderProcessor processor, IPurchaseOrderRepository purchaseOrderRepository)
{
    public Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default) =>
        processor.ProcessAsync(request, cancellationToken);

    public async Task<PurchaseOrderResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await purchaseOrderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(PurchaseOrder), id);

        return order.ToDto();
    }
}
