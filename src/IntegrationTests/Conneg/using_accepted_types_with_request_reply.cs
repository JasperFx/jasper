using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Conneg;
using Jasper.Testing.Bus;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
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

            var requestorRegistry = new JasperRegistry();
            requestorRegistry.Publish.Message<Request1>().To("tcp://localhost:2457/incoming");
            requestorRegistry.Transports.ListenForMessagesFrom("tcp://localhost:1555");
            var requestor = JasperRuntime.For(requestorRegistry);

            var replierRegistry = new JasperRegistry();
            replierRegistry.Transports.ListenForMessagesFrom("tcp://localhost:2457/incoming");
            var replier = JasperRuntime.For(replierRegistry);


            try
            {
                var reply = await requestor.Get<IServiceBus>()
                    .Request<Reply1>(new Request1 {One = 3, Two = 4}, 60.Seconds());

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
        public Reply1 Handle(Request1 request, Envelope envelope)
        {
            return new Reply1
            {
                Sum = request.One + request.Two,
            };
        }
    }

    public class Request1
    {
        public int One { get; set; }
        public int Two { get; set; }
    }


    public class Reply1Reader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Reply1).ToMessageAlias();
        public Type DotNetType { get; } = typeof(Reply1);
        public string ContentType { get; } = "text/plain";
        public object ReadFromData(byte[] data)
        {
            WasUsed = true;

            var text = Encoding.UTF8.GetString(data);
            return new Reply1
            {
                Sum = int.Parse(text)
            };
        }

        public static bool WasUsed { get; set; }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotSupportedException();
        }
    }

    public class Reply1Writer : IMessageSerializer
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

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotSupportedException();
        }
    }

    public class Reply1
    {
        public int Sum { get; set; }
    }
}
