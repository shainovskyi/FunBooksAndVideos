using Dapper;
using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Infrastructure.Repositories;

public class ShippingSlipRepository(IUnitOfWork unitOfWork) : IShippingSlipRepository
{
    public async Task<int> AddAsync(ShippingSlip shippingSlip, CancellationToken cancellationToken = default)
    {
        var slipId = await unitOfWork.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                INSERT INTO dbo.ShippingSlips (PurchaseOrderId , CustomerId , CreatedAtUtc )
                OUTPUT INSERTED.Id
                VALUES                        (@PurchaseOrderId, @CustomerId, @CreatedAtUtc)
                """,
                new { shippingSlip.PurchaseOrderId, shippingSlip.CustomerId, shippingSlip.CreatedAtUtc },
                unitOfWork.Transaction, cancellationToken: cancellationToken));

        await unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO dbo.ShippingSlipItems (ShippingSlipId , ItemName )
                VALUES                            (@ShippingSlipId, @ItemName)
                """,
                shippingSlip.ItemsToShip.Select(i => new { ShippingSlipId = slipId, ItemName = i }),
                unitOfWork.Transaction, cancellationToken: cancellationToken));

        return slipId;
    }

    public async Task<IReadOnlyList<ShippingSlip>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var slips = (await unitOfWork.Connection.QueryAsync<SlipRow>(
            new CommandDefinition("SELECT Id, PurchaseOrderId, CustomerId, CreatedAtUtc FROM dbo.ShippingSlips WHERE CustomerId = @customerId",
                new { customerId }, unitOfWork.Transaction, cancellationToken: cancellationToken))).ToList();

        if (slips.Count == 0)
            return [];

        var slipIds = slips.Select(s => s.Id).ToList();
        var items = (await unitOfWork.Connection.QueryAsync<ItemRow>(
            new CommandDefinition("SELECT ShippingSlipId, ItemName FROM dbo.ShippingSlipItems WHERE ShippingSlipId IN @slipIds",
                new { slipIds }, unitOfWork.Transaction, cancellationToken: cancellationToken)))
            .ToLookup(i => i.ShippingSlipId, i => i.ItemName);

        return [.. slips.Select(s => new ShippingSlip(s.Id, s.PurchaseOrderId, s.CustomerId, items[s.Id],
            DateTime.SpecifyKind(s.CreatedAtUtc, DateTimeKind.Utc)))];
    }

    private record SlipRow(int Id, int PurchaseOrderId, int CustomerId, DateTime CreatedAtUtc);
    private record ItemRow(int ShippingSlipId, string ItemName);
}
