using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Jasper.Testing.Samples
{
    public class SomeEndpoints
    {
        // Responds to GET: /something
        public string get_something()
        {
            return "Something";
        }

        // Responds to POST: /something
        public string post_something()
        {
            return "You posted something";
        }
    }

    public class AsyncEndpoints
    {
        public Task get_greetings(HttpResponse response)
        {
            response.ContentType = "text/plain";
            return response.WriteAsync("Greetings and salutations!");
        }
    }
}
