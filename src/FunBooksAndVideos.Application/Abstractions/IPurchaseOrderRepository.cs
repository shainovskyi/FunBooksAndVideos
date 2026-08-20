using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Abstractions;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Persists the purchase order and its line items; returns the persisted order with generated ids populated.</summary>
    Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
}
