using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using DeliveryTracker.ViewModels;
using DeliveryTracker.Views;
using DeliveryTrackerWeb.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Route("api/view")]
    public class ViewController : Controller
    {
        // view/groups
        // view/{groupName}/views
        // view/{groupName}/{viewName}
        
        private readonly Lazy<IViewService<UserViewModel>> userViewService;
        private readonly Lazy<IViewService<TaskViewModel>> taskViewService;

        public ViewController(
            IServiceProvider serviceProvider)
        {
            this.userViewService = new Lazy<IViewService<UserViewModel>>(
                () => (IViewService<UserViewModel>)serviceProvider.GetService(typeof(IViewService<UserViewModel>)),
                LazyThreadSafetyMode.ExecutionAndPublication);
            this.taskViewService = new Lazy<IViewService<TaskViewModel>>(
                () => (IViewService<TaskViewModel>)serviceProvider.GetService(typeof(IViewService<TaskViewModel>)),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        [HttpGet("groups")]
        public IActionResult GetGroupsList()
        {
            return this.Ok(new[]
            {
                ViewGroups.TaskViewGroup,
                ViewGroups.UserViewGroup,
            });
        }

        [HttpGet("{viewGroup}/views")]
        public IActionResult GetViewsList()
        {
            throw new NotImplementedException();
        }
        
        [HttpGet("{viewGroup}/{viewName}")]
        public IActionResult GetViewResult(string viewGroup, string viewName)
        {
            var userCredentials = this.User.Claims.ToUserCredentials();
            var parameters = this.Request.Query
                .Select(p => new KeyValuePair<string, string[]>(p.Key, p.Value.ToArray()))
                .ToImmutableDictionary();
            
            switch (viewGroup)
            {
                case ViewGroups.UserViewGroup:
                    var userResult =  this.userViewService.Value.GetViewResult(userCredentials, viewName, parameters);
                    return userResult.Success
                        ? (IActionResult)this.Ok(userResult.Result)
                        : this.BadRequest();
                case ViewGroups.TaskViewGroup:
                    var taskResult =  this.taskViewService.Value.GetViewResult(userCredentials, viewName, parameters);
                    return taskResult.Success
                        ? (IActionResult)this.Ok(taskResult.Result)
                        : this.BadRequest();
                    
            }
           
            return this.BadRequest();
        }
    }
}