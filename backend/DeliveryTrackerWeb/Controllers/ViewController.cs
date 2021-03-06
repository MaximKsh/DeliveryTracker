﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Views;
using DeliveryTrackerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTrackerWeb.Controllers
{
    [Authorize]
    [Route("api/view")]
    public class ViewController : Controller
    {
        // view/groups
        // view/{groupName}/views
        // view/{groupName}/digest
        // view/{groupName}/{viewName}

        private readonly IViewService viewService;

        public ViewController(IViewService viewService)
        {
            this.viewService = viewService;
        }

        [HttpGet("groups")]
        public IActionResult GetGroupsList()
        {
            var result = this.viewService.GetViewGroupsList();
            if (!result.Success)
            {
                return this.BadRequest(new ViewResponse(result.Errors));
            }
            
            return this.Ok(new ViewResponse
            {
                Groups = result.Result,
            });
        }

        [HttpGet("{viewGroup}/views")]
        public IActionResult GetViewsList(string viewGroup)
        {
            var groupResult = this.viewService.GetViewGroup(viewGroup);
            if (!groupResult.Success)
            {
                return this.BadRequest(new ViewResponse(groupResult.Errors));
            }

            var group = groupResult.Result;
            
            var result = group.GetViewsList();
            if (!result.Success)
            {
                return this.BadRequest(new ViewResponse(result.Errors));
            }
            
            return this.Ok(new ViewResponse
            {
                Views = result.Result,
            });
        }

        [HttpGet("{viewGroup}/digest")]
        public async Task<IActionResult> GetDigest(string viewGroup)
        {
            var groupResult = this.viewService.GetViewGroup(viewGroup);
            if (!groupResult.Success)
            {
                return this.BadRequest(new ViewResponse(groupResult.Errors));
            }

            var group = groupResult.Result;
            
            var result = await group.GetDigestAsync();
            if (!result.Success)
            {
                return this.BadRequest(new ViewResponse(result.Errors));
            }
            
            return this.Ok(new ViewResponse
            {
                Digest = result.Result,
            });
        }
            
        
        [HttpGet("{viewGroup}/{viewName}")]
        public async Task<IActionResult> GetViewResult(string viewGroup, string viewName)
        {
            var parameters = new ReadOnlyDictionary<string, IReadOnlyList<string>>(
                this.Request.Query.ToDictionary(k => k.Key, v => (IReadOnlyList<string>)v.Value.ToList().AsReadOnly()));
            
            var groupResult = this.viewService.GetViewGroup(viewGroup);
            if (!groupResult.Success)
            {
                return this.BadRequest(new ViewResponse(groupResult.Errors));
            }

            var group = groupResult.Result;
            
            var result = await group.ExecuteViewAsync(viewName, parameters);
            if (!result.Success)
            {
                return this.BadRequest(new ViewResponse(result.Errors));
            }

            var response = new ViewResponse
            {
                ViewResult = result.Result.Select(p => p.GetDictionary()),
            };
            return this.Ok(response);
        }
    }
}