using System;
using StoryTeller;

namespace StorytellerSpecs
{
    internal class Program
    {
        public static void Debug()
        {
            using (var runner = StorytellerRunner.Basic())
            {
                var counts = runner.Run("Publishing / Send and Wait for an Acknowledgement").Counts;

                Console.WriteLine(counts);

                //runner.RunAll(2.Minutes());
            }
        }

        public static void Main(string[] args)
        {
            StorytellerAgent.Run(args);
        }
    }
}