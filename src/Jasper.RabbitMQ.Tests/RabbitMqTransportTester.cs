using System;
using System.ServiceModel.Channels;
using Jasper.RabbitMQ.Internal;
using NSubstitute;
using RabbitMQ.Client;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqTransportTester
    {
        private readonly RabbitMqTransport theTransport = new RabbitMqTransport();
        private readonly IModel theChannel = Substitute.For<IModel>();


        [Fact]
        public void automatic_recovery_is_try_by_default()
        {
            theTransport.ConnectionFactory.AutomaticRecoveryEnabled.ShouldBeTrue();
        }

        [Fact]
        public void auto_provision_is_false_by_default()
        {
            theTransport.AutoProvision.ShouldBeFalse();
        }

        [Fact]
        public void initialize_with_no_auto_provision_or_auto_purge_with_queue_only()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void initialize_with_no_auto_provision_or_auto_purge_with_queue_and_exchange_only()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void initialize_with_no_auto_provision_but_auto_purge_on_endpoint_only()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void initialize_with_no_auto_provision_but_global_auto_purge()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void initialize_with_auto_provision_and_global_auto_purge()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void initialize_with_auto_provision_and_local_auto_purge()
        {
            throw new NotImplementedException();
        }
    }
}
