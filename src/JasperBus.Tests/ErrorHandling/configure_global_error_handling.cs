using System;
using System.Linq;
using JasperBus.ErrorHandling;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.ErrorHandling
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

            Graph.ErrorHandlers.Single()
                .ShouldBeOfType<ErrorHandler>()
                .Sources.Single()
                .ShouldBeOfType<RequeueContinuation>();
        }
    }
}