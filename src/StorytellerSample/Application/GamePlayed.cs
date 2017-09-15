using System;

namespace StorytellerSample.Application
{
    public class GamePlayed
    {
        public DateTime Date { get; set; }
        public TeamResult Home { get; set; }
        public TeamResult Visitor { get; set; }
    }
}