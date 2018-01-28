using System;
using System.Threading.Tasks;
using DeliveryTracker.References;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Authorize]
    [Route("api/reference")]
    public class ReferenceController : Controller
    {
        private readonly IReferenceFacade referenceFacade;
        
        public ReferenceController(
            IReferenceFacade referenceFacade)
        {
            this.referenceFacade = referenceFacade;
        }
        
        // reference/types
        // reference/{type}/create
        // reference/{type}/edit
        // reference/{type}/get
        // reference/{type}/delete
        
        
        [HttpGet("types")]
        public IActionResult GetTypesList()
        {
            return this.Ok(new ReferenceResponse { ReferencesList = this.referenceFacade.GetReferencesList() });
        }
        
        [HttpPost("{type}/create")]
        public async Task<IActionResult> Create(string type, [FromBody] ReferenceRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule(nameof(type),type)
                .AddNotNullRule("body", request)
                .AddNotNullRule(nameof(request.Entity), request.Entity)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new ReferenceResponse(validationResult.Error));
            }

            var result = await this.referenceFacade.CreateAsync(type, request.Entity);
            if (!result.Success)
            {
                return this.BadRequest(new ReferenceResponse(result.Errors));
            }

            return this.Ok(new ReferenceResponse {Entity = result.Result.GetDictionary()});
        }
        
        [HttpPost("{type}/edit")]
        public async Task<IActionResult> Edit(string type, [FromBody] ReferenceRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule(nameof(type),type)
                .AddNotNullRule("body", request)
                .AddNotNullRule(nameof(request.Entity), request.Entity)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new ReferenceResponse(validationResult.Error));
            }

            var result = await this.referenceFacade.EditAsync(type, request.Entity);
            if (!result.Success)
            {
                return this.BadRequest(new ReferenceResponse(result.Errors));
            }

            return this.Ok(new ReferenceResponse {Entity = result.Result.GetDictionary()});
        }
        
        [HttpGet("{type}/get")]
        public async Task<IActionResult> Get(string type, Guid id)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule(nameof(type),type)
                .AddNotEmptyGuidRule(nameof(id), id)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new ReferenceResponse(validationResult.Error));
            }

            var result = await this.referenceFacade.GetAsync(type, id);
            if (!result.Success)
            {
                return this.BadRequest(new ReferenceResponse(result.Errors));
            }

            return this.Ok(new ReferenceResponse {Entity = result.Result.GetDictionary()});
        }
        
        [HttpPost("{type}/delete")]
        public async Task<IActionResult> Delete(string type, [FromBody] ReferenceRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullOrWhitespaceRule(nameof(type), type)
                .AddNotNullRule(nameof(request), request)
                .AddNotEmptyGuidRule(nameof(request.Id), request.Id)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new ReferenceResponse(validationResult.Error));
            }

            var result = await this.referenceFacade.DeleteAsync(type, request.Id);
            if (!result.Success)
            {
                return this.BadRequest(new ReferenceResponse(result.Errors));
            }

            return this.Ok(new ReferenceResponse());
        }
    }
}