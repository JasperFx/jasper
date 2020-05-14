using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Jasper.Attributes;
using Jasper.Serialization;
using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Jasper.Http.Testing
{

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
            return new Sum {Total = one + two};
        }
    }
    // ENDSAMPLE

    public class Invoice
    {
    }

    public interface IInvoiceService
    {
        Task Create(Invoice invoice);
    }

    [JasperIgnore]

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


    [JasperIgnore]
    public class ResourceEndpoint
    {
        // SAMPLE: ResourceEndpoints
        public Task<Invoice> get_invoice_async(string invoiceId, IQuerySession session)
        {
            return session.LoadAsync<Invoice>(invoiceId);
        }

        public Task<Invoice> get_invoice_sync(string invoiceId, IQuerySession session)
        {
            return session.LoadAsync<Invoice>(invoiceId);
        }

        // ENDSAMPLE
    }

    public class Command
    {
    }

    public class ResourceModel
    {
    }

    [JasperIgnore]
    public class ResourceEndpoints
    {
        // SAMPLE: ResourceAndInputTypes
        // ResourceModel is the "resource" type
        // Command is the "input" type
        public ResourceModel post_resource(Command model)
        {
            return new ResourceModel();
        }

        // ResourceModel is the "resource" type
        // Command is the "input" type
        public Task<ResourceModel> post_resource_async(Command model)
        {
            return Task.FromResult(new ResourceModel());
        }
        // ENDSAMPLE


        /*
        // SAMPLE: crude-http
        // Responds to "PUT: /something"
        public Task put_something(HttpRequest request, HttpResponse response)
        {
            // Read the HttpRequest
            // Write out some kind of response
        }
        // ENDSAMPLE
        */

        // SAMPLE: resource-with-argument
        // ResourceModel is the resource type
        // "id" is a route argument
        public ResourceModel get_resource_id(string id)
        {
            return lookupById(id);
        }
        // ENDSAMPLE

        private ResourceModel lookupById(string id)
        {
            throw new NotImplementedException();
        }

        // SAMPLE: input-type-without-any-resource-type
        // Responds to "PUT: /invoice"
        // Invoice is the input type
        // There is no resource type
        public void put_invoice(Invoice invoice)
        {
            // process the new invoice
        }
        // ENDSAMPLE

        // SAMPLE: input-type-without-any-resource-type-but-a-status-code
        // Responds to "POST: /invoice"
        // Invoice is the input type
        // There is no resource type
        public int post_invoice(Invoice invoice)
        {
            // process the new invoice

            // 201: Created
            return 201;
        }

        // ENDSAMPLE
    }

    // SAMPLE: overwriting-the-JSON-serialization-with-StartUp
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Override the JSON serialization
            services.AddSingleton(new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                TypeNameHandling = TypeNameHandling.Objects
            });
        }
    }
    // ENDSAMPLE

    // SAMPLE: overwriting-the-JSON-serialization-with-JasperOptions
    public class MySpecialJsonUsingApp : JasperOptions
    {
        public MySpecialJsonUsingApp()
        {
            Services.For<JsonSerializerSettings>().Use(new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTime
            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: InvoiceXmlWriter
    public class InvoiceXmlWriter : MessageSerializerBase<Invoice>
    {
        public InvoiceXmlWriter() : base("application/xml")
        {
        }

        // We don't care in this case because this is only used inside the message bus
        // part of Jasper
        public override byte[] Write(Invoice model)
        {
            throw new NotSupportedException();
        }
    }
    // ENDSAMPLE

    // SAMPLE: InvoiceXmlReader
    public class InvoiceXmlReader : MessageDeserializerBase<Invoice>
    {
        public InvoiceXmlReader() : base("application/xml")
        {
        }

        public override Invoice ReadData(byte[] data)
        {
            throw new NotSupportedException();
        }

        protected override Task<Invoice> ReadData(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(Invoice));
            var model = (Invoice) serializer.Deserialize(stream);

            return Task.FromResult(model);
        }
    }
    // ENDSAMPLE

    // SAMPLE: registering-custom-readers-writers
    public class AppWithCustomSerializers : JasperOptions
    {
        public AppWithCustomSerializers()
        {
            // Register a custom writer
            Services.AddSingleton<IMessageSerializer, InvoiceXmlWriter>();

            // Register a custom reader
            Services.AddSingleton<IMessageDeserializer, InvoiceXmlReader>();
        }
    }

    // ENDSAMPLE
}
