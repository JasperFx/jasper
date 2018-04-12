using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Jasper.Testing
{
    public class temp_debugging
    {
        [Fact]
        public async Task send_a_bunch_of_posts()
        {
            using (var client = new HttpClient())
            {
                await client.PostAsync("http://localhost:5060/marten/clear", new StringContent(string.Empty));
                await client.PostAsync("http://localhost:5061/marten/clear", new StringContent(string.Empty));


                for (int i = 0; i < 20; i++)
                {
                    await client.PostAsync("http://localhost:5060/marten/one", new StringContent(string.Empty));
                    await client.PostAsync("http://localhost:5060/marten/two", new StringContent(string.Empty));
                    await client.PostAsync("http://localhost:5060/marten/three", new StringContent(string.Empty));
                    await client.PostAsync("http://localhost:5060/marten/four", new StringContent(string.Empty));
                }
            }
        }
    }
}
