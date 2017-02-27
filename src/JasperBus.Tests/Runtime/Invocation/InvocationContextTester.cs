using System.Linq;
using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Invocation
{

    public class InvocationContextTester
    {

        [Fact]
        public void enqueue()
        {
            var messages = new InvocationContext(new Envelope{Message = new Message1()}, new HandlerChain());
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(m1);
            messages.EnqueueCascading(m2);

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }

        [Fact]
        public void ignores_nulls_just_fine()
        {
            var messages = new InvocationContext(new Envelope { Message = new Message1() }, new HandlerChain());
            messages.EnqueueCascading(null);

            messages.OutgoingMessages().Any().ShouldBeFalse();
        }

        [Fact]
        public void enqueue_an_oject_array()
        {
            var messages = new InvocationContext(new Envelope{Message = new Message1()}, new HandlerChain());
            var m1 = new Message1();
            var m2 = new Message2();

            messages.EnqueueCascading(new object[]{m1, m2});

            messages.OutgoingMessages().ShouldHaveTheSameElementsAs(m1, m2);
        }
    }


}