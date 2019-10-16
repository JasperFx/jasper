using System;
using System.Linq;
using Baseline;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.WorkerQueues
{
    public class WorkerGraphTests
    {
        public WorkerGraphTests()
        {
            theExpression = theWorkers.As<IWorkersExpression>();
        }

        private readonly WorkersGraph theWorkers = new WorkersGraph();
        private readonly IWorkersExpression theExpression;

        private void afterCompilingAgainstTheMessageTypes()
        {
            theWorkers.Compile(new[]
            {
                typeof(M1),
                typeof(M2),
                typeof(M3),
                typeof(M4),
                typeof(M5),
                typeof(BooM6),
                typeof(BooM7),
                typeof(M8)
            });
        }

        [Theory]
        [InlineData(typeof(DurableDefault), "loopback://default/durable")]
        [InlineData(typeof(NamedDurable), "loopback://important/durable")]
        [InlineData(typeof(NotDurableDefault), "loopback://default")]
        [InlineData(typeof(NotDurableNamed), "loopback://low")]
        public void select_the_loopback_uri_for_message_type(Type messageType, string uriString)
        {
            theWorkers.LoopbackUriFor(messageType).ShouldBe(uriString.ToUri());
        }

        [Fact]
        public void attribute_wins_over_configuration()
        {
            theExpression.Worker("foo").HandleMessages(x => x.Name.StartsWith("M"));

            afterCompilingAgainstTheMessageTypes();

            theWorkers.WorkerFor(typeof(M2)).ShouldBe("important");
            theWorkers.WorkerFor(typeof(M3)).ShouldBe("fast");

            theWorkers.WorkerFor(typeof(M2).ToMessageTypeName()).ShouldBe("important");
            theWorkers.WorkerFor(typeof(M3).ToMessageTypeName()).ShouldBe("fast");
        }

        [Fact]
        public void can_set_durable_and_max_parallelization_in_attribute()
        {
            afterCompilingAgainstTheMessageTypes();

            theWorkers["fast"].Parallelization.ShouldBe(3);
            theWorkers["fast"].IsDurable.ShouldBeTrue();
        }

        [Fact]
        public void has_the_default_queues_regardless()
        {
            afterCompilingAgainstTheMessageTypes();

            theWorkers.AllWorkers.Any(x => x.Name == TransportConstants.Default).ShouldBeTrue();
            theWorkers.AllWorkers.Any(x => x.Name == TransportConstants.Retries).ShouldBeTrue();
            theWorkers.AllWorkers.Any(x => x.Name == TransportConstants.Replies).ShouldBeTrue();
        }

        [Fact]
        public void set_durable_in_fluent_interface()
        {
            theExpression.Worker("foo").IsDurable();

            afterCompilingAgainstTheMessageTypes();

            theWorkers["foo"].IsDurable.ShouldBeTrue();
        }

        [Fact]
        public void set_maximum_parallelization()
        {
            theExpression.Worker("foo").MaximumParallelization(13);

            theWorkers["foo"].Parallelization.ShouldBe(13);
        }


        [Fact]
        public void set_the_routing_rules()
        {
            theExpression.Worker("foo").HandlesMessage<M4>();
            theExpression.Worker("bar").HandleMessages(t => t.Name.StartsWith("Boo"));

            afterCompilingAgainstTheMessageTypes();

            theWorkers.WorkerFor(typeof(BooM6)).ShouldBe("bar");
            theWorkers.WorkerFor(typeof(BooM7)).ShouldBe("bar");
            theWorkers.WorkerFor(typeof(M4)).ShouldBe("foo");

            theWorkers.WorkerFor(typeof(BooM6).ToMessageTypeName()).ShouldBe("bar");
            theWorkers.WorkerFor(typeof(BooM7).ToMessageTypeName()).ShouldBe("bar");
            theWorkers.WorkerFor(typeof(M4).ToMessageTypeName()).ShouldBe("foo");
        }

        [Fact]
        public void set_the_worker_with_attribute()
        {
            afterCompilingAgainstTheMessageTypes();

            theWorkers.WorkerFor(typeof(M2)).ShouldBe("important");
            theWorkers.WorkerFor(typeof(M3)).ShouldBe("fast");

            theWorkers.WorkerFor(typeof(M2).ToMessageTypeName()).ShouldBe("important");
            theWorkers.WorkerFor(typeof(M3).ToMessageTypeName()).ShouldBe("fast");
        }

        [Fact]
        public void should_be_durable_from_fi()
        {
            theExpression.Worker("foo").HandlesMessage<M1>().IsDurable();

            afterCompilingAgainstTheMessageTypes();
            theWorkers.ShouldBeDurable(typeof(M1)).ShouldBeTrue();


            theWorkers.ShouldBeDurable(typeof(M3)).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M4)).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M5)).ShouldBeFalse();

            theWorkers.ShouldBeDurable(typeof(M1).ToMessageTypeName()).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M3).ToMessageTypeName()).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M4).ToMessageTypeName()).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M5).ToMessageTypeName()).ShouldBeFalse();
        }

        [Fact]
        public void should_be_durable_with_attribute()
        {
            afterCompilingAgainstTheMessageTypes();
            theWorkers.ShouldBeDurable(typeof(M3)).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M4)).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M5)).ShouldBeFalse();

            theWorkers.ShouldBeDurable(typeof(M3).ToMessageTypeName()).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M4).ToMessageTypeName()).ShouldBeTrue();
            theWorkers.ShouldBeDurable(typeof(M5).ToMessageTypeName()).ShouldBeFalse();
        }

        [Fact]
        public void use_default_queue_if_none_is_known()
        {
            afterCompilingAgainstTheMessageTypes();

            theWorkers.WorkerFor(GetType()).ShouldBe(TransportConstants.Default);
        }

        [Fact]
        public void workers_are_not_durable_by_default()
        {
            theExpression.Worker("foo").HandleMessages(x => x.Name.StartsWith("M"));

            afterCompilingAgainstTheMessageTypes();

            theWorkers["foo"].IsDurable.ShouldBeFalse();
        }
    }

    public class NotDurableDefault
    {
    }

    [Worker("low")]
    public class NotDurableNamed
    {
    }

    [Durable]
    public class DurableDefault
    {
    }

    [Durable]
    [Worker("important")]
    public class NamedDurable
    {
    }

    public class M1
    {
    }


    [Worker("important")]
    public class M2
    {
    }

    [Worker("fast", IsDurable = true, MaximumParallelization = 3)]
    public class M3
    {
    }

    [Durable]
    public class M4
    {
    }

    public class M5
    {
    }

    public class BooM6
    {
    }

    public class BooM7
    {
    }

    [MessageIdentity("JamesBond")]
    public class M8
    {
    }
}
