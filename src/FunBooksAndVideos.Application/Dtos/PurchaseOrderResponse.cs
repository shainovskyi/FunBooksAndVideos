namespace FunBooksAndVideos.Application.Dtos;

public record LineItemDto(int Id, string Type, int? ProductId, string? MembershipType, string Description, decimal Price);

public record PurchaseOrderResponse(
    int Id,
    int CustomerId,
    decimal TotalPrice,
    DateTime CreatedAtUtc,
    IReadOnlyList<LineItemDto> LineItems);
