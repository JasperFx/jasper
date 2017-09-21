using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class reading_model_synchronously_by_content_type
    {
        private ModelReader theReader;

        public reading_model_synchronously_by_content_type()
        {
            theReader = new ModelReader(new IMessageDeserializer[]
            {
                new FakeReader("blue"),
                new FakeReader("red"),
                new FakeReader("green"),
            });
        }

        [Fact]
        public void read_with_a_single_accepts_type()
        {
            var bytes = Encoding.UTF8.GetBytes("Chiefs");


            theReader.TryRead("blue", bytes, out object model)
                .ShouldBeTrue();

            model.As<ConnegMessage>().ContentType.ShouldBe("blue");
            model.As<ConnegMessage>().Name.ShouldBe("Chiefs");
        }


        [Fact]
        public void read_with_a_single_accepts_type_that_does_not_match()
        {
            var bytes = Encoding.UTF8.GetBytes("Broncos");


            theReader.TryRead("purple", bytes, out object model)
                .ShouldBeFalse();

            model.ShouldBeNull();
        }


    }

    internal class FakeReader : IMessageDeserializer
    {
        public FakeReader(string contentType)
        {
            ContentType = contentType;
        }

        public FakeReader(Type dotNetType, string contentType)
        {
            DotNetType = dotNetType;
            ContentType = contentType;
            MessageType = dotNetType.ToMessageAlias();
        }

        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            return new ConnegMessage
            {
                ContentType = ContentType,
                Name = Encoding.UTF8.GetString(data)
            };
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotSupportedException();
        }

    }

    public class ConnegMessage
    {
        public string ContentType { get; set; }
        public string Name { get; set; }
    }
}
