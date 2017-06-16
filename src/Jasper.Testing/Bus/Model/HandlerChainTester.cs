using System.Linq;
using Jasper.Bus.Model;
using Jasper.Testing.Bus.Runtime;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Model
{
    public class HandlerChainTester
    {
        [Fact]
        public void create_by_method()
        {
            var chain = HandlerChain.For<Target>(x => x.Go(null));
            chain.MessageType.ShouldBe(typeof(Message1));

            var methodCall = chain.Handlers.Single();
            methodCall.HandlerType.ShouldBe(typeof(Target));
            methodCall.Method.Name.ShouldBe(nameof(Target.Go));

        }

        [Fact]
        public void create_by_static_method()
        {
            var chain = HandlerChain.For<Target>(nameof(Target.GoStatic));

            chain.MessageType.ShouldBe(typeof(Message2));

            var methodCall = chain.Handlers.Single();
            methodCall.HandlerType.ShouldBe(typeof(Target));
            methodCall.Method.Name.ShouldBe(nameof(Target.GoStatic));
        }

        [Fact]
        public void default_number_of_max_attempts_is_1()
        {
            var chain = HandlerChain.For<Target>(nameof(Target.GoStatic));
            chain.MaximumAttempts.ShouldBe(1);
        }

        public class Target
        {
            public void Go(Message1 message)
            {

            }

            public static void GoStatic(Message2 message)
            {

            }


        }

    }
}