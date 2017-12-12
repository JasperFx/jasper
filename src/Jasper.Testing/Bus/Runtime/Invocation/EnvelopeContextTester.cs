using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Util;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Invocation
{

    public class EnvelopeContextTester
    {

        [Fact]
        public void enqueue()
        {
            var messages = new EnvelopeContext(null, new Envelope{Message = new Message1()}, Substitute.For<IServiceBus>());
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(m1);
            messages.EnqueueCascading(m2);

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }

        [Fact]
        public void ignores_nulls_just_fine()
        {
            var messages = new EnvelopeContext(null, new Envelope { Message = new Message1() }, Substitute.For<IServiceBus>());
            messages.EnqueueCascading(null);

            messages.OutgoingMessages().Any().ShouldBeFalse();
        }

        [Fact]
        public void enqueue_an_oject_array()
        {
            var messages = new EnvelopeContext(null, new Envelope{Message = new Message1()}, Substitute.For<IServiceBus>());
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(new object[]{m1, m2});

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }
    }




}
