using System.Linq;
using JasperBus.Model;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Model
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