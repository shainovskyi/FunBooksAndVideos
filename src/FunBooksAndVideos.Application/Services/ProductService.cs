using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Mapping;
using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Services;

public class ProductService(IProductRepository productRepository)
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        [.. (await productRepository.GetAllAsync(cancellationToken)).Select(p => p.ToDto())];

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return product.ToDto();
    }
}
