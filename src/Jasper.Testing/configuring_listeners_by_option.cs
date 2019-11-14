using System;
using System.Linq;
using Jasper.Testing.Messaging.Transports.Stub;
using Jasper.Util;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class configuring_listeners_by_registry : IDisposable
    {
        private IHost _host;
        private JasperOptions theOptions;

        public configuring_listeners_by_registry()
        {
            _host = Host.CreateDefaultBuilder().UseJasper(x =>
            {
                x.Transports.ListenForMessagesFrom("local://one").Sequential();
                x.Transports.ListenForMessagesFrom("local://two").MaximumParallelization(11);
                x.Transports.ListenForMessagesFrom("local://three").IsDurable();
                x.Transports.ListenForMessagesFrom("local://four").IsDurable().IsNotDurable();

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
            theOptions.ListenForMessagesFrom("local://one")
                .As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);
        }

        [Fact]
        public void configure_max_parallelization()
        {
            theOptions.ListenForMessagesFrom("local://two")
                .As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);
        }

        [Fact]
        public void configure_durable()
        {
            theOptions.ListenForMessagesFrom("local://three")
                .As<ListenerSettings>()
                .IsDurable
                .ShouldBeTrue();
        }

        [Fact]
        public void configure_not_durable()
        {
            theOptions.ListenForMessagesFrom("local://four")
                .As<ListenerSettings>()
                .IsDurable
                .ShouldBeFalse();
        }
    }

    public class configuring_listeners_by_option
    {
        [Fact]
        public void return_the_same_listener()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener1 = options.ListenForMessagesFrom(uri);
            listener1.ShouldNotBeNull();

            var listener2 = options.ListenForMessagesFrom(uri);

            listener1.ShouldBeSameAs(listener2);

            options.Listeners
                .Count(x => x.Uri == uri).ShouldBe(1);
        }


        [Fact]
        public void set_max_parallelization()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.ListenForMessagesFrom(uri);

            listener.MaximumParallelization(11).ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);

        }

        [Fact]
        public void set_sequential()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.ListenForMessagesFrom(uri);

            listener.Sequential().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);

        }

        [Fact]
        public void is_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.ListenForMessagesFrom(uri);

            listener.IsDurable().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .IsDurable
                .ShouldBeTrue();

        }

        [Fact]
        public void is_not_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.ListenForMessagesFrom(uri);

            listener.IsDurable().IsNotDurable().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .IsDurable
                .ShouldBeFalse();

        }



    }
}
