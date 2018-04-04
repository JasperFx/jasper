using System;
using Jasper.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    class Program
    {
        static int Main(string[] args)
        {
            var host = new JasperStorytellerHost<MyJasperAppRegistry>();
            host.AddNode(new OtherApp());

            return StorytellerAgent.Run(args, host);

            /*
            // SAMPLE: bootstrapping-storyteller-with-Jasper
            JasperStorytellerHost.Run<MyJasperAppRegistry>(args);
            // ENDSAMPLE
            */
        }
    }
}
