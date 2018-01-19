using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    public sealed class ServiceController : Controller
    {
        [HttpGet("/")]
        public IActionResult Alive()
        {
            return this.Ok();
        }
        
    }
}