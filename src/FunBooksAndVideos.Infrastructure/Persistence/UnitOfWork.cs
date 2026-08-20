using System.Data;
using System.Data.Common;
using FunBooksAndVideos.Application.Abstractions;

namespace FunBooksAndVideos.Infrastructure.Persistence;

public class UnitOfWork(IDbConnectionFactory connectionFactory) : IUnitOfWork
{
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public IDbTransaction? Transaction => _transaction;

    public IDbConnection Connection
    {
        get
        {
            if (_connection is null)
            {
                _connection = connectionFactory.CreateConnection();
                _connection.Open();
            }

            return _connection;
        }
    }

    private async ValueTask<IDbConnection> EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection is null)
        {
            _connection = connectionFactory.CreateConnection();

            if (_connection is DbConnection dbConnection)
                await dbConnection.OpenAsync(cancellationToken);
            else
                _connection.Open();
        }

        return _connection;
    }

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("A transaction is already active.");

        var connection = await EnsureConnectionOpenAsync(cancellationToken);

        _transaction = connection is DbConnection dbConnection
            ? await dbConnection.BeginTransactionAsync(cancellationToken)
            : connection.BeginTransaction();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        if (_transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.CommitAsync(cancellationToken);
            await dbTransaction.DisposeAsync();
        }
        else
        {
            _transaction.Commit();
            _transaction.Dispose();
        }

        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction to roll back.");

        if (_transaction is DbTransaction dbTransaction)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            await dbTransaction.DisposeAsync();
        }
        else
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            try
            {
                _transaction.Rollback();
            }
            catch (Exception)
            {
                // Connection may already be closed/broken; nothing more we can do here.
            }

            if (_transaction is IAsyncDisposable asyncDisposableTransaction)
                await asyncDisposableTransaction.DisposeAsync();
            else
                _transaction.Dispose();

            _transaction = null;
        }

        if (_connection is not null)
        {
            if (_connection is IAsyncDisposable asyncDisposableConnection)
                await asyncDisposableConnection.DisposeAsync();
            else
                _connection.Dispose();

            _connection = null;
        }

        GC.SuppressFinalize(this);
    }
}
