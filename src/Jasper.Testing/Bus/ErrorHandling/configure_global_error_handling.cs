using System;
using System.Linq;
using Jasper.Bus.ErrorHandling;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.ErrorHandling
{
    public class configure_global_error_handling : IntegrationContext
    {
        [Fact]
        public void apply_global_error_handling()
        {
            with(_ =>
            {
                _.ErrorHandling.OnException<TimeoutException>()
                    .Requeue();


            });

            // TODO -- Almost a worthless test
            Handlers.ErrorHandlers.Single()
                .ShouldBeOfType<ErrorHandler>()
                .Sources.Single()
                .ShouldBeOfType<ContinuationSource>();
        }
    }
}