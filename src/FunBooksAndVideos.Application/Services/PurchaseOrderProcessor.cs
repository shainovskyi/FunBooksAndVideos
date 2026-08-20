using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Mapping;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Services;

public class PurchaseOrderProcessor(
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IPurchaseOrderRepository purchaseOrderRepository,
    IMembershipRepository membershipRepository,
    IDomainEventDispatcher eventDispatcher,
    IUnitOfWork unitOfWork)
{
    public async Task<PurchaseOrderResponse> ProcessAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var lineItems = await ValidateAndBuildLineItemsAsync(request, cancellationToken);

        await unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var order = new PurchaseOrder(id: 0, request.CustomerId, lineItems);
            var persistedOrder = await purchaseOrderRepository.AddAsync(order, cancellationToken);

            // Deliberately published before commit: business rules (BR1, BR2) must be
            // atomic with the order — any handler failure rolls back the whole order.
            await eventDispatcher.PublishAsync(new PurchaseOrderCreated(persistedOrder), cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);
            return persistedOrder.ToDto();
        }
        catch
        {
            // Deliberately CancellationToken.None: if we got here *because* the token
            // was cancelled, passing it would abort the rollback itself.
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<List<LineItem>> ValidateAndBuildLineItemsAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.LineItems is not { Count: > 0 })
            throw new ValidationException("A purchase order must contain at least one line item.");

        _ = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.LineItems
            .Where(l => l.ProductId.HasValue)
            .Select(l => l.ProductId!.Value)
            .Distinct()
            .ToList();

        var products = (await productRepository.GetByIdsAsync(productIds, cancellationToken))
            .ToDictionary(p => p.Id);

        var membershipPrices = request.LineItems.Any(l => l.MembershipType.HasValue)
            ? await membershipRepository.GetPricesAsync(cancellationToken)
            : null;

        var lineItems = new List<LineItem>();
        foreach (var line in request.LineItems)
        {
            switch (line)
            {
                case { ProductId: not null, MembershipType: not null }:
                    throw new ValidationException("An line item cannot be both a product and a membership.");

                case { ProductId: { } productId }:
                    if (!products.TryGetValue(productId, out var product))
                        throw new NotFoundException(nameof(Product), productId);
                    lineItems.Add(LineItem.ForProduct(id: 0, product));
                    break;

                case { MembershipType: { } membershipType }:
                    if (membershipPrices is null || !membershipPrices.TryGetValue(membershipType, out var membershipPrice))
                        throw new ValidationException($"No price is defined for membership type '{membershipType}'.");
                    lineItems.Add(LineItem.ForMembership(id: 0, membershipType, membershipPrice));
                    break;

                default:
                    throw new ValidationException("Each line item must specify either a product or a membership.");
            }
        }

        return lineItems;
    }
}
