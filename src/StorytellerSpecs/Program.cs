using StoryTeller;
using StoryTeller.Engine;

namespace StorytellerSpecs
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return StorytellerAgent.Run<SpecSystem>(args);
        }
    }

    public class SpecSystem : SimpleSystem{}
}
