using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Oakton;

namespace RunLoadTests
{
    public class RunInput
    {
        [Description("Root Url of the sending application")]
        public string Url { get; set; }

        [Description("Duration of the test run")]
        public int Duration { get; set; } = 10;

        [Description("Number of active threads")]
        public int ThreadsFlag { get; set; } = 20;
    }

    public class RunCommand : OaktonCommand<RunInput>
    {
        public RunCommand()
        {
            Usage("Default").Arguments(x => x.Url);
            Usage("Timed").Arguments(x => x.Url, x => x.Duration);
        }

        public override bool Execute(RunInput input)
        {
            using (var client = new HttpClient())
            {
                var cancellation = new CancellationTokenSource();
                cancellation.CancelAfter(input.Duration.Seconds());

                var tasks = new List<Task>();

                for (int i = 0; i < input.ThreadsFlag; i++)
                {
                    tasks.Add(postInLoop(client, input, cancellation.Token));
                }


                Task.WaitAll(tasks.ToArray());
            }

            return true;
        }

        private async Task postInLoop(HttpClient client, RunInput input, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await client.PostAsync(input.Url + "/one", new StringContent(string.Empty));
                await client.PostAsync(input.Url + "/two", new StringContent(string.Empty));
                //await client.PostAsync(input.Url + "/three", new StringContent(string.Empty));
                await client.PostAsync(input.Url + "/four", new StringContent(string.Empty));
            }
        }
    }
}
