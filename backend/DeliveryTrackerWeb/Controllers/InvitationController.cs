using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.CreatorOrManager)]   
    [Route("api/invitation")]
    public class InvitationController : Controller
    {
        private readonly IInvitationService invitationService;
        
        public InvitationController(IInvitationService invitationService)
        {
            this.invitationService = invitationService;
        }

        // invitation/create
        // invitation/get
        // invitation/delete
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateInvitation(User preliminaryUserData)
        {
            var result = await this.invitationService.CreateAsync(preliminaryUserData);
            if (result.Success)
            {
                return this.Ok(result.Result);
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(result.Errors);
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetInvitation(string code)
        {
            var result = await this.invitationService.GetAsync(code);
            if (result.Success)
            {
                return this.Ok(result.Result);
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(result.Errors);
        }
        
        [HttpGet("delete")]
        public async Task<IActionResult> DeleteInvitation(string code)
        {
            var result = await this.invitationService.GetAsync(code);
            if (result.Success)
            {
                return this.Ok(result.Result);
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(result.Errors);
        }
    }
}