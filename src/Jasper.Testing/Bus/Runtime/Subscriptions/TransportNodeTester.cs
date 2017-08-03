using System;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class TransportNodeTester
    {
        private readonly Uri _incomingChannel = "memory://incoming".ToUri();
        private readonly Uri _channel = "memory://channel".ToUri();
        private readonly Uri _controlChannel = "memory://control".ToUri();
        private readonly string _machineName = "Machine";
        private readonly JasperRegistry _registry;
        private readonly ServiceBusFeature _bus;
        private TransportNode _transportNode;

        public TransportNodeTester()
        {
            _registry = new JasperRegistry {ServiceName = "NodeName"};
            _registry.Channels.ListenForMessagesFrom(_incomingChannel);
            _registry.Channels.Add(_channel);
            _bus = _registry.Features.For<ServiceBusFeature>();
        }

        public TransportNode TransportNode {
            get
            {
                if(_transportNode == null)
                    _transportNode = new TransportNode(_bus.Channels, _machineName);
                return _transportNode;
            }
        }

        [Fact]
        public void sets_address_to_control_channel()
        {
            _registry.Channels.ListenForMessagesFrom(_controlChannel).UseAsControlChannel();

            _bus.Channels.ControlChannel.Uri.ShouldBe(_controlChannel);

            TransportNode.ControlChannel.ShouldBe(_controlChannel);
        }

        [Fact]
        public void sets_address_to_incoming_channel_if_no_control_channel()
        {
            TransportNode.ControlChannel.ShouldBe(_incomingChannel);
        }

        [Fact]
        public void sets_id()
        {
            TransportNode.Id.ShouldBe($"{_registry.ServiceName}@{_machineName}");
        }

        [Fact]
        public void sets_nodeName()
        {
            TransportNode.NodeName.ShouldBe(_registry.ServiceName);
        }

        [Fact]
        public void sets_machineName()
        {
            TransportNode.MachineName.ShouldBe(_machineName);
        }
    }
}
