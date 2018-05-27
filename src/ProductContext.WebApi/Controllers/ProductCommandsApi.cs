using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductContext.Domain.Contracts;

namespace ProductContext.WebApi.Controllers
{
    [Route("api/product")]
    public class ProductCommandsApi : Controller
    {
        private readonly IMediator _mediator;

        public ProductCommandsApi(IMediator mediator) => _mediator = mediator;

        [HttpPut]
        [Route("create")]
        public Task<IActionResult> Put([FromBody] Commands.V1.CreateProduct request) =>
            HandleOrThrow(request, x => _mediator.Send(x));

        [HttpPut]
        [Route("/{productId}/contents")]
        public Task<IActionResult> Put(string productId, [FromBody] Commands.V1.AddContentToProduct request) =>
            HandleOrThrow(request, x => _mediator.Send(x));

        [HttpPut]
        [Route("/{productId}/variants")]
        public Task<IActionResult> Put(string productId, [FromBody] Commands.V1.AddVariantToProduct request) =>
            HandleOrThrow(request, x => _mediator.Send(x));

        private async Task<IActionResult> HandleOrThrow<T>(T request, Func<T, Task> handler)
        {
            try
            {
                await handler(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return NotFound(new {message = ex.ToString()});
            }
        }
    }
}