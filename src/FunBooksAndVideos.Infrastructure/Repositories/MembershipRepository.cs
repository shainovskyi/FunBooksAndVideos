using Dapper;
using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Domain.Enums;

namespace FunBooksAndVideos.Infrastructure.Repositories;

public class MembershipRepository(IUnitOfWork unitOfWork) : IMembershipRepository
{
    public async Task<IReadOnlyDictionary<MembershipType, decimal>> GetPricesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await unitOfWork.Connection.QueryAsync<MembershipPriceRow>(
            new CommandDefinition("SELECT MembershipType, Price FROM dbo.MembershipPrices",
                transaction: unitOfWork.Transaction, cancellationToken: cancellationToken));

        return rows.ToDictionary(r => Enum.Parse<MembershipType>(r.MembershipType), r => r.Price);
    }

    private record MembershipPriceRow(string MembershipType, decimal Price);
}
