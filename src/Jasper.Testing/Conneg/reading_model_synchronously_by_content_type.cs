using System.IO;
using System.Text;
using System.Threading.Tasks;
using Jasper.Conneg;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class reading_model_synchronously_by_content_type
    {
        private ModelReader<ConnegMessage> theReader;

        public reading_model_synchronously_by_content_type()
        {
            theReader = new ModelReader<ConnegMessage>(new IMediaReader<ConnegMessage>[]
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


            theReader.TryRead("blue", bytes, out ConnegMessage model)
                .ShouldBeTrue();

            model.ContentType.ShouldBe("blue");
            model.Name.ShouldBe("Chiefs");
        }


        [Fact]
        public void read_with_a_single_accepts_type_that_does_not_match()
        {
            var bytes = Encoding.UTF8.GetBytes("Broncos");


            theReader.TryRead("purple", bytes, out ConnegMessage model)
                .ShouldBeFalse();

            model.ShouldBeNull();
        }


    }

    public class FakeReader : IMediaReader<ConnegMessage>
    {
        public FakeReader(string contentType)
        {
            ContentType = contentType;
        }

        public string ContentType { get; }

        public ConnegMessage Read(byte[] data)
        {
            return new ConnegMessage
            {
                ContentType = ContentType,
                Name = Encoding.UTF8.GetString(data)
            };
        }

        public Task<ConnegMessage> Read(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ConnegMessage
    {
        public string ContentType { get; set; }
        public string Name { get; set; }
    }
}
