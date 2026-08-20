using Dapper;
using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Infrastructure.Repositories;

public class ProductRepository(IUnitOfWork unitOfWork) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await unitOfWork.Connection.QuerySingleOrDefaultAsync<ProductRow>(
            new CommandDefinition($"SELECT Id, Name, Price, Type FROM dbo.Products WHERE Id = @id",
                new { id }, unitOfWork.Transaction, cancellationToken: cancellationToken));
        return row?.ToEntity();
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await unitOfWork.Connection.QueryAsync<ProductRow>(
            new CommandDefinition("SELECT Id, Name, Price, Type FROM dbo.Products",
                transaction: unitOfWork.Transaction, cancellationToken: cancellationToken));
        return [.. rows.Select(r => r.ToEntity())];
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        var rows = await unitOfWork.Connection.QueryAsync<ProductRow>(
            new CommandDefinition($"SELECT Id, Name, Price, Type FROM dbo.Products WHERE Id IN @idList",
                new { idList }, unitOfWork.Transaction, cancellationToken: cancellationToken));
        return [.. rows.Select(r => r.ToEntity())];
    }

    private record ProductRow(int Id, string Name, decimal Price, string Type)
    {
        public Product ToEntity() => Type switch
        {
            "Book" => new Book(Id, Name, Price),
            "Video" => new Video(Id, Name, Price),
            _ => throw new InvalidOperationException($"Unknown product type '{Type}'.")
        };
    }
}
