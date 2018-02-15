using System;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Messaging.Lightweight;
using Jasper.Testing.Messaging.Runtime;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Transport
{
    public class http_transport_end_to_end : SendingContext
    {
        public http_transport_end_to_end()
        {
            StartTheReceiver(_ =>
            {
                _.Http.Transport.EnableListening(true);

                _.Http
                    .UseUrls("http://localhost:5002")
                    .UseKestrel();

                _.Handlers.IncludeType<MessageConsumer>();
                _.Handlers.IncludeType<RequestReplyHandler>();
            });


        }

        [Fact]
        public void http_transport_is_enabled_and_registered()
        {


            var settings = theReceiver.Get<HttpTransportSettings>();
            settings.EnableMessageTransport.ShouldBeTrue();

            theReceiver.Get<IChannelGraph>().ValidTransports.ShouldContain("http");
        }

        [Fact]
        public void send_messages_end_to_end_lightweight()
        {
            StartTheSender(_ =>
            {
                _.Publish.Message<Message1>().To("http://localhost:5002/messages");
                _.Publish.Message<Message2>().To("http://localhost:5002/messages");
            });

            var waiter1 = theTracker.WaitFor<Message1>();
            var waiter2 = theTracker.WaitFor<Message2>();

            var message1 = new Message1 {Id = Guid.NewGuid()};
            var message2 = new Message2 {Id = Guid.NewGuid()};

            theSender.Messaging.Send(message1);
            theSender.Messaging.Send(message2);

            waiter1.Wait(10.Seconds());
            waiter2.Wait(10.Seconds());

            waiter1.Result.Message.As<Message1>()
                .Id.ShouldBe(message1.Id);
        }

    }

    public abstract class SendingContext : IDisposable
    {
        private readonly JasperHttpRegistry senderRegistry = new JasperHttpRegistry();
        private readonly JasperHttpRegistry receiverRegistry = new JasperHttpRegistry();
        protected JasperRuntime theSender;
        protected JasperRuntime theReceiver;
        protected MessageTracker theTracker;

        public SendingContext()
        {
            theTracker = new MessageTracker();
            receiverRegistry.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>();

            receiverRegistry.Services.For<MessageTracker>().Use(theTracker);

        }

        protected void StartTheSender(Action<JasperRegistry> configure)
        {
            configure(senderRegistry);
            theSender = JasperRuntime.For(senderRegistry);
        }

        protected void RestartTheSender()
        {
            theSender = JasperRuntime.For(senderRegistry);
        }

        protected void StopTheSender()
        {
            theSender?.Dispose();
        }

        protected void StartTheReceiver(Action<JasperHttpRegistry> configure)
        {
            configure(receiverRegistry);
            theReceiver = JasperRuntime.For(receiverRegistry);
        }

        protected void RestartTheReceiver()
        {
            theSender = JasperRuntime.For(receiverRegistry);
        }

        protected void StopTheReceiver()
        {
            theSender?.Dispose();
        }


        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
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
