using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/tasks")]
    public class TaskController : Controller
    {
        // tasks/create
        // tasks/get
        // tasks/edit
        // tasks/change_state
        // tasks/delete
        
        [HttpPost("create")]
        public IActionResult Create()
        {
            return this.Ok();
        }
        
        [HttpGet("get")]
        public IActionResult Get()
        {
            return this.Ok();
        }
        
        [HttpPost("edit")]
        public IActionResult Edit()
        {
            return this.Ok();
        }
        
        [HttpPost("change_state")]
        public IActionResult ChangeState()
        {
            return this.Ok();
        }
        
        [HttpPost("delete")]
        public IActionResult Delete()
        {
            return this.Ok();
        }
    }
}