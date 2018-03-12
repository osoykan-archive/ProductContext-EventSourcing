using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using ProductContext.Common.Bus;
using ProductContext.Domain.Commands;
using ProductContext.WebApi.Models;

namespace ProductContext.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly IBus _bus;

        public ProductController(IBus bus)
        {
            _bus = bus;
        }

        [HttpPut]
        public Task CreateProduct([FromBody]CreateProductModel model)
        {
            return _bus.PublishAsync(
                new CreateProduct(
                    model.Code, 
                    model.BrandId,
                    model.AgeGroupId,
                    model.GenderId, 
                    model.BusinessUnitId));
        }
    }
}
