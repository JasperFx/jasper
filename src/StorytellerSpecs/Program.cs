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
                var counts = runner.Run("Error Handling / Pure Happy Path").Counts;

                // Error Handling / Retry on Exceptions

                Console.WriteLine(counts);
            }
        }

        public static void Main(string[] args)
        {
            StorytellerAgent.Run(args);
        }
    }
}