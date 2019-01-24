using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class using_error_handling_attributes : IntegrationContext
    {
        [Fact]
        public void use_maximum_attempts()
        {
            withAllDefaults();
            chainFor<Message1>().Retries.MaximumAttempts.ShouldBe(3);
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
