using System;
using System.Threading.Tasks;
using Alba;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class default_error_handling : RegistryContext<HttpTestingApp>
    {
        [Fact]
        public Task get_a_500()
        {
            return scenario(_ =>
            {
                _.Get.Url("/exception");
                _.StatusCodeShouldBe(500);
                _.ContentShouldContain("DivideByZeroException");
            });
        }

        public default_error_handling(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }
    }

    public class ExceptionEndpoint
    {
        public string get_exception()
        {
            throw new DivideByZeroException();
        }
    }
}
