using Microsoft.AspNetCore.Mvc;

namespace HttpTests.AspNetCoreIntegration
{
    public class ValuesController : Controller
    {
        [HttpGet("/values")] // GET api/values
        public string Get()
        {
            return "Hello from MVC Core";
        }

        // GET api/values/5
        [HttpGet("/values/{id}")]
        public string Get(int id)
        {
            return id.ToString();
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
