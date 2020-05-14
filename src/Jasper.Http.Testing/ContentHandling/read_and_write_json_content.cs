using System.Threading.Tasks;
using Alba;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.ContentHandling
{
    public class read_and_write_json_content : RegistryContext<HttpTestingApp>
    {
        public read_and_write_json_content(RegistryFixture<HttpTestingApp> fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task read_and_write()
        {
            var numbers = new SomeNumbers
            {
                X = 3, Y = 5
            };

            var result = await scenario(_ =>
            {
                _.Post.Json(numbers).ToUrl("/sum");
                _.StatusCodeShouldBeOk();
                _.ContentTypeShouldBe("application/json");
            });

            var sum = result.ResponseBody.ReadAsJson<SumValue>();

            sum.Sum.ShouldBe(8);
        }
    }

    public class SomeNumbers
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class SumValue
    {
        public int Sum { get; set; }
    }

    // SAMPLE: NumbersEndpoint
    public class NumbersEndpoint
    {
        public static SumValue post_sum(SomeNumbers input)
        {
            return new SumValue {Sum = input.X + input.Y};
        }
    }

    // ENDSAMPLE
}
