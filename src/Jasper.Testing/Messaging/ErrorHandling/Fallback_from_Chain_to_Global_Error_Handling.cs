using System;
using System.Threading.Tasks;
using Jasper.Messaging.ErrorHandling;
using Jasper.Messaging.Tracking;
using Xunit;

namespace Jasper.Testing.Messaging.ErrorHandling
{
    public class Fallback_from_Chain_to_Global_Error_Handling : ErrorHandlingContext
    {
        public Fallback_from_Chain_to_Global_Error_Handling()
        {
            theOptions.Extensions.UseMessageTrackingTestingSupport();

            theOptions.Handlers.RetryOn<DivideByZeroException>();
            theOptions.Handlers.RequeueOn<DataMisalignedException>();
            theOptions.Handlers.MoveToDeadLetterQueueOn<DataMisalignedException>();

            theOptions.Handlers.ConfigureHandlerForMessage<ErrorCausingMessage>(chain =>
            {
                chain.MoveToDeadLetterQueueOn<DivideByZeroException>();
                chain.RetryOn<InvalidOperationException>();
                chain.Retries.MaximumAttempts = 3;
            });

        }

        [Fact]
        public async Task chain_specific_rules_do_not_apply()
        {
            throwOnAttempt<DataMisalignedException>(1);
            throwOnAttempt<DataMisalignedException>(2);

            var record = await afterProcessingIsComplete();

            record.ShouldHaveSucceededOnAttempt(3);
        }

        [Fact]
        public async Task chain_specific_rules_catch()
        {
            throwOnAttempt<DivideByZeroException>(1);

            await shouldMoveToErrorQueueOnAttempt(1);
        }

        [Fact]
        public async Task chain_specific_rules_win_again()
        {
            throwOnAttempt<InvalidOperationException>(1);

            await shouldSucceedOnAttempt(2);
        }
    }
}
