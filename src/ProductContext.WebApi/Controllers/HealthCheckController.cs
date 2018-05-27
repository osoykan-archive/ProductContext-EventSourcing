using Microsoft.AspNetCore.Mvc;

namespace ProductContext.WebApi.Controllers
{
    [Route("/")]
    public class HealthCheckController : Controller
    {
        [HttpGet]
        [Route("healthcheck")]
        public OkResult Ping() => Ok();
    }
}