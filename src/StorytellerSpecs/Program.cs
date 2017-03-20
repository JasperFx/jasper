using System;
using Baseline.Dates;
using StoryTeller;

namespace StorytellerSpecs
{
    internal class Program
    {
        public static void Debug()
        {
            using (var runner = StorytellerRunner.Basic())
            {
                // worry about the 2nd message here
                //var counts = runner.Run("Publishing / LQ / Send to a Specific Channel").Counts;

                // Error Handling / Retry on Exceptions

                //Console.WriteLine(counts);

                runner.RunAll(2.Minutes());
            }
        }

        public static void Main(string[] args)
        {
            StorytellerAgent.Run(args);
        }
    }
}