using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Helpers;
using DeliveryTracker.Services;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTracker.Controllers
{
    public class SessionController: Controller
    {

        private readonly AccountService accountService;

        public SessionController(
            AccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("/session/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestViewModel loginRequest)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState.ToErrorListViewModel());
            }
            
            var response = await this.accountService.Login(
                loginRequest.UserName, 
                loginRequest.Password,
                loginRequest.Role);
            return response != null ? 
                (IActionResult)this.Ok(response) :
                this.Unauthorized();
        }
        
        
    }
}