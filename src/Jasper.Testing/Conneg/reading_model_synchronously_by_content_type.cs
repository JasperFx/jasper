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
    public class reading_model_synchronously_by_content_type
    {
        private ModelReader theReader;

        public reading_model_synchronously_by_content_type()
        {
            theReader = new ModelReader(new IMediaReader[]
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

    public class FakeReader : IMediaReader
    {
        public FakeReader(string contentType)
        {
            ContentType = contentType;
        }

        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

        public object Read(byte[] data)
        {
            return new ConnegMessage
            {
                ContentType = ContentType,
                Name = Encoding.UTF8.GetString(data)
            };
        }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

    }

    public class ConnegMessage
    {
        public string ContentType { get; set; }
        public string Name { get; set; }
    }
}
