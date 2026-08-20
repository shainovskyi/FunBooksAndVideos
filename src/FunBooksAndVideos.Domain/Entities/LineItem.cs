using FunBooksAndVideos.Domain.Enums;
using FunBooksAndVideos.Domain.Exceptions;

namespace FunBooksAndVideos.Domain.Entities;

public class LineItem
{
    public int Id { get; }
    public LineItemType Type { get; }
    public int? ProductId { get; }
    public MembershipType? MembershipType { get; }
    public string Description { get; }
    public decimal Price { get; }

    private LineItem(int id, LineItemType type, int? productId, MembershipType? membershipType, string description, decimal price)
    {
        if (price < 0)
            throw new DomainException("Line item price cannot be negative.");

        Id = id;
        Type = type;
        ProductId = productId;
        MembershipType = membershipType;
        Description = description;
        Price = price;
    }

    public static LineItem ForProduct(int id, Product product) =>
        new(id, LineItemType.Product, product.Id, null, product.Name, product.Price);

    public static LineItem ForProduct(int id, int productId, string description, decimal price) =>
        new(id, LineItemType.Product, productId, null, description, price);

    public static LineItem ForMembership(int id, MembershipType membershipType, decimal price) =>
        new(id, LineItemType.Membership, null, membershipType,
            $"{membershipType} Membership", price);
}
