using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Helpers;
using DeliveryTracker.Services;
using DeliveryTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTracker.Controllers
{
    public class SessionController: Controller
    {
        #region fields
        
        private readonly AccountService accountService;

        #endregion
        
        #region constuctor
        
        public SessionController(
            AccountService accountService)
        {
            this.accountService = accountService;
        }

        #endregion
        
        #region actions
        
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

        [Authorize]
        [HttpGet("/session/check")]
        public async Task<IActionResult> CheckSession()
        {
            var user = await this.accountService.FindUser(this.User.Identity.Name);
            var role = await this.accountService.FindRole(user);
            if (user != null
                && role != null)
            {
                return this.Ok(new UserInfoViewModel
                {
                    DisplayableName = user.DisplayableName,
                    Group = user.Group.DisplayableName,
                    Role = role,
                    UserName = user.UserName,
                });
            }
            return this.NotFound();
        }
        
        #endregion
        
    }
}