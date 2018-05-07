using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RunLoadTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("About to start hammering on the sender and receiver");


            using (var client = new HttpClient())
            {
                Console.WriteLine("Clearing out existing persisted messages");
                await client.PostAsync("http://localhost:5060/marten/clear", new StringContent(string.Empty));
                await client.PostAsync("http://localhost:5061/marten/clear", new StringContent(string.Empty));


                var tasks = new List<Task>();


                for (int i = 0; i < 20; i++)
                {
                    tasks.Add(client.PostAsync("http://localhost:5060/one", new StringContent(string.Empty)));
                    tasks.Add(client.PostAsync("http://localhost:5060/two", new StringContent(string.Empty)));
                    tasks.Add(client.PostAsync("http://localhost:5060/three", new StringContent(string.Empty)));
                    tasks.Add(client.PostAsync("http://localhost:5060/four", new StringContent(string.Empty)));

                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
