using System.Threading.Tasks;
using DeliveryTracker.Geopositioning;
using DeliveryTracker.Identification;
using DeliveryTracker.Validation;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/geopositioning")]
    public class GeopositioningController : Controller
    {
        #region fields

        private readonly IGeopositioningService geopositioningService;

        #endregion

        #region constructors

        public GeopositioningController(
            IGeopositioningService geopositioningService)
        {
            this.geopositioningService = geopositioningService;
        }

        #endregion

        #region actions

        [HttpPost("update")]
        [Authorize(AuthorizationPolicies.Performer)]
        public async Task<IActionResult> UpdateGeoposition([FromBody] GeopositioningRequest request)
        {
            var validationResult = new ParametersValidator()
                .AddNotNullRule(nameof(request), request)
                .Validate();
            if (!validationResult.Success)
            {
                return this.BadRequest(new InstanceResponse(validationResult.Error));
            }

            var result = await this.geopositioningService.UpdateGeopositionAsync(request.Geoposition);
            if (result.Success)
            {
                return this.Ok(new GeopositioningResponse());
            }

            return this.BadRequest(new GeopositioningResponse(result.Errors));
        }

        #endregion
    }
}