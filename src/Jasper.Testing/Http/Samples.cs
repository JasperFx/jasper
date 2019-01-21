using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Jasper.Testing.Http
{
    public class Samples
    {

    }

    /*
     // SAMPLE: SampleHomeEndpoint
    public class HomeEndpoint
    {
        public string Get()
        {
            return "Hello, World from a Jasper application!";
        }
    }
    // ENDSAMPLE
    */


    public class Sum
    {
        public int Total { get; set; }
    }

    // SAMPLE: simple-json-endpoint
    public static class JsonEndpoint
    {
        // Responds to the route "GET: /add/:one/:two"
        public static Sum get_add_one_two(int one, int two)
        {
            return new Sum{Total = one + two};
        }
    }
    // ENDSAMPLE

    public class Invoice{}

    public interface IInvoiceService
    {
        Task Create(Invoice invoice);
    }

    // SAMPLE: simple-json-post
    public class PostEndpoint
    {
        // Responds to the route "POST: /invoice/create"
        // The first argument is assumed to be the body of the request
        // The IInvoiceService is assumed to come from the application's IoC container
        public Task post_invoice_create(Invoice invoice, IInvoiceService service)
        {
            return service.Create(invoice);
        }
    }
    // ENDSAMPLE


    [JasperIgnore]
    public class AspNetCoreInjectingEndpoint
    {
        // SAMPLE: injecting-httpcontext
        public Task get_color_name(string name, HttpContext context)
        {
            return context.Response.WriteAsync("The color is " + name);
        }
        // ENDSAMPLE

        // SAMPLE: injecting-request-and-response
        public Task get_stuff(HttpRequest request, HttpResponse response, ILogger<AspNetCoreInjectingEndpoint> logger)
        {
            logger.LogDebug(request.Path);

            return response.WriteAsync("here's some stuff");
        }
        // ENDSAMPLE
    }





}
