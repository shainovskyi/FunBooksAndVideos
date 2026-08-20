using FunBooksAndVideos.Domain.Enums;

namespace FunBooksAndVideos.Application.Abstractions;

public interface IMembershipRepository
{
    Task<IReadOnlyDictionary<MembershipType, decimal>> GetPricesAsync(CancellationToken cancellationToken = default);
}
