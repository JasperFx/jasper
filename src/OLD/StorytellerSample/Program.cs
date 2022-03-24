using Jasper.TestSupport.Storyteller;
using StoryTeller;

namespace StorytellerSample
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            #region sample_adding_external_node
            var host = new JasperStorytellerHost<MyJasperAppOptions>();
            host.AddNode(new OtherApp());

            return StorytellerAgent.Run(args, host);
            #endregion
            /*
            #region sample_bootstrapping_storyteller_with_Jasper
            JasperStorytellerHost.Run<MyJasperAppOptions>(args);
            #endregion
            */
        }
    }
}
