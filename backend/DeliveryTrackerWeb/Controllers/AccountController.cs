using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        // account/login
        // account/about
        // account/edit
        // account/change_password
        
        [HttpPost("login")]
        public IActionResult Login()
        {
            return this.Ok();
        }
        
        [HttpGet("about")]
        public IActionResult About()
        {
            return this.Ok();
        }
        
        [HttpPost("edit")]
        public IActionResult Edit()
        {
            return this.Ok();
        }
        
        [HttpPost("change_password")]
        public IActionResult ChangePassword()
        {
            return this.Ok();
        }
    }
}