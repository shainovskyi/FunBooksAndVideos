using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FunBooksAndVideos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Orders : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateOrder()
        {
            // TODO: implement

            var response = new
            {
                OrderId = Guid.NewGuid(),
                Message = "Order created successfully."
            };

            return Ok(response);
        }
    }
}
