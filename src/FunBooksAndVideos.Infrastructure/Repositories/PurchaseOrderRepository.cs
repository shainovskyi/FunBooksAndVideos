using Dapper;
using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;

namespace FunBooksAndVideos.Infrastructure.Repositories;

public class PurchaseOrderRepository(IUnitOfWork unitOfWork) : IPurchaseOrderRepository
{
    public async Task<PurchaseOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var header = await unitOfWork.Connection.QuerySingleOrDefaultAsync<OrderRow>(
            new CommandDefinition("SELECT Id, CustomerId, CreatedAtUtc FROM dbo.PurchaseOrders WHERE Id = @id",
                new { id }, unitOfWork.Transaction, cancellationToken: cancellationToken));

        if (header is null)
            return null;

        var lines = await unitOfWork.Connection.QueryAsync<LineRow>(
            new CommandDefinition("SELECT Id, Type, ProductId, MembershipType, Description, Price FROM dbo.LineItems WHERE PurchaseOrderId = @id",
                new { id }, unitOfWork.Transaction, cancellationToken: cancellationToken));

        var lineItems = lines.Select(l => l.Type == nameof(LineItemType.Membership)
            ? LineItem.ForMembership(l.Id, Enum.Parse<MembershipType>(l.MembershipType!), l.Price)
            : LineItem.ForProduct(l.Id, l.ProductId!.Value, l.Description, l.Price));

        return new PurchaseOrder(header.Id, header.CustomerId, lineItems, DateTime.SpecifyKind(header.CreatedAtUtc, DateTimeKind.Utc));
    }

    public async Task<PurchaseOrder> AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        var orderId = await unitOfWork.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO dbo.PurchaseOrders (CustomerId , TotalPrice , CreatedAtUtc )
                OUTPUT INSERTED.Id
                VALUES                         (@CustomerId, @TotalPrice, @CreatedAtUtc)
                """,
                new { purchaseOrder.CustomerId, purchaseOrder.TotalPrice, purchaseOrder.CreatedAtUtc },
                unitOfWork.Transaction, cancellationToken: cancellationToken));

        var persistedLineItems = new List<LineItem>(purchaseOrder.LineItems.Count);
        foreach (var lineItem in purchaseOrder.LineItems)
        {
            var lineItemId = await unitOfWork.Connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    """
                    INSERT INTO dbo.LineItems (PurchaseOrderId , Type , ProductId , MembershipType , Description , Price )
                    OUTPUT INSERTED.Id
                    VALUES                    (@PurchaseOrderId, @Type, @ProductId, @MembershipType, @Description, @Price)
                    """,
                    new
                    {
                        PurchaseOrderId = orderId,
                        Type = lineItem.Type.ToString(),
                        lineItem.ProductId,
                        MembershipType = lineItem.MembershipType?.ToString(),
                        lineItem.Description,
                        lineItem.Price
                    },
                    unitOfWork.Transaction, cancellationToken: cancellationToken));

            persistedLineItems.Add(lineItem.Type == LineItemType.Membership
                ? LineItem.ForMembership(lineItemId, lineItem.MembershipType!.Value, lineItem.Price)
                : LineItem.ForProduct(lineItemId, lineItem.ProductId!.Value, lineItem.Description, lineItem.Price));
        }

        return new PurchaseOrder(orderId, purchaseOrder.CustomerId, persistedLineItems, purchaseOrder.CreatedAtUtc);
    }

    private record OrderRow(int Id, int CustomerId, DateTime CreatedAtUtc);
    private record LineRow(int Id, string Type, int? ProductId, string? MembershipType, string Description, decimal Price);
}
