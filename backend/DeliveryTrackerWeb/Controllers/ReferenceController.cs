using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/reference")]
    public class ReferenceController : Controller
    {
        // reference/types
        // reference/{type}/create
        // reference/{type}/edit
        // reference/{type}/get
        // reference/{type}/delete
        
     
        [HttpGet("types")]
        public IActionResult GetGroupsList()
        {
            return this.Ok();
        }
        
        [HttpPost("{type}/create")]
        public IActionResult Create(string type)
        {
            return this.Ok();
        }
        
        [HttpPost("{type}/edit")]
        public IActionResult Edit(string type)
        {
            return this.Ok();
        }
        
        [HttpGet("{type}/get")]
        public IActionResult Get(string type)
        {
            return this.Ok();
        }
        
        [HttpPost("{type}/delete")]
        public IActionResult Delete(string type)
        {
            return this.Ok();
        }
    }
}