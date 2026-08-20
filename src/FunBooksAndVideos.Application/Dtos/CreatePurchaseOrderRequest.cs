using System.ComponentModel.DataAnnotations;
using FunBooksAndVideos.Domain.Enums;

namespace FunBooksAndVideos.Application.Dtos;

public record CreateLineItemRequest(int? ProductId, MembershipType? MembershipType);

public record CreatePurchaseOrderRequest(
    [Range(1, int.MaxValue)] int CustomerId,
    [Required, MinLength(1)] IReadOnlyList<CreateLineItemRequest> LineItems);
