using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Bus;
using Jasper.Conneg;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace IntegrationTests.Conneg
{
    public class using_accepted_types_with_request_reply
    {
        [Fact]
        public async Task use_custom_reader_writer()
        {
            Reply1Reader.WasUsed = false;
            Reply1Writer.WasUsed = false;

            var requestorRegistry = new JasperBusRegistry();
            requestorRegistry.SendMessage<Request1>().To("jasper://localhost:2456/incoming");
            var requestor = JasperRuntime.For(requestorRegistry);

            var replierRegistry = new JasperBusRegistry();
            replierRegistry.Channels.ListenForMessagesFrom("jasper://localhost:2456/incoming");
            var replier = JasperRuntime.For(replierRegistry);


            try
            {
                var reply = await requestor.Container.GetInstance<IServiceBus>()
                    .Request<Reply1>(new Request1 {One = 3, Two = 4});

                reply.Sum.ShouldBe(7);

                Reply1Reader.WasUsed.ShouldBeTrue();
                Reply1Writer.WasUsed.ShouldBeTrue();
            }
            finally
            {
                replier?.Dispose();
                requestor?.Dispose();
            }
        }
    }

    public class RequestReplyHandler
    {
        public Reply1 Handle(Request1 request)
        {
            return new Reply1
            {
                Sum = request.One + request.Two
            };
        }
    }

    public class Request1
    {
        public int One { get; set; }
        public int Two { get; set; }
    }


    public class Reply1Reader : IMediaReader
    {
        public string MessageType { get; } = typeof(Reply1).ToTypeAlias();
        public Type DotNetType { get; } = typeof(Reply1);
        public string ContentType { get; } = "text/plain";
        public object Read(byte[] data)
        {
            WasUsed = true;

            var text = Encoding.UTF8.GetString(data);
            return new Reply1
            {
                Sum = int.Parse(text)
            };
        }

        public static bool WasUsed { get; set; }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class Reply1Writer : IMediaWriter
    {
        public Type DotNetType { get; } = typeof(Reply1);
        public string ContentType { get; } = "text/plain";
        public byte[] Write(object model)
        {
            WasUsed = true;
            var text = model.As<Reply1>().Sum.ToString();
            return Encoding.UTF8.GetBytes(text);
        }

        public static bool WasUsed { get; set; }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class Reply1
    {
        public int Sum { get; set; }
    }
}
