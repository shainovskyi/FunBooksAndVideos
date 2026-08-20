namespace FunBooksAndVideos.Application.Dtos;

public record ShippingSlipDto(int Id, int PurchaseOrderId, int CustomerId, IReadOnlyList<string> ItemsToShip, DateTime CreatedAtUtc);
