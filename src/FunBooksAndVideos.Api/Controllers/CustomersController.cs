using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(CustomerService customerService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await customerService.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/shipping-slips")]
    [ProducesResponseType<IReadOnlyList<ShippingSlipDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShippingSlips(int id, CancellationToken cancellationToken) =>
        Ok(await customerService.GetShippingSlipsAsync(id, cancellationToken));
}
