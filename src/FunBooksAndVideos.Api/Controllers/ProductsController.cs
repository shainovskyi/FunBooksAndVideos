using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await productService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await productService.GetByIdAsync(id, cancellationToken));
}
