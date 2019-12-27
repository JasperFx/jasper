using System;
using Jasper.Attributes;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class using_error_handling_attributes : IntegrationContext
    {
        [Fact]
        public void use_maximum_attempts()
        {
            chainFor<Message1>().Retries.MaximumAttempts.ShouldBe(3);
        }


        public using_error_handling_attributes(DefaultApp @default) : base(@default)
        {
        }
    }

    public class ErrorCausingConsumer
    {
        [MaximumAttempts(3)]
        public void Handle(Message1 message)
        {
        }

        [RetryOn(typeof(DivideByZeroException))]
        public void Handle(Message2 message)
        {
        }

        [RequeueOn(typeof(NotImplementedException))]
        public void Handle(Message3 message)
        {
        }

        [MoveToErrorQueueOn(typeof(DataMisalignedException))]
        public void Handle(Message4 message)
        {
        }

        [RescheduleLater(typeof(DivideByZeroException), 5)]
        public void Handle(Message5 message)
        {
        }
    }
}
