using System.Data;

namespace FunBooksAndVideos.Application.Abstractions;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }

    Task BeginAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
