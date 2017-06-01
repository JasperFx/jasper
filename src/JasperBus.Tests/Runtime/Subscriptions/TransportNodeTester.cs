﻿using System;
using JasperBus.Runtime;
using JasperBus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Subscriptions
{
    public class TransportNodeTester
    {
        private readonly Uri _incomingChannel = "memory://incoming".ToUri();
        private readonly Uri _channel = "memory://channel".ToUri();
        private readonly Uri _controlChannel = "memory://control".ToUri();
        private readonly string _machineName = "Machine";
        private readonly JasperBusRegistry _registry;
        private readonly ServiceBusFeature _bus;
        private TransportNode _transportNode;

        public TransportNodeTester()
        {
            _registry = new JasperBusRegistry {NodeName = "NodeName"};
            _registry.ListenForMessagesFrom(_incomingChannel);
            _registry.Channel(_channel);
            _bus = _registry.Feature<ServiceBusFeature>();
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
            _registry.ListenForMessagesFrom(_controlChannel).UseAsControlChannel();

            _bus.Channels.ControlChannel.Uri.ShouldBe(_controlChannel);

            TransportNode.Address.ShouldBe(_controlChannel);
        }

        [Fact]
        public void sets_address_to_incoming_channel_if_no_control_channel()
        {
            TransportNode.Address.ShouldBe(_incomingChannel);
        }

        [Fact]
        public void sets_id()
        {
            TransportNode.Id.ShouldBe($"{_registry.NodeName}@{_machineName}");
        }

        [Fact]
        public void sets_nodeName()
        {
            TransportNode.NodeName.ShouldBe(_registry.NodeName);
        }

        [Fact]
        public void sets_machineName()
        {
            TransportNode.MachineName.ShouldBe(_machineName);
        }
    }
}
