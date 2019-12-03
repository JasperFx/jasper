using Jasper.Configuration;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Local;
using Jasper.Util;
using LamarCodeGeneration.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public static class EndpointsExtensions
    {

        public static T GetTransport<T>(this IEndpoints endpoints) where T : ITransport, new()
        {
            return endpoints.As<TransportCollection>().Get<T>();
        }
    }

    public class configuring_listeners_by_option
    {

        [Fact]
        public void set_max_parallelization()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options
                .Endpoints
                .ListenForMessagesFrom(uri)
                .MaximumThreads(11);

            options.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("one")
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);

        }

        [Fact]
        public void set_sequential()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options
                .Endpoints
                .ListenForMessagesFrom(uri)
                .Sequential();

            var executionOptions = options.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("one")
                .ExecutionOptions;

            executionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);

            executionOptions.EnsureOrdered.ShouldBeTrue();

        }

        [Fact]
        public void is_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options.Endpoints.ListenForMessagesFrom(uri).Durably();

            options.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("one")
                .IsDurable
                .ShouldBeTrue();

        }

        [Fact]
        public void is_not_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options.Endpoints.ListenForMessagesFrom(uri).Lightweight();

            options.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("one")
                .IsDurable
                .ShouldBeFalse();


        }

        [Fact]
        public void configure_execution_options()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options.Endpoints.ListenForMessagesFrom(uri)
                .ConfigureExecution(o => o.BoundedCapacity = 13);

            options.Endpoints.GetTransport<LocalTransport>()
                .QueueFor("one")
                .ExecutionOptions
                .BoundedCapacity
                .ShouldBe(13);
        }



    }
}
