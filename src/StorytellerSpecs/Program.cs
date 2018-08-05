using System;
using System.Threading.Tasks;
using Servers.Commands;
using StoryTeller;

namespace StorytellerSpecs
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return StorytellerAgent.Run(args);
        }
    }
}
