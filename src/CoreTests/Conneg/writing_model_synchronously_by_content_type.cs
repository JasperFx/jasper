using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace CoreTests.Conneg
{
    public class writing_model_synchronously_by_content_type
    {
        public writing_model_synchronously_by_content_type()
        {
            theWriter = new ModelWriter(new IMessageSerializer[]
            {
                new FakeWriter("blue"),
                new FakeWriter("red"),
                new FakeWriter("green")
            });
        }

        private readonly ModelWriter theWriter;
        private readonly ConnegMessage theMessage = new ConnegMessage {Name = "Raiders"};

        [Fact]
        public void select_the_default()
        {
            theWriter.TryWrite("*/*", theMessage, out var contentType, out var data)
                .ShouldBeTrue();

            contentType.ShouldBe("blue");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:blue");
        }

        [Fact]
        public void select_the_default_with_multiple_missing()
        {
            theWriter.TryWrite("yellow,purple,*/*", theMessage, out var contentType, out var data)
                .ShouldBeTrue();

            contentType.ShouldBe("blue");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:blue");
        }

        [Fact]
        public void write_with_a_single_matching_accepts()
        {
            theWriter.TryWrite("red", theMessage, out var contentType, out var data)
                .ShouldBeTrue();

            contentType.ShouldBe("red");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:red");
        }

        [Fact]
        public void write_with_multiple_matching_accepts()
        {
            theWriter.TryWrite("red,green,blue", theMessage, out var contentType, out var data)
                .ShouldBeTrue();

            contentType.ShouldBe("red");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:red");
        }

        [Fact]
        public void write_with_multiple_missing_accepts()
        {
            theWriter.TryWrite("purple,yellow", theMessage, out var contentType, out var data)
                .ShouldBeFalse();
        }

        [Fact]
        public void write_with_single_missing_accepts()
        {
            theWriter.TryWrite("purple", theMessage, out var contentType, out var data)
                .ShouldBeFalse();
        }

        [Fact]
        public void write_with_some_matching_accepts()
        {
            theWriter.TryWrite("purple,green,blue", theMessage, out var contentType, out var data)
                .ShouldBeTrue();

            contentType.ShouldBe("green");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:green");
        }
    }


    internal class FakeWriter : IMessageSerializer
    {
        public FakeWriter(string contentType)
        {
            ContentType = contentType;
        }

        public Type DotNetType { get; }
        public string ContentType { get; }

        public byte[] Write(object model)
        {
            return Encoding.UTF8.GetBytes($"{model.As<ConnegMessage>().Name}:{ContentType}");
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotSupportedException();
        }

        public Task Write(ConnegMessage model, Stream stream)
        {
            throw new NotSupportedException();
        }
    }
}
