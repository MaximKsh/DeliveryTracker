using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    public sealed class PingController : Controller
    {
        [HttpGet("/")]
        public IActionResult Ping()
        {
            return this.Ok();
        }
        
    }
}