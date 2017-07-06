using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class writing_model_synchronously_by_content_type
    {
        private ModelWriter<ConnegMessage> theWriter;
        private ConnegMessage theMessage = new ConnegMessage{Name = "Raiders"};

        public writing_model_synchronously_by_content_type()
        {
            theWriter = new ModelWriter<ConnegMessage>(new IMediaWriter[]
            {
                new FakeWriter("blue"),
                new FakeWriter("red"),
                new FakeWriter("green"),
            });
        }

        [Fact]
        public void write_with_a_single_matching_accepts()
        {
            theWriter.TryWrite("red", theMessage, out string contentType, out byte[] data)
                .ShouldBeTrue();

            contentType.ShouldBe("red");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:red");
        }

        [Fact]
        public void write_with_multiple_matching_accepts()
        {
            theWriter.TryWrite("red,green,blue", theMessage, out string contentType, out byte[] data)
                .ShouldBeTrue();

            contentType.ShouldBe("red");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:red");
        }

        [Fact]
        public void write_with_some_matching_accepts()
        {
            theWriter.TryWrite("purple,green,blue", theMessage, out string contentType, out byte[] data)
                .ShouldBeTrue();

            contentType.ShouldBe("green");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:green");
        }

        [Fact]
        public void write_with_single_missing_accepts()
        {
            theWriter.TryWrite("purple", theMessage, out string contentType, out byte[] data)
                .ShouldBeFalse();
        }

        [Fact]
        public void write_with_multiple_missing_accepts()
        {
            theWriter.TryWrite("purple,yellow", theMessage, out string contentType, out byte[] data)
                .ShouldBeFalse();
        }

        [Fact]
        public void select_the_default()
        {
            theWriter.TryWrite("*/*", theMessage, out string contentType, out byte[] data)
                .ShouldBeTrue();

            contentType.ShouldBe("blue");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:blue");
        }

        [Fact]
        public void select_the_default_with_multiple_missing()
        {
            theWriter.TryWrite("yellow,purple,*/*", theMessage, out string contentType, out byte[] data)
                .ShouldBeTrue();

            contentType.ShouldBe("blue");
            Encoding.UTF8.GetString(data).ShouldBe("Raiders:blue");
        }
    }


    public class FakeWriter : IMediaWriter
    {
        public Type DotNetType { get; }
        public string ContentType { get; }

        public FakeWriter(string contentType)
        {
            ContentType = contentType;
        }

        public byte[] Write(object model)
        {
            return Encoding.UTF8.GetBytes($"{model.As<ConnegMessage>().Name}:{ContentType}");
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task Write(ConnegMessage model, Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}
