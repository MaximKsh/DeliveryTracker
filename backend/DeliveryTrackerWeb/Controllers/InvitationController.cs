using System;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Common;
using DeliveryTracker.Identification;
using DeliveryTracker.Instances;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
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
        public async Task<IActionResult> CreateInvitation([FromBody] InvitationRequest request)
        {
            var preliminaryUserData = request.User;
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request.User), request.User)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new InvitationResponse(validationResult.Error));
            }
            
            var result = await this.invitationService.CreateAsync(preliminaryUserData);
            if (result.Success)
            {
                return this.Ok(new InvitationResponse
                {
                    Invitation = result.Result
                });
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(new InvitationResponse(result.Errors));
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetInvitation(Guid id)
        {
            var result = await this.invitationService.GetAsync(id);
            if (result.Success)
            {
                return this.Ok(new InvitationResponse
                {
                    Invitation = result.Result
                });
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(new InvitationResponse(result.Errors));
        }
        
        [HttpGet("get")]
        public async Task<IActionResult> GetInvitation(string code)
        {
            var result = await this.invitationService.GetAsync(code);
            if (result.Success)
            {
                return this.Ok(new InvitationResponse
                {
                    Invitation = result.Result
                });
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(new InvitationResponse(result.Errors));
        }
        
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteInvitation([FromBody] InvitationRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new InvitationResponse(validationResult.Error));
            }

            var code = request.Code;
            var id = request.ID;
            ServiceResult result;
            if (id.HasValue)
            {
                result = await this.invitationService.DeleteAsync(id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(code))
            {
                result = await this.invitationService.DeleteAsync(code);
            }
            else
            {
                validationResult = new ParametersValidator()
                    .AddNotNullOrWhitespaceRule($"{nameof(request)}.{nameof(request.Code)}", request.Code)
                    .AddNotNullRule($"{nameof(request)}.{nameof(request.ID)}", request.ID)
                    .Validate();
                return this.BadRequest(new InvitationResponse(validationResult.Error));
            }
            
            if (result.Success)
            {
                return this.Ok();
            }

            if (result.Errors.Any(p => p.Code == ErrorCode.AccessDenied))
            {
                return this.Forbid();
            }

            return this.BadRequest(new InvitationResponse(result.Errors));
        }
    }
}