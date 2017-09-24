using Microsoft.AspNetCore.Mvc;

namespace DeliveryTracker.Controllers
{
    public sealed class PingController: Controller
    {
        [HttpGet("/")]
        public IActionResult Ping()
        {
            return this.Ok();
        }
        
    }
}