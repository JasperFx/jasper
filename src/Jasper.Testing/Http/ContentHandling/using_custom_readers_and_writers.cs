using System;
using System.Text;
using System.Threading.Tasks;
using Alba;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class using_custom_readers_and_writers
    {
        [Fact]
        public Task discovers_and_opts_into_the_one_reader_and_writer()
        {
            return HttpTesting.Scenario(_ =>
            {
                _.Post.Text("Tamba Hali").ToUrl("/special/output");
                _.ContentShouldBe("Tamba Hali");
                _.ContentTypeShouldBe("text/special");
            });
        }
    }

    public class CustomReaderWriterEndpoint
    {
        public SpecialOutput post_special_output(SpecialInput input)
        {
            return new SpecialOutput{Value = input.Name};
        }
    }

    public class SpecialInput
    {
        public string Name { get; set; }
    }

    public class SpecialOutput
    {
        public string Value { get; set; }
    }

    public class SpecialReader : IMediaReader
    {
        public string MessageType { get; } = typeof(SpecialInput).ToTypeAlias();
        public Type DotNetType { get; } = typeof(SpecialInput);
        public string ContentType { get; } = "text/special";
        public object ReadFromData(byte[] data)
        {
            var text = Encoding.UTF8.GetString(data);

            return new SpecialInput
            {
                Name = text
            };
        }

        public async Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            var text = await request.Body.ReadAllTextAsync();

            return new SpecialInput
            {
                Name = text
            }.As<T>();
        }
    }

    public class SpecialWriter : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(SpecialOutput);
        public string ContentType { get; } = "text/special";
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            response.Headers["content-type"] = ContentType;
            return response.WriteAsync(model.As<SpecialOutput>().Value);
        }
    }
}
