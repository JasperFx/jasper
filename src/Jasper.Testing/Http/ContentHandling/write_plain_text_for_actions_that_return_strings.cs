using System.Threading.Tasks;
using Alba;
using Xunit;

namespace Jasper.Testing.Http.ContentHandling
{
    public class write_plain_text_for_actions_that_return_strings : RegistryContext<HttpTestingApp>
    {
        public write_plain_text_for_actions_that_return_strings(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }

        [Fact]
        public Task write_as_text()
        {
            return scenario(_ =>
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
