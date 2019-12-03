using System;
using Jasper.Messaging.Transports.Local;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class configuring_listeners_by_registry : IDisposable
    {
        private IHost _host;
        private JasperOptions theOptions;

        public configuring_listeners_by_registry()
        {
            _host = Host.CreateDefaultBuilder().UseJasper(x =>
            {
                x.Endpoints.ListenForMessagesFrom("local://one").Sequential();
                x.Endpoints.ListenForMessagesFrom("local://two").MaximumThreads(11);
                x.Endpoints.ListenForMessagesFrom("local://three").Durably();
                x.Endpoints.ListenForMessagesFrom("local://four").Durably().Lightweight();

            }).Build();

            theOptions = _host.Get<JasperOptions>();
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        [Fact]
        public void configure_sequential()
        {
            theOptions.Endpoints.GetTransport<LocalTransport>().QueueFor("one")
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);
        }

        [Fact]
        public void configure_max_parallelization()
        {
            theOptions.Endpoints.GetTransport<LocalTransport>().QueueFor("two")
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);
        }

        [Fact]
        public void configure_durable()
        {
            theOptions
                .Endpoints
                .ListenForMessagesFrom("local://three")
                .Durably();


            theOptions.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("three")
                .IsDurable
                .ShouldBeTrue();
        }

        [Fact]
        public void configure_not_durable()
        {
            theOptions.Endpoints.ListenForMessagesFrom("local://four");

            theOptions.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("four")
                .IsDurable
                .ShouldBeFalse();
        }
    }
}
