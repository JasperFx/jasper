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
     #region sample_SampleHomeEndpoint
    public class HomeEndpoint
    {
        public string Get()
        {
            return "Hello, World from a Jasper application!";
        }
    }
    #endregion
    */


    public class Sum
    {
        public int Total { get; set; }
    }

    #region sample_simple_json_endpoint
    public static class JsonEndpoint
    {
        // Responds to the route "GET: /add/:one/:two"
        public static Sum get_add_one_two(int one, int two)
        {
            return new() {Total = one + two};
        }
    }
    #endregion

    public class Invoice
    {
    }

    public interface IInvoiceService
    {
        Task Create(Invoice invoice);
    }

    [JasperIgnore]

    #region sample_simple_json_post
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
    #endregion


    [JasperIgnore]
    public class AspNetCoreInjectingEndpoint
    {
        #region sample_injecting_httpcontext
        public Task get_color_name(string name, HttpContext context)
        {
            return context.Response.WriteAsync("The color is " + name);
        }
        #endregion

        #region sample_injecting_request_and_response
        public Task get_stuff(HttpRequest request, HttpResponse response, ILogger<AspNetCoreInjectingEndpoint> logger)
        {
            logger.LogDebug(request.Path);

            return response.WriteAsync("here's some stuff");
        }

        #endregion
    }


    [JasperIgnore]
    public class ResourceEndpoint
    {
        #region sample_ResourceEndpoints
        public Task<Invoice> get_invoice_async(string invoiceId, IQuerySession session)
        {
            return session.LoadAsync<Invoice>(invoiceId);
        }

        public Task<Invoice> get_invoice_sync(string invoiceId, IQuerySession session)
        {
            return session.LoadAsync<Invoice>(invoiceId);
        }

        #endregion
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
        #region sample_ResourceAndInputTypes
        // ResourceModel is the "resource" type
        // Command is the "input" type
        public ResourceModel post_resource(Command model)
        {
            return new();
        }

        // ResourceModel is the "resource" type
        // Command is the "input" type
        public Task<ResourceModel> post_resource_async(Command model)
        {
            return Task.FromResult(new ResourceModel());
        }
        #endregion


        /*
        #region sample_crude_http
        // Responds to "PUT: /something"
        public Task put_something(HttpRequest request, HttpResponse response)
        {
            // Read the HttpRequest
            // Write out some kind of response
        }
        #endregion
        */

        #region sample_resource_with_argument
        // ResourceModel is the resource type
        // "id" is a route argument
        public ResourceModel get_resource_id(string id)
        {
            return lookupById(id);
        }
        #endregion

        private ResourceModel lookupById(string id)
        {
            throw new NotImplementedException();
        }

        #region sample_input_type_without_any_resource_type
        // Responds to "PUT: /invoice"
        // Invoice is the input type
        // There is no resource type
        public void put_invoice(Invoice invoice)
        {
            // process the new invoice
        }
        #endregion

        #region sample_input_type_without_any_resource_type_but_a_status_code
        // Responds to "POST: /invoice"
        // Invoice is the input type
        // There is no resource type
        public int post_invoice(Invoice invoice)
        {
            // process the new invoice

            // 201: Created
            return 201;
        }

        #endregion
    }

    #region sample_overwriting_the_JSON_serialization_with_StartUp
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
    #endregion

    #region sample_overwriting_the_JSON_serialization_with_JasperOptions
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
    #endregion


    #region sample_InvoiceXmlWriter
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
    #endregion

    #region sample_InvoiceXmlReader
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
    #endregion

    #region sample_registering_custom_readers_writers
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

    #endregion
}
