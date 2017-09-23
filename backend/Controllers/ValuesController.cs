using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeliveryTracker.Caching;
using DeliveryTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryTracker.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {

        private readonly RolesCache rolesCache;
        public ValuesController(RolesCache rolesCache, AccountService accountService)
        {
            this.rolesCache = rolesCache;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {

            var role1 = this.rolesCache.Creator;
            var role2 = this.rolesCache.Manager;
            var role3 = this.rolesCache.Performer;
            return "value";
        }



        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
