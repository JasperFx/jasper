using System;
using System.Text;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Jasper.Http.ContentHandling;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Http.Testing.ContentHandling
{
    public class using_custom_readers_and_writers : RegistryContext<HttpTestingApp>
    {
        public using_custom_readers_and_writers(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }

        [Fact]
        public Task discovers_and_opts_into_the_one_reader_and_writer()
        {
            return scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output");
                _.ContentShouldBe("Tamba Hali");
                _.ContentTypeShouldBe("text/special");
            });
        }
    }

    public class CustomReaderWriterEndpoint : IDisposable
    {
        public void Dispose()
        {
            // nothing, just wanna test the codegen
        }

        public SpecialOutput post_special_output(SpecialInput input)
        {
            return new SpecialOutput {Value = input.Name};
        }

        public XmlOutput post_xml_output(XmlInput input)
        {
            return new XmlOutput {Value = input.Name};
        }
    }

    public class XmlInput
    {
        public string Name { get; set; }
    }

    public class XmlOutput
    {
        public string Value { get; set; }
    }

    public class SpecialInput
    {
        public string Name { get; set; }
    }

    public class SpecialOutput
    {
        public string Value { get; set; }
    }

    public class SpecialReader : IRequestReader
    {
        public string MessageType { get; } = typeof(SpecialInput).ToMessageTypeName();
        public Type DotNetType { get; } = typeof(SpecialInput);
        public string ContentType { get; } = "text/special";

        public async Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            var text = await request.Body.ReadAllTextAsync();

            return new SpecialInput
            {
                Name = text
            }.As<T>();
        }

        public object ReadFromData(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data);

            return new SpecialInput
            {
                Name = text
            };
        }
    }

    public class SpecialWriter : IResponseWriter
    {
        public Type DotNetType { get; } = typeof(SpecialOutput);
        public string ContentType { get; } = "text/special";

        public Task WriteToStream(object model, HttpResponse response)
        {
            response.Headers["content-type"] = ContentType;
            return response.WriteAsync(model.As<SpecialOutput>().Value);
        }

        public byte[] Write(object model)
        {
            throw new NotSupportedException();
        }
    }
}
