using Jasper.Messaging.Transports.Local;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class configuring_listeners_by_option
    {

        [Fact]
        public void set_max_parallelization()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options
                .Transports
                .ListenForMessagesFrom(uri)
                .MaximumThreads(11);

            options.Transports.Get<LocalTransport>()
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
                .Transports
                .ListenForMessagesFrom(uri)
                .Sequential();

            var executionOptions = options.Transports.Get<LocalTransport>()
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

            options.Transports.ListenForMessagesFrom(uri).Durably();

            options.Transports.Get<LocalTransport>()
                .QueueFor("one")
                .IsDurable
                .ShouldBeTrue();

        }

        [Fact]
        public void is_not_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            options.Transports.ListenForMessagesFrom(uri).Lightweight();

            options.Transports.Get<LocalTransport>()
                .QueueFor("one")
                .IsDurable
                .ShouldBeFalse();


        }



    }
}
