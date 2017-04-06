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
                var counts = runner.Run("Publishing / LQ / Resiliency / Receive a garbled message that blows up in deserialization").Counts;
                //var counts = runner.Run("Publishing / LQ / Resiliency / Receive a message with an unknown content type").Counts;
                //var counts = runner.Run("Publishing / LQ / Resiliency / Receive an unhandled message type").Counts;

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