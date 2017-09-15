using System;
using Jasper.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "try")
            {
                using (var runner = StorytellerRunner.For<JasperStorytellerHost<MyJasperAppRegistry>>())
                {
                    runner.Run("Recording Messages / Try out the diagnostics");
                    runner.OpenResultsInBrowser();
                }


                return;
            }

            // SAMPLE: bootstrapping-storyteller-with-Jasper
            JasperStorytellerHost.Run<MyJasperAppRegistry>(args);
            // ENDSAMPLE
        }
    }
}
