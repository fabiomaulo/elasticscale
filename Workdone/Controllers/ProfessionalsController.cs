using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Slider.WorkDone.Api.Controllers
{
    public class ProfessionalsController : ApiController
    {
        // GET: api/Professionals
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Professionals/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Professionals
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Professionals/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Professionals/5
        public void Delete(int id)
        {
        }
    }
}
