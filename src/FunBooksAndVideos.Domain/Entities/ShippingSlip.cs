using FunBooksAndVideos.Domain.Exceptions;

namespace FunBooksAndVideos.Domain.Entities;

/// <summary>
/// Shipping slip generated for purchase orders containing physical products (BR2).
/// </summary>
public class ShippingSlip
{
    public int Id { get; }
    public int PurchaseOrderId { get; }
    public int CustomerId { get; }
    public IReadOnlyList<string> ItemsToShip { get; }
    public DateTime CreatedAtUtc { get; }

    public ShippingSlip(int id, int purchaseOrderId, int customerId, IEnumerable<string> itemsToShip, DateTime? createdAtUtc = null)
    {
        var items = itemsToShip?.ToList() ?? throw new DomainException("Shipping slip items are required.");
        if (items.Count == 0)
            throw new DomainException("A shipping slip must contain at least one item.");

        Id = id;
        PurchaseOrderId = purchaseOrderId;
        CustomerId = customerId;
        ItemsToShip = items;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }
}
