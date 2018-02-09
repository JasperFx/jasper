using System;
using System.Threading.Tasks;
using Alba;
using JasperHttpTesting;
using Xunit;

namespace Jasper.Http.Testing.ContentHandling
{
    public class default_error_handling
    {
        [Fact]
        public Task get_a_500()
        {
            return HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/exception");
                _.StatusCodeShouldBe(500);
                _.ContentShouldContain("DivideByZeroException");
            });
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
