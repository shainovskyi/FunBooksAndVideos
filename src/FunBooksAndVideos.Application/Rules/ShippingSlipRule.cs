using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Rules;

/// <summary>BR2: a shipping slip is generated for orders containing physical products.</summary>
public class ShippingSlipRule(IProductRepository productRepository, IShippingSlipRepository shippingSlipRepository)
    : IDomainEventHandler<PurchaseOrderCreated>
{
    public async Task HandleAsync(PurchaseOrderCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var order = domainEvent.Order;
        if (order.GetProductLines().Count == 0)
            return;

        var productIds = order.GetProductLines().Select(l => l.ProductId!.Value).Distinct();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

        var physicalItems = products.Where(p => p.IsPhysical).Select(p => p.Name).ToList();
        if (physicalItems.Count == 0)
            return;

        var slip = new ShippingSlip(id: 0, order.Id, order.CustomerId, physicalItems);
        await shippingSlipRepository.AddAsync(slip, cancellationToken);
    }
}
