using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController(PurchaseOrderService purchaseOrderService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PurchaseOrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await purchaseOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PurchaseOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await purchaseOrderService.GetByIdAsync(id, cancellationToken));
}
