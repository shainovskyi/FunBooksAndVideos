using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Mapping;

public static class DtoMapping
{
    public static ProductDto ToDto(this Product product) =>
        new(product.Id, product.Name, product.Price, product.GetType().Name, product.IsPhysical);

    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, [.. customer.Memberships.Select(m => m.ToString())]);

    public static LineItemDto ToDto(this LineItem line) =>
        new(line.Id, line.Type.ToString(), line.ProductId, line.MembershipType?.ToString(), line.Description, line.Price);

    public static PurchaseOrderResponse ToDto(this PurchaseOrder order) =>
        new(order.Id, order.CustomerId, order.TotalPrice, order.CreatedAtUtc, [.. order.LineItems.Select(l => l.ToDto())]);

    public static ShippingSlipDto ToDto(this ShippingSlip slip) =>
        new(slip.Id, slip.PurchaseOrderId, slip.CustomerId, slip.ItemsToShip, slip.CreatedAtUtc);
}
