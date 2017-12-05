using System.Threading.Tasks;
using Jasper.Http;
using JasperHttpTesting;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class write_plain_text_for_actions_that_return_strings
    {
        [Fact]
        public Task write_as_text()
        {
            return HttpTesting.Scenario(_ =>
            {
                _.Get.Url("/string");
                _.ContentShouldBe("some string");
                _.ContentTypeShouldBe("text/plain");
                _.Header("content-length").SingleValueShouldEqual("11");
            });
        }
    }

    public class StringEndpoint
    {
        public string get_string()
        {
            return "some string";
        }
    }
}
