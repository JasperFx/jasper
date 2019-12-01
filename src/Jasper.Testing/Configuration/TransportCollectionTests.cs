using System;
using System.Linq;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Stub;
using Jasper.Messaging.Transports.Tcp;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class TransportCollectionTests
    {
        [Fact]
        public void add_transport()
        {
            var transport = Substitute.For<ITransport>();
            transport.Protocol.Returns("fake");

            var collection = new TransportCollection {transport};

            collection.ShouldContain(transport);


        }

        [Fact]
        public void stub_is_registered_by_default()
        {
            new TransportCollection()
                .OfType<StubTransport>()
                .Count().ShouldBe(1);
        }

        [Fact]
        public void tcp_is_registered_by_default()
        {
            new TransportCollection()
                .OfType<TcpTransport>()
                .Count().ShouldBe(1);
        }

        [Fact]
        public void local_is_registered_by_default()
        {
            new TransportCollection()
                .OfType<LocalTransport>()
                .Count().ShouldBe(1);
        }

        [Fact]
        public void retrieve_transport_by_scheme()
        {
            new TransportCollection()
                .TransportForScheme("tcp")
                .ShouldBeOfType<TcpTransport>();
        }

        [Fact]
        public void retrieve_transport_by_type()
        {
            new TransportCollection()
                .Get<LocalTransport>()
                .ShouldNotBeNull();
        }

        [Fact]
        public void create_transport_type_if_missing()
        {
            var collection = new TransportCollection();
            var transport = collection.Get<FakeTransport>();

            collection.Get<FakeTransport>()
                .ShouldBeSameAs(transport);
        }

        public class FakeTransport : ITransport
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public string Protocol { get; } = "fake";
            public Uri ReplyUri { get; }
            public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }

            public void Initialize(IMessagingRoot root)
            {
                throw new NotImplementedException();
            }

            public Endpoint ListenTo(Uri uri)
            {
                throw new NotImplementedException();
            }

            public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }

            public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }

            public void Initialize(IMessagingRoot root, ITransportRuntime runtime)
            {
                throw new NotImplementedException();
            }

            public ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
            {
                throw new NotImplementedException();
            }

            public void Subscribe(Uri uri, Subscription subscription)
            {
                throw new NotImplementedException();
            }
        }
    }
}
