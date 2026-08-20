using FunBooksAndVideos.Domain.Enums;
using FunBooksAndVideos.Domain.Exceptions;

namespace FunBooksAndVideos.Domain.Entities;

public class PurchaseOrder
{
    private readonly List<LineItem> _lineItems;

    public int Id { get; }
    public int CustomerId { get; }
    public decimal TotalPrice { get; }
    public DateTime CreatedAtUtc { get; }
    public IReadOnlyList<LineItem> LineItems => _lineItems;

    public PurchaseOrder(int id, int customerId, IEnumerable<LineItem> lineItems, DateTime? createdAtUtc = null)
    {
        _lineItems = lineItems?.ToList() ?? throw new DomainException("Purchase order line items are required.");
        if (_lineItems.Count == 0)
            throw new DomainException("A purchase order must contain at least one line item.");

        Id = id;
        CustomerId = customerId;
        TotalPrice = _lineItems.Sum(l => l.Price);
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public bool ContainsMemberships => _lineItems.Any(l => l.Type == LineItemType.Membership);

    public IReadOnlyList<MembershipType> GetMemberships() =>
        [.. _lineItems.Where(l => l.Type == LineItemType.Membership)
                  .Select(l => l.MembershipType!.Value)
                  .Distinct()];

    public IReadOnlyList<LineItem> GetProductLines() => [.. _lineItems.Where(l => l.Type == LineItemType.Product)];
}
