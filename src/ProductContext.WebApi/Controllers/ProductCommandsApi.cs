using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ProductContext.Domain.Contracts;
using ProductContext.Framework;

namespace ProductContext.WebApi.Controllers
{
    [Route("api/product")]
    public class ProductCommandsApi : Controller
    {
        private readonly IBus _bus;

        public ProductCommandsApi(IBus bus)
        {
            _bus = bus;
        }

        [HttpPut]
        public Task<IActionResult> Put([FromBody] Commands.V1.CreateProduct request) =>
            HandleOrThrow(request, x => _bus.PublishAsync(x));

        [HttpPut]
        public Task<IActionResult> Put([FromBody] Commands.V1.AddContentToProduct request) =>
            HandleOrThrow(request, x => _bus.PublishAsync(x));

        [HttpPut]
        public Task<IActionResult> Put([FromBody] Commands.V1.AddVariantToProduct request) =>
            HandleOrThrow(request, x => _bus.PublishAsync(x));

        private async Task<IActionResult> HandleOrThrow<T>(T request, Func<T, Task> handler)
        {
            try
            {
                await handler(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.ToString() });
            }
        }
    }
}
