using Jasper.TestSupport.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            // SAMPLE: adding-external-node
            var host = new JasperStorytellerHost<MyJasperAppRegistry>();
            host.AddNode(new OtherApp());

            return StorytellerAgent.Run(args, host);
            // ENDSAMPLE
            /*
            // SAMPLE: bootstrapping-storyteller-with-Jasper
            JasperStorytellerHost.Run<MyJasperAppRegistry>(args);
            // ENDSAMPLE
            */
        }
    }
}
